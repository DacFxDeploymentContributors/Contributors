using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace EditionAwareCreateIndex
{
    public class IndexVisitior : TSqlFragmentVisitor
    {
        private bool _called;
        public List<AlterIndexStatement> Alters = new List<AlterIndexStatement>();
        public List<CreateIndexStatement> Creates = new List<CreateIndexStatement>();

        public bool ContainsIndexChange()
        {
            return _called;
        }

        public override void ExplicitVisit(CreateIndexStatement node)
        {
            _called = true;
            Creates.Add(node);
            base.ExplicitVisit(node);
        }

        public override void ExplicitVisit(AlterIndexStatement node)
        {
            Alters.Add(node);
            _called = true;
            base.ExplicitVisit(node);
        }
    }
}