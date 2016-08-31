using System;
using System.Diagnostics;

namespace SFPackager.Services
{
    public class AspNetCorePackager
    {
        public int Package(string projectFolder, string codeTargetFolder, string configuration)
        {
            projectFolder = projectFolder.TrimEnd('\\');
            var processInfo = new ProcessStartInfo
            {
                Arguments =
                    $"publish \"{projectFolder}\" --output \"{codeTargetFolder}\" --configuration {configuration} --no-build",
                FileName = "dotnet.exe",
                CreateNoWindow = false,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };

            int exitCode;
            using (var process = Process.Start(processInfo))
            {
                Console.WriteLine(process.StandardOutput.ReadToEnd());
                process.WaitForExit();
                exitCode = process.ExitCode;
            }

            return exitCode;
        }
    }
}