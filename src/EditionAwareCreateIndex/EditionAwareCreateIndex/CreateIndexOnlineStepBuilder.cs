using System.Collections.Generic;
using System.IO;
using EditionAwareCreateIndex.CustomSteps;
using Microsoft.SqlServer.Dac.Deployment;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace EditionAwareCreateIndex
{
    internal class CreateIndexOnlineStepBuilder
    {
        private readonly SQLServerEdition _edition;

        public CreateIndexOnlineStepBuilder(SQLServerEdition edition)
        {
            _edition = edition;
        }

        public DeploymentStep Build(DeploymentStep originalStep)
        {
            if (!OriginalStepHasCreateIndex(originalStep))
                return null;

            switch (_edition)
            {
                case SQLServerEdition.Unknown:
                    return new CreateIndexDecideAtRuntime(originalStep);
                case SQLServerEdition.Standard:
                case SQLServerEdition.Enterprise:
                    return new CreateIndexDecideAtBuildTime(originalStep, _edition);
            }

            return null;
        }

        private bool OriginalStepHasCreateIndex(DeploymentStep originalStep)
        {
            var tsql = originalStep.GenerateTSQL();

            foreach (var script in tsql)
            {
                if (IsCreateOrAlterIndex(script))
                {
                    return true;
                }
            }

            return true;
        }

        private bool IsCreateOrAlterIndex(string script)
        {
            var parser = new TSql120Parser(true);
            IList<ParseError> errors;
            var fragment = parser.Parse(new StringReader(script), out errors);
            var visitor = new IndexVisitior();

            fragment.Accept(visitor);

            return visitor.ContainsIndexChange();
        }
    }
}