using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.SqlServer.Dac.Deployment;
using Microsoft.SqlServer.Dac.Extensibility;

namespace EditionAwareCreateIndex
{
    [ExportDeploymentPlanModifier(Name, Version)]
    public class DeploymentFilter : DeploymentPlanModifier
    {
        private const string Name = "EditionAwareCreateIndex";
        private const string Version = "0.1.0.0";

        protected override void OnExecute(DeploymentPlanContributorContext context)
        {
            if (context.Arguments.ContainsKey("EasyDebugEditionAwareCreateIndex"))
            {
                Debugger.Launch();
            }

            var stepBuilder = new CreateIndexOnlineStepBuilder(GetEdition(context.Arguments));

            try
            {
                Print("Starting...", Severity.Message);

                var next = context.PlanHandle.Head;
                while (next != null)
                {
                    var current = next;
                    next = current.Next;

                    if (current is CreateElementStep || current is AlterElementStep)
                    {
                        var replacementStep = stepBuilder.Build(current);

                        if (replacementStep != null)
                        {
                            Remove(context.PlanHandle, current);
                            AddBefore(context.PlanHandle, next, replacementStep);
                        }
                    }
                }

                Print("Finished", Severity.Message);
            }
            catch (Exception e)
            {
                Print(string.Format("Error running contributor, exception: {0}", e), Severity.Error);
            }
        }

        private SQLServerEdition GetEdition(Dictionary<string, string> arguments)
        {
            const string forceEditionKey = "EditionAwareCreateIndexForceEdition";

            if (arguments.ContainsKey(forceEditionKey))
            {
                SQLServerEdition edition;
                if (Enum.TryParse(arguments[forceEditionKey], true, out edition))
                {
                    return edition;
                }
            }

            return SQLServerEdition.Unknown; //default batch size
        }

        private void Print(string message, Severity severity)
        {
            PublishMessage(new ExtensibilityError(string.Format("{0}: {1}", Name, message), severity));
        }
    }

    public enum SQLServerEdition
    {
        Unknown,
        Standard,
        Enterprise
    }
}