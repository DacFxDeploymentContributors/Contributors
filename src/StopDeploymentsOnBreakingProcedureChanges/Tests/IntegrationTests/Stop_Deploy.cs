using System;
using System.IO;
using IntegrationTests.Framework;
using NUnit.Framework;

namespace IntegrationTests
{
    [TestFixture]
    public class Stop_Deploy
    {
        [TestFixtureSetUp]
        public void Setup()
        {
            Database.CleanOrCreateDatabase();
        }

        [Test]
        public void When_Parameter_Deleted()
        {
            var messages = DacpacDeploy.Deploy(@"..\..\..\TestDacpacDeploy\bin\Debug\TestDacpacDeploy.dacpac", Database.server_name, Database.db_name);

            Assert.IsTrue(messages.Contains(@"StopDeploymentsOnBreakingProcedureChanges: The procedure [dbo].[TestoRemoveParameter] has had a parameter removed, parameter name: [dbo].[TestoRemoveParameter].[@jj]"));

        }

        [Test]
        public void When_Parameter_Add_With_No_Default()
        {
            var messages = DacpacDeploy.Deploy(@"..\..\..\TestDacpacDeploy\bin\Debug\TestDacpacDeploy.dacpac", Database.server_name, Database.db_name);
            Assert.IsTrue(messages.Contains(@"StopDeploymentsOnBreakingProcedureChanges: The procedure [dbo].[TestoAddParameterNoDefault] has had an additional parameter but no default, parameter name: [dbo].[TestoAddParameterNoDefault].[@b]"));
        }

        [Test]
        public void When_Parameter_Default_Remoced()
        {
            var messages = DacpacDeploy.Deploy(@"..\..\..\TestDacpacDeploy\bin\Debug\TestDacpacDeploy.dacpac", Database.server_name, Database.db_name);
            Assert.IsTrue(messages.Contains(@"StopDeploymentsOnBreakingProcedureChanges: The procedure [dbo].[TestoRemoveParameter] has had a parameter removed, parameter name: [dbo].[TestoRemoveParameter].[@jj]"));

        }
    }
}