using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgileSqlClub.BatchedTableMigration;
using Microsoft.SqlServer.Dac.Deployment;
using Moq;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class BatchedSqlTableMigrationStep_Tests
    {

        [Test]
        public void Replaces_Insert_With_Batch_Insert()
        {
            
            var steps = new List<string>() {"PRINT N'Starting rebuilding table [dbo].[ForcedTableMigration]...';\r\n\r\n"
,
@"BEGIN TRANSACTION;

SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

SET XACT_ABORT ON;

CREATE TABLE [dbo].[tmp_ms_xx_ForcedTableMigration] (
    [Id]    INT          NOT NULL,
    [Name]  VARCHAR (25) NOT NULL,
    [count] INT          NOT NULL,
    [total] AS           [Id] * [count],
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

IF EXISTS (SELECT TOP 1 1 
           FROM   [dbo].[ForcedTableMigration])
    BEGIN
        INSERT INTO [dbo].[tmp_ms_xx_ForcedTableMigration] ([count])
        SELECT [count]
        FROM   [dbo].[ForcedTableMigration];
    END

DROP TABLE [dbo].[ForcedTableMigration];

EXECUTE sp_rename N'[dbo].[tmp_ms_xx_ForcedTableMigration]', N'ForcedTableMigration';

COMMIT TRANSACTION;

SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
"};


            var originalStep = new Mock<DeploymentStep>();
            originalStep.Setup(p => p.GenerateTSQL()).Returns(steps);

            var batchedStep = new BatchedSqlTableMigrationStep(originalStep.Object);

            var actual = batchedStep.GenerateTSQL();
            Assert.IsTrue(actual.Last().Contains("select count(*) from [dbo].[ForcedTableMigration]"));
        }

    }
}
