using System.Collections.Generic;
using Microsoft.SqlServer.Dac.Deployment;

namespace EditionAwareCreateIndex.CustomSteps
{
    public class IndexModifierStep : DeploymentStep
    {
        private readonly DeploymentStep _originalStep;

        public IndexModifierStep(DeploymentStep originalStep)
        {
            _originalStep = originalStep;
        }

        public override IList<string> GenerateTSQL()
        {
            var newScripts = new List<string>();

            foreach (var script in _originalStep.GenerateTSQL())
            {
                var replacement = GetReplacementScript(script);
                if (string.IsNullOrEmpty(replacement))
                {
                    newScripts.Add(script);
                }
                else
                {
                    newScripts.Add(replacement);
                }
            }

            return newScripts;
        }

        protected virtual string GetReplacementScript(string script)
        {
            return null;
        }
    }
}