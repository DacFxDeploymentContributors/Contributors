using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.SqlServer.Dac.Deployment;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace EditionAwareCreateIndex.CustomSteps
{
    internal class CreateIndexDecideAtBuildTime : IndexModifierStep
    {
        private readonly SQLServerEdition _edition;

        public CreateIndexDecideAtBuildTime(DeploymentStep originalStep, SQLServerEdition edition) : base(originalStep)
        {
            _edition = edition;
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
                var newCreate = GetReplacementCreate(create);
                var generator = new Sql120ScriptGenerator();
                string newStatement;
                generator.GenerateScript(newCreate, out newStatement);

                newScript = newScript.Replace(script.Substring(fragment.StartOffset, fragment.FragmentLength),
                    newStatement);
            }

            return newScript;
        }

        private TSqlFragment GetReplacementCreate(CreateIndexStatement create)
        {
            if (_edition == SQLServerEdition.Enterprise)
                return AddOnline(create);
            return RemoveOnline(create);
        }

        private TSqlFragment AddOnline(CreateIndexStatement create)
        {
            var onlineOption =
                (create.IndexOptions.FirstOrDefault(p => p.OptionKind == IndexOptionKind.Online)) as OnlineIndexOption;

            if (onlineOption == null)
            {
                create.IndexOptions.Add(new OnlineIndexOption
                {
                    OptionKind = IndexOptionKind.Online,
                    OptionState = OptionState.On
                });
                return create;
            }

            if (onlineOption.OptionState == OptionState.Off || onlineOption.OptionState == OptionState.NotSet)
            {
                onlineOption.OptionState = OptionState.On;
            }

            return create;
        }

        private TSqlFragment RemoveOnline(CreateIndexStatement create)
        {
            var onlineOption =
                (create.IndexOptions.FirstOrDefault(p => p.OptionKind == IndexOptionKind.Online)) as OnlineIndexOption;
            if (onlineOption == null)
                return create;

            onlineOption.OptionState = OptionState.Off;
            return create;
        }
    }
}