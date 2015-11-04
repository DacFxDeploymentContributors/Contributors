using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Dac.Deployment;
using QueryExpression = System.Web.UI.WebControls.Expressions.QueryExpression;

namespace AgileSqlClub.BatchedTableMigration
{
    public class MigrationStepBatcher
    {
        private readonly SqlTableMigrationStep _step;
        private readonly int _rowCount;

        public MigrationStepBatcher(SqlTableMigrationStep step, int rowCount)
        {
            _step = step;
            _rowCount = rowCount;
        }

        public BatchedSqlTableMigrationStep BatchStep()
        {
            var batchedInsert = new BatchedSqlTableMigrationStep(_step, _rowCount);

            return batchedInsert;
        }

    }
}
