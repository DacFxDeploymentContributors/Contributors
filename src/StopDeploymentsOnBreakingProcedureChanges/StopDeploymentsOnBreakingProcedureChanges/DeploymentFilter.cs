using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.SqlServer.Dac.Deployment;
using Microsoft.SqlServer.Dac.Extensibility;
using Microsoft.SqlServer.Dac.Model;

namespace StopDeploymentsOnBreakingProcedureChanges
{
    [ExportDeploymentPlanModifier(Name, Version)]
    public class DeploymentFilter : DeploymentPlanModifier
    {
        private const string Name = "StopDeploymentsOnBreakingProcedureChanges";
        private const string Version = "0.1.0.0";

        protected override void OnExecute(DeploymentPlanContributorContext context)
        {
            if (context.Arguments.ContainsKey("EasyDebugStopDeploymentsOnBreakingProcedureChanges"))
            {
                Debugger.Launch();
            }

            try
            {
                Print("Starting...", Severity.Message);

                foreach (var changeDefinition in context.ComparisonResult.ElementsChanged.Keys)
                {
                    var change = context.ComparisonResult.ElementsChanged[changeDefinition];

                    if (change.TargetObject.ObjectType == ModelSchema.Procedure)
                    {
                        VerifyStoredProcedureChange(context, change);
                    }
                }


                Print("Finished", Severity.Message);
            }
            catch (Exception e)
            {
                Print(string.Format("Error running contributor, exception: {0}", e), Severity.Error);
            }
        }

        private void VerifyStoredProcedureChange(DeploymentPlanContributorContext context,
            ModelComparisonChangeDefinition change)
        {
            var newProcedure = context.Source.GetObject(ModelSchema.Procedure, change.TargetObject.Name,
                DacQueryScopes.UserDefined);
            var oldProcedure = context.Target.GetObject(ModelSchema.Procedure, change.TargetObject.Name,
                DacQueryScopes.UserDefined);

            if (newProcedure == null)
                return;

            foreach (var parameter in  newProcedure.GetReferencedRelationshipInstances(Procedure.Parameters))
            {
                if (HasParameterBeenAddedWithoutDefaultValue(parameter, oldProcedure))
                {
                    //Severity.Error - fails the build process
                    Print(
                        string.Format(
                            "The procedure {0} has had an additional parameter but no default, parameter name: {1}",
                            change.TargetObject.Name, parameter.ObjectName), Severity.Error);
                }

                if (HasDefaultValueBeenRemoved(parameter, oldProcedure))
                {
                    Print(
                        string.Format("The procedure {0} has had a parameter default value removed, parameter name: {1}", change.TargetObject.Name, parameter.ObjectName), Severity.Error);
                }

            }

            foreach (var parameter in oldProcedure.GetReferencedRelationshipInstances(Procedure.Parameters))
            {
                if (HasParameterBeenRemoved(parameter, newProcedure))
                {
                    Print(
                        string.Format("The procedure {0} has had a parameter removed, parameter name: {1}",
                            change.TargetObject.Name, parameter.ObjectName), Severity.Error);
                }
            }



        }

        private bool HasDefaultValueBeenRemoved(ModelRelationshipInstance parameter, TSqlObject oldProcedure)
        {
            var newParameterHasDefault = parameter.Object.GetProperty(Parameter.DefaultExpression) != null;

            var parameterInOldModel =
                oldProcedure.GetReferencedRelationshipInstances(Procedure.Parameters)
                    .FirstOrDefault(p => p.ObjectName.Parts.Last() == parameter.ObjectName.Parts.Last());

            /* 
                A parameter in the new model has no default but it had a default in the old model...
            */

            if (parameterInOldModel != null && !newParameterHasDefault && (parameterInOldModel.Object.GetProperty(Parameter.DefaultExpression) != null))
            {
                return true;
            }

            return false;
        }

        private static bool HasParameterBeenAddedWithoutDefaultValue(ModelRelationshipInstance parameter,
            TSqlObject newProcedure)
        {
            var hasDefault = parameter.Object.GetProperty(Parameter.DefaultExpression) != null;

            var parameterInOldModel =
                newProcedure.GetReferencedRelationshipInstances(Procedure.Parameters)
                    .FirstOrDefault(p => p.ObjectName.Parts.Last() == parameter.ObjectName.Parts.Last());

            /*
                A parameter did not exist in the old model and has no default
            */

            if (parameterInOldModel == null && !hasDefault)
            {
                return true;
            }

            return false;
        }

        private static bool HasParameterBeenRemoved(ModelRelationshipInstance parameter, TSqlObject oldProcedure)
        {
            /*
                A parameter does not exist in the old model
            */

            return
                oldProcedure.GetReferencedRelationshipInstances(Procedure.Parameters)
                    .FirstOrDefault(p => p.ObjectName.Parts.Last() == parameter.ObjectName.Parts.Last()) == null;
        }

        private void Print(string message, Severity severity)
        {
            PublishMessage(new ExtensibilityError(string.Format("{0}: {1}", Name, message), severity));
        }
    }
}