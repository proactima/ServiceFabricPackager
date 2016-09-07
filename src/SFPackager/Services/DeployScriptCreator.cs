using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SFPackager.Models;

namespace SFPackager.Services
{
    public class DeployScriptCreator
    {
        private const string VersionIdentifier = "##VERSION##";
        private const string BasePackagePathIdentifier = "##BASEPATH##";
        private const string PackagesIdentifier = "##PACKAGES##";
        private const string DeployScriptName = "Deploy-Script.ps1";

        public void Do(VersionNumber newVersion, DirectoryInfo basePackagePath, List<string> packages)
        {
            var selfAssembly = System.Reflection.Assembly.GetEntryAssembly();
            var assemblyPath = Path.GetDirectoryName(selfAssembly.Location);
            var srcScriptLocation = Path.Combine(assemblyPath, DeployScriptName);

            var deployScriptTemplate = new FileInfo(srcScriptLocation);

            var packageList = packages
                .Select(x => $"\"{x}\"")
                .Aggregate((current, next) => current + "," + next);

            var deployScript = File
                .ReadAllText(deployScriptTemplate.FullName)
                .Replace(VersionIdentifier, $"\"{newVersion}\"")
                .Replace(BasePackagePathIdentifier, $"\"{basePackagePath.FullName}\"")
                .Replace(PackagesIdentifier, packageList);

            var outDeployScript = Path.Combine(basePackagePath.FullName, DeployScriptName);
            File.WriteAllText(outDeployScript, deployScript, Encoding.UTF8);
        }
    }
}