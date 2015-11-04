using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.SqlServer.Dac.Deployment;
using Microsoft.SqlServer.Dac.Extensibility;

namespace AgileSqlClub.BatchedTableMigration
{
    [ExportDeploymentPlanModifier(Name, Version)]
    public class DeploymentFilter : DeploymentPlanModifier
    {
        private const string Name = "AgileSqlClub.BatchedTableMigration";
        private const string Version = "0.1.0.0";

        protected override void OnExecute(DeploymentPlanContributorContext context)
        {
            if (context.Arguments.ContainsKey("EasyDebugBatchedTableMigration"))
            {
                MessageBox.Show("Breaking to let you attach a debugger");
            }

            var rowCount = GetRowCount(context.Arguments);
            
            try
            {
                Print("Starting...", Severity.Message);

                var next = context.PlanHandle.Head;
                while (next != null)
                {
                    var current = next;
                    next = current.Next;

                    if (current is SqlTableMigrationStep)
                    {
                        var batched = new MigrationStepBatcher(current as SqlTableMigrationStep, rowCount).BatchStep();

                        Remove(context.PlanHandle, current);
                        AddBefore(context.PlanHandle, next, batched);
                    }
                }

                Print("Finished", Severity.Message);
            }
            catch (Exception e)
            {
                Print(string.Format("Error running contributor, exception: {0}", e), Severity.Error);
            }
        }

        private int GetRowCount(Dictionary<string, string> arguments)
        {
            const string batchSizeKey = "BatchedTableMigrationBatchSize";

            if (arguments.ContainsKey(batchSizeKey))
            {
                var batchSize = 0;

                if (!int.TryParse(arguments[batchSizeKey], out batchSize))
                {
                    throw new InvalidCastException(
                        string.Format("The value in the argument {0} could not be converted to an int, value = {1}",
                            batchSizeKey, arguments[batchSizeKey]));
                }

                return batchSize;
            }

            return 1000; //default batch size
        }

        private void Print(string message, Severity severity)
        {
            PublishMessage(new ExtensibilityError(string.Format("{0}: {1}", Name, message), severity));
        }
    }
}