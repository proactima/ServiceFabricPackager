using System;
using System.Diagnostics;

namespace SFPackager.Services
{
    public class AspNetCorePackager
    {
        private readonly ConsoleWriter _log;

        public AspNetCorePackager(ConsoleWriter log)
        {
            _log = log;
        }

        public int Package(string projectFolder, string codeTargetFolder, string configuration)
        {
            projectFolder = projectFolder.TrimEnd('\\');
            var processInfo = new ProcessStartInfo
            {
                Arguments =
                    $"publish \"{projectFolder}\" --output \"{codeTargetFolder}\" --configuration {configuration}",
                FileName = "dotnet.exe",
                CreateNoWindow = false,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };

            int exitCode;
            using (var process = Process.Start(processInfo))
            {
                _log.WriteLine(process.StandardOutput.ReadToEnd());
                process.WaitForExit();
                exitCode = process.ExitCode;
            }

            return exitCode;
        }
    }
}