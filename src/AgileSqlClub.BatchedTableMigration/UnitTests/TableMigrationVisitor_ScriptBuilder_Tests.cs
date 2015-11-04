using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class TableMigrationVisitor_ScriptBuilder_Tests
    {
        [Test]
        public void Select_Query_Is_Converted_Into_Select_Count_Star()
        {

            var original = new QuerySpecification();
            original.FromClause = new FromClause();
            original.FromClause.TableReferences.Add(new NamedTableReference()
            {
                SchemaObject = new SchemaObjectName()
                {
                    BaseIdentifier = { Value = "Bert"}
                }
            });

            var selectCol1 = new ColumnReferenceExpression();
            selectCol1.MultiPartIdentifier.Identifiers.Add(new Identifier()
            {
                Value = "Bertram"
            });
            


            //original.SelectElements.Add(new ColumnReferenceExpression());



        }


    }
}
