using System.Collections.Generic;
using System.IO;
using Microsoft.SqlServer.Dac.Deployment;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace EditionAwareCreateIndex.CustomSteps
{
    public class CreateIndexDecideAtRuntime : IndexModifierStep
    {
        public CreateIndexDecideAtRuntime(DeploymentStep originalStep) : base(originalStep)
        {
        }

        protected override string GetReplacementScript(string script)
        {
            var parser = new TSql120Parser(true);
            IList<ParseError> errors;
            var fragment = parser.Parse(new StringReader(script), out errors);
            var visitor = new IndexVisitior();

            fragment.Accept(visitor);
            var newScript = script;

            foreach (var create in visitor.Creates)
            {
                var newCreate = GenerateCreateWithEditionCheck(create);
                var generator = new Sql120ScriptGenerator();
                string newStatement;
                generator.GenerateScript(newCreate, out newStatement);

                newScript = newScript.Replace(
                    script.Substring(fragment.StartOffset, fragment.FragmentLength), newStatement);
            }

            return newScript;
        }

        private IfStatement GenerateCreateWithEditionCheck(CreateIndexStatement create)
        {
            var ifWrapper = new IfStatement();


            ifWrapper.Predicate = GetIsNotNull();

            var modifiedCreate = GetModifiedCreate(create);
            ifWrapper.ThenStatement = WrapInBeginEnd(modifiedCreate);
            ifWrapper.ElseStatement = WrapInBeginEnd(create);


            return ifWrapper;
        }

        private TSqlStatement WrapInBeginEnd(TSqlStatement fragment)
        {
            var beginEnd = new BeginEndBlockStatement();
            beginEnd.StatementList = new StatementList();

            beginEnd.StatementList.Statements.Add(fragment);
            return beginEnd;
        }

        private CreateIndexStatement GetModifiedCreate(CreateIndexStatement create)
        {
            var modifiedCreate = new CreateIndexStatement();


            foreach (var c in create.Columns)
            {
                modifiedCreate.Columns.Add(
                    new ColumnWithSortOrder
                    {
                        Column = c.Column,
                        SortOrder = c.SortOrder
                    });
            }

            foreach (var c in create.IncludeColumns)
            {
                modifiedCreate.IncludeColumns.Add(
                    new ColumnReferenceExpression
                    {
                        ColumnType = c.ColumnType,
                        MultiPartIdentifier = c.MultiPartIdentifier
                        ,
                        Collation = c.Collation
                    });
            }

            modifiedCreate.Name = create.Name;
            modifiedCreate.OnName = create.OnName;

            modifiedCreate.Clustered = create.Clustered;
            modifiedCreate.FileStreamOn = create.FileStreamOn;
            modifiedCreate.FilterPredicate = create.FilterPredicate;
            modifiedCreate.OnFileGroupOrPartitionScheme = create.OnFileGroupOrPartitionScheme;
            modifiedCreate.Translated80SyntaxTo90 = create.Translated80SyntaxTo90;
            modifiedCreate.Unique = create.Unique;

            foreach (var option in create.IndexOptions)
            {
                modifiedCreate.IndexOptions.Add(option);
            }

            modifiedCreate.IndexOptions.Add(new OnlineIndexOption
            {
                OptionKind = IndexOptionKind.Online,
                OptionState = OptionState.On
            });

            return modifiedCreate;
        }

        private BooleanIsNullExpression GetIsNotNull()
        {
            var isNull = new BooleanIsNullExpression();
            isNull.IsNot = true;
            var query = (isNull.Expression = new ScalarSubquery()) as ScalarSubquery;
            var spec = (query.QueryExpression = new QuerySpecification()) as QuerySpecification;
            spec.SelectElements.Add(new SelectStarExpression());
            var fromTable = new QueryDerivedTable();
            spec.FromClause = new FromClause();
            spec.FromClause.TableReferences.Add(fromTable);
            var subQuerySpec = new QuerySpecification();
            var version = new GlobalVariableExpression();
            version.Name = "@@version";

            var columnName = new IdentifierOrValueExpression();
            //var v = (columnName.ValueExpression = new StringLiteral()) as StringLiteral;
            //v.Value = "v";
            columnName.Identifier = new Identifier();
            columnName.Identifier.Value = "v";


            subQuerySpec.SelectElements.Add(new SelectScalarExpression {Expression = version, ColumnName = columnName});
            fromTable.QueryExpression = subQuerySpec;
            fromTable.Alias = new Identifier {Value = "edition"};

            spec.WhereClause = new WhereClause();
            var likePredicate = (spec.WhereClause.SearchCondition = new LikePredicate()) as LikePredicate;
            var col = (likePredicate.FirstExpression = new ColumnReferenceExpression()) as ColumnReferenceExpression;
            col.ColumnType = ColumnType.Regular;
            col.MultiPartIdentifier = new MultiPartIdentifier();
            col.MultiPartIdentifier.Identifiers.Add(new Identifier {Value = "v"});
            var ver = (likePredicate.SecondExpression = new StringLiteral()) as StringLiteral;
            ver.Value = "%Enterprise%";

            return isNull;
        }
    }
}