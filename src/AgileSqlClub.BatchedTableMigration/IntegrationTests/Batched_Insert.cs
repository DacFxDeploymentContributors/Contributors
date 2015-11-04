using System;
using System.IO;
using IntegrationTests.Framework;
using NUnit.Framework;

namespace IntegrationTests
{
    [TestFixture]
    public class Batched_Insert
    {
        [TestFixtureSetUp]
        public void Setup()
        {
            Database.CleanOrCreateDatabase();
        }

        [Test]
        public void Insert_Is_Changed_To_Batched_Insert()
        {
            const string scriptFile = @".\script.sql";

            DacpacDeploy.Deploy(@"..\..\..\TestDacpacDeploy\bin\Debug\TestDacpacDeploy.dacpac", Database.server_name,
                Database.db_name, scriptFile);
            var script = File.ReadAllText(scriptFile);

            Console.WriteLine(script);
            Assert.IsTrue(script.Contains(@"WHILE (SELECT count(*)
       FROM   [dbo].[ForcedTableMigration]) > 0
    BEGIN
        WITH to_delete
        AS   (SELECT TOP 1480 [count]
              FROM   [dbo].[ForcedTableMigration])
        DELETE to_delete
        OUTPUT deleted.* INTO [dbo].[tmp_ms_xx_ForcedTableMigration] ([count]);
    END
    END"));
        }
    }
}