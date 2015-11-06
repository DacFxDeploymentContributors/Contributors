using System;
using System.Diagnostics;
using System.IO;

namespace IntegrationTests.Framework
{
    internal static class DacpacDeploy
    {
        public static string Deploy(string dacpac, string server, string database, string outputPath = ".\\script.sql", string additionalDpeloymentArgs = "")
        {
            const string destinationFile = "sqlpackage\\StopDeploymentsOnBreakingProcedureChanges.dll";

            if (File.Exists(destinationFile))
            {
                File.Delete(destinationFile);
            }

            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }

            File.Copy(".\\StopDeploymentsOnBreakingProcedureChanges.dll", destinationFile);

            var args =
                string.Format(
                    "/action:script /sf:{0} /tsn:{1} /tdn:{2} /op:{3} /p:AllowIncompatiblePlatform=true /p:AdditionalDeploymentContributors=StopDeploymentsOnBreakingProcedureChanges /p:AdditionalDeploymentContributorArguments=zEasyDebugStopDeploymentsOnBreakingProcedureChanges=true;{4}",
                    dacpac, server, database, outputPath, additionalDpeloymentArgs);


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

           
            Console.WriteLine("Messages: " + s);

            return s;
        }
    }
}