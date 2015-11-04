using System;
using System.Diagnostics;
using System.IO;

namespace IntegrationTests.Framework
{
    internal static class DacpacDeploy
    {
        public static void Deploy(string dacpac, string server, string database, string outputPath)
        {
            const string destinationFile = "sqlpackage\\AgileSqlClub.BatchedTableMigration.dll";

            if (File.Exists(destinationFile))
            {
                File.Delete(destinationFile);
            }

            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }

            File.Copy(".\\AgileSqlClub.BatchedTableMigration.dll", destinationFile);

            var args =
                string.Format(
                    "/action:script /sf:{0} /tsn:{1} /tdn:{2} /op:{3} /p:AllowIncompatiblePlatform=true /p:AdditionalDeploymentContributors=AgileSqlClub.BatchedTableMigration /p:AdditionalDeploymentContributorArguments=BatchedTableMigrationBatchSize=1480;",
                    dacpac, server, database, outputPath);


            var startupInfo = new ProcessStartInfo();
            startupInfo.FileName = "sqlpackage\\sqlpackage.exe";
            startupInfo.Arguments = args;
            startupInfo.RedirectStandardError = true;
            startupInfo.RedirectStandardOutput = true;
            startupInfo.UseShellExecute = false;

            var p = Process.Start(startupInfo);
            p.WaitForExit();

            var s = "StdError: \r\n" + p.StandardError.ReadToEnd();
            s += "\r\nStdOut: \r\n" + p.StandardOutput.ReadToEnd();

            if (p.ExitCode != 0)
            {
                throw new Exception(string.Format("sqlpackage.exe failed: exit code: {1} messages: {0}", s, p.ExitCode));
            }

            Console.WriteLine("Messages: " + s);
        }
    }
}