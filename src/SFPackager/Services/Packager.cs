using System.Collections.Generic;
using System.IO;
using System.Linq;
using SFPackager.Models;

namespace SFPackager.Services
{
    public class Packager
    {
        public void PackageApplications(
            Dictionary<string, GlobalVersion> thingsToPackage,
            Dictionary<string, ServiceFabricApplicationProject> appList,
            PackageConfig packageConfig)
        {
            var applications = thingsToPackage
                .Where(x => x.Value.VersionType == VersionType.Application)
                .Where(x => x.Value.IncludeInPackage);

            foreach (var source in applications)
            {
                var appData = appList[source.Key];

                Directory.CreateDirectory(appData.PackagePath);
                CopyApplicationManifestToPackage(appData);

                var servicesToCopy = thingsToPackage
                    .Where(x => x.Value.VersionType == VersionType.Service)
                    .Where(x => x.Value.IncludeInPackage)
                    .Where(x => x.Value.ParentRef.Equals(source.Key))
                    .ToList();

                CopyServicesToPackage(servicesToCopy, thingsToPackage, appData);
            }
        }

        private static void CopyServicesToPackage(
            IEnumerable<KeyValuePair<string, GlobalVersion>> services,
            Dictionary<string, GlobalVersion> thingsToPackage,
            ServiceFabricApplicationProject appData)
        {
            foreach (var service in services)
            {
                var serviceData = appData.Services[service.Key];

                CopyServiceManifest(serviceData, appData);

                var subPackages = thingsToPackage
                    .Where(x => x.Value.VersionType == VersionType.ServicePackage)
                    .Where(x => x.Value.IncludeInPackage)
                    .Where(x => x.Value.ParentRef.Equals(service.Key));

                foreach (var subPackage in subPackages)
                {
                    PackageFiles(appData, serviceData, subPackage);
                }
            }
        }

        private static void CopyServiceManifest(ServiceFabricServiceProject service, ServiceFabricApplicationProject appData)
        {
            var servicePackageFolder = $"{appData.PackagePath}\\{service.ServiceName}";
            Directory.CreateDirectory(servicePackageFolder);
            File.Copy(service.ServiceManifestFileFullPath, $"{servicePackageFolder}\\{service.ServiceManifestFile}");
        }

        private static void PackageFiles(
            ServiceFabricApplicationProject appData,
            ServiceFabricServiceProject serviceProject,
            KeyValuePair<string, GlobalVersion> service)
        {
            DirectoryInfo directory;
            IEnumerable<FileInfo> files;
            var temp = serviceProject.SubPackages
                .First(x => x.PackageType == service.Value.PackageType);

            if (service.Value.PackageType == PackageType.Code)
            {
                directory = new DirectoryInfo($"{serviceProject.ProjectFolder}{appData.BuildOutputPathSuffix}");
                files = directory
                    .GetFiles("*", SearchOption.AllDirectories)
                    .Where(Constants.IncludeFileFilter)
                    .Select(x => x.FullName)
                    .OrderBy(x => x)
                    .Select(x => new FileInfo(x));
            }
            else
            {
                directory = new DirectoryInfo($"{serviceProject.PackageRoot}{temp.Name}");
                files = directory
                    .GetFiles("*", SearchOption.AllDirectories)
                    .Select(x => x.FullName)
                    .OrderBy(x => x)
                    .Select(x => new FileInfo(x));
            }

            var basePathLength = directory.FullName.Length;
            var servicePackageFolder = $"{appData.PackagePath}\\{serviceProject.ServiceName}\\{temp.Name}";
            Directory.CreateDirectory(servicePackageFolder);
            foreach (var file in files)
            {
                var relPath = file.FullName.Remove(0, basePathLength + 1);
                var targetFile = new FileInfo($"{servicePackageFolder}\\{relPath}");
                if (!Directory.Exists(targetFile.DirectoryName))
                    Directory.CreateDirectory(targetFile.DirectoryName);

                File.Copy(file.FullName, targetFile.FullName);
            }
        }
        private static void CopyApplicationManifestToPackage(ServiceFabricApplicationProject appData)
        {
            File.Copy(appData.ApplicationManifestFileFullPath, appData.AppManifestPackageTarget, true);
        }
    }
}