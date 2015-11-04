using System.Collections.Generic;
using System.IO;
using Microsoft.SqlServer.Dac.Deployment;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace AgileSqlClub.BatchedTableMigration
{
    public class BatchedSqlTableMigrationStep : DeploymentStep
    {
        private readonly int _rowCount;
        private readonly DeploymentStep _wrappedStep;
        
        public BatchedSqlTableMigrationStep(DeploymentStep wrappedStep, int rowCount)
        {
            _wrappedStep = wrappedStep;
            _rowCount = rowCount;
        }

        public override IList<string> GenerateTSQL()
        {
            var steps = _wrappedStep.GenerateTSQL();

            steps = ModifySteps(steps);

            return steps;
        }

        private IList<string> ModifySteps(IList<string> steps)
        {
            var parser = new TSql120Parser(true);
            var modifySteps = new List<string>();

            foreach (var step in steps)
            {
                IList<ParseError> errors;
                var script = parser.Parse(new StringReader(step), out errors);
                var visitor = new TableMigrationVisitor(_rowCount.ToString());
                script.Accept(visitor);

                if (string.IsNullOrEmpty(visitor.NewStatement.NewText))
                {
                    modifySteps.Add(step);
                }
                else
                {
                    var newStep = step.Replace(
                        step.Substring(visitor.NewStatement.StartPos, visitor.NewStatement.Length),
                        visitor.NewStatement.NewText);

                    modifySteps.Add(newStep);
                }
            }


            return modifySteps;
        }
    }
}