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
        private readonly AppConfig _appConfig;

        public DeployScriptCreator(AppConfig appConfig)
        {
            _appConfig = appConfig;
        }

        public void Do(VersionNumber newVersion, List<string> packages)
        {
            var srcScriptLocation = Path.Combine(_appConfig.SelfPath.FullName, DeployScriptName);

            var deployScriptTemplate = new FileInfo(srcScriptLocation);

            var packageList = packages
                .Select(x => $"\"{x}\"")
                .Aggregate((current, next) => current + "," + next);

            var deployScript = File
                .ReadAllText(deployScriptTemplate.FullName)
                .Replace(VersionIdentifier, $"\"{newVersion}\"")
                .Replace(BasePackagePathIdentifier, $"\"{_appConfig.PackageOutputPath.FullName}\"")
                .Replace(PackagesIdentifier, packageList);

            var outDeployScript = Path.Combine(_appConfig.PackageOutputPath.FullName, DeployScriptName);
            File.WriteAllText(outDeployScript, deployScript, Encoding.UTF8);
        }
    }
}