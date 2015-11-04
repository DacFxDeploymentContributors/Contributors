using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace AgileSqlClub.BatchedTableMigration
{
    public class TableMigrationVisitor : TSqlFragmentVisitor
    {
        private readonly string _rowCount;
        public NewStatementContext NewStatement;

        public TableMigrationVisitor(string rowCount)
        {
            _rowCount = rowCount;
        }

        public override void ExplicitVisit(InsertStatement node)
        {
            base.ExplicitVisit(node);

            NewStatement.StartPos = node.StartOffset;
            NewStatement.Length = node.FragmentLength;

            var whyle = BuildWhileStatement(node.InsertSpecification);


            var scriptGen = new Sql110ScriptGenerator();
            string script;
            scriptGen.GenerateScript(whyle, out script);
            //Console.WriteLine(script);

            NewStatement.NewText = script;
        }

        /*
            go from:
            
            insert into dbo.table(columns...)
            select top X from dbo.another_table(columns...)

            to:

            
            while (select count(*) from dbo.table) > 0
            begin            
                with to_delete(
                    select top X from dbo.another_table(columns...)                
                )
                delete to_delete.* into dbo.table(columns...)               
            end
            
        */

        private WhileStatement BuildWhileStatement(InsertSpecification originalInsert)
        {
            var whyle = new WhileStatement();
            whyle.Predicate =
                BuildWhilePredicate(
                    BuildSelectSubQuery((originalInsert.InsertSource as SelectInsertSource).Select as QuerySpecification));
            whyle.Statement = BuildBeginEndWrappedBody(originalInsert);
            return whyle;
        }

        private BooleanComparisonExpression BuildWhilePredicate(QuerySpecification subQuery)
        {
            var query = new BooleanComparisonExpression();
            query.ComparisonType = BooleanComparisonType.GreaterThan;
            var scalarQuery = (query.FirstExpression = new ScalarSubquery()) as ScalarSubquery;
            scalarQuery.QueryExpression = subQuery;

            query.SecondExpression = new IntegerLiteral
            {
                Value = "0"
            };

            return query;
        }

        private BeginEndBlockStatement BuildBeginEndWrappedBody(InsertSpecification originalInsert)
        {
            var block = new BeginEndBlockStatement();

            block.StatementList = new StatementList();
            //block.StatementList.Statements.Add(BuildInsertWithTop(originalInsert));
            block.StatementList.Statements.Add(BuildDelete(originalInsert));


            return block;
        }

        private TSqlStatement BuildDelete(InsertSpecification originalInsert)
        {
            var delete = new DeleteStatement();

            delete.WithCtesAndXmlNamespaces = new WithCtesAndXmlNamespaces();
            var cte = new CommonTableExpression();
            
            cte.ExpressionName = new Identifier()
            {
                Value = "to_delete"
            };

            cte.QueryExpression = BuildNewRowSource(originalInsert);
            delete.WithCtesAndXmlNamespaces.CommonTableExpressions.Add(cte);

            delete.DeleteSpecification = new DeleteSpecification();
            var tableName = new SchemaObjectName();
            tableName.Identifiers.Add( new Identifier()
            {
                Value = "to_delete"
            });

            delete.DeleteSpecification.Target = new NamedTableReference() {SchemaObject = tableName };
            var outputInto = delete.DeleteSpecification.OutputIntoClause = new OutputIntoClause();

            var deletedTable = new MultiPartIdentifier();
            deletedTable.Identifiers.Add(new Identifier()
            {
                Value = "deleted"
            
            });

            outputInto.SelectColumns.Add(new SelectStarExpression()
            {
                Qualifier = deletedTable
            });

            outputInto.IntoTable = originalInsert.Target;
            foreach (var col in originalInsert.Columns)
            {
                outputInto.IntoTableColumns.Add(col);
            }

            return delete;
        }

        private QuerySpecification BuildNewRowSource(InsertSpecification originalInsert)
        {
            var specification = (originalInsert.InsertSource as SelectInsertSource).Select as QuerySpecification;

            specification.TopRowFilter = new TopRowFilter
            {
                Expression = new IntegerLiteral
                {
                    Value = _rowCount
                }
            };

            return specification;
        }

        private TSqlStatement BuildInsertWithTop(InsertSpecification originalInsert)
        {
            ((originalInsert.InsertSource as SelectInsertSource).Select as QuerySpecification).TopRowFilter = new TopRowFilter
            {
                Expression = new IntegerLiteral
                {
                    Value = _rowCount
                }
            };

            return new InsertStatement
            {
                InsertSpecification = originalInsert
            };
        }

        private QuerySpecification BuildSelectSubQuery(QuerySpecification originalSelectQuery)
        {
            var subQuerySelect = new QuerySpecification();
            var countStar = new FunctionCall();
            countStar.FunctionName = new Identifier {Value = "count"};
            countStar.Parameters.Add(
                    new ColumnReferenceExpression
                        {
                            ColumnType = ColumnType.Wildcard
                        }
                );

            subQuerySelect.SelectElements.Add(new SelectScalarExpression()
            {
                Expression = countStar
            });

            subQuerySelect.FromClause = originalSelectQuery.FromClause;
            return subQuerySelect;
        }
    }
}