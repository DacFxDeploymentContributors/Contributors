using System;
using System.IO;
using IntegrationTests.Framework;
using NUnit.Framework;

namespace IntegrationTests
{
    [TestFixture]
    public class Create_Index
    {
        [TestFixtureSetUp]
        public void Setup()
        {
            Database.CleanOrCreateDatabase();
        }

        [Test]
        public void Create_Index_Is_Wrapped_In_Edition_Check()
        {
            const string scriptFile = @".\script.sql";

            DacpacDeploy.Deploy(@"..\..\..\TestDacpacDeploy\bin\Debug\TestDacpacDeploy.dacpac", Database.server_name, Database.db_name, scriptFile);
            var script = File.ReadAllText(scriptFile);

            Console.WriteLine(script);
            Assert.IsTrue(script.Contains(@"IF (SELECT *
    FROM   (SELECT @@version AS v) AS edition
    WHERE  v LIKE '%Enterprise%') IS NOT NULL
    BEGIN
        CREATE NONCLUSTERED INDEX [ix_on_or_offline]
            ON [dbo].[CreateIndexTable]([Name] ASC) WITH (ONLINE = ON);
    END
ELSE
    BEGIN
        CREATE NONCLUSTERED INDEX [ix_on_or_offline]
            ON [dbo].[CreateIndexTable]([Name] ASC);
    END
GO"));

        }

        [Test]
        public void Create_Index_Has_Online_Enabled_When_Edition_Set_To_Enterprise()
        {
            const string scriptFile = @".\script.sql";

            DacpacDeploy.Deploy(@"..\..\..\TestDacpacDeploy\bin\Debug\TestDacpacDeploy.dacpac", Database.server_name, Database.db_name, scriptFile, "EditionAwareCreateIndexForceEdition=Enterprise");
            var script = File.ReadAllText(scriptFile);

            Console.WriteLine(script);
            Assert.IsTrue(script.Contains(@"CREATE NONCLUSTERED INDEX [ix_on_or_offline]
    ON [dbo].[CreateIndexTable]([Name] ASC) WITH (ONLINE = ON)
GO"));

        }

        [Test]
        public void Create_Index_Has_Online_Disabled_When_Edition_Set_To_Standard()
        {

            const string scriptFile = @".\script.sql";

            DacpacDeploy.Deploy(@"..\..\..\TestDacpacDeploy\bin\Debug\TestDacpacDeploy.dacpac", Database.server_name, Database.db_name, scriptFile, "EditionAwareCreateIndexForceEdition=Standard");
            var script = File.ReadAllText(scriptFile);

            Console.WriteLine(script);
            Assert.IsTrue(script.Contains(@"CREATE NONCLUSTERED INDEX [ix_on_or_offline]
    ON [dbo].[CreateIndexTable]([Name] ASC)
GO"));

        }
    }
}