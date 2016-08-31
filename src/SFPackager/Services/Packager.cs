using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SFPackager.Interfaces;
using SFPackager.Models;

namespace SFPackager.Services
{
    public class Packager
    {
        private readonly AspNetCorePackager _aspNetCorePackager;
        private readonly IHandleFiles _fileHandler;
        private readonly PackageConfig _packageConfig;

        public Packager(AspNetCorePackager aspNetCorePackager, IHandleFiles fileHandler, PackageConfig packageConfig)
        {
            _aspNetCorePackager = aspNetCorePackager;
            _fileHandler = fileHandler;
            _packageConfig = packageConfig;
        }

        public async Task PackageApplications(
            Dictionary<string, GlobalVersion> thingsToPackage,
            Dictionary<string, ServiceFabricApplicationProject> appList)
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

                await CopyServicesToPackage(servicesToCopy, thingsToPackage, appData).ConfigureAwait(false);
            }
        }

        private async Task CopyServicesToPackage(
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
                    if (serviceData.IsAspNetCore && subPackage.Value.PackageType == PackageType.Code)
                    {
                        var package = serviceData.SubPackages
                            .First(x => x.PackageType == PackageType.Code);
                        var servicePackageFolder = $"{appData.PackagePath}\\{serviceData.ServiceName}\\{package.Name}";
                        var resultCode = _aspNetCorePackager.Package(serviceData.ProjectFolder, servicePackageFolder, "Release");
                        if (resultCode != 0)
                        {
                            throw new InvalidOperationException("Something went wrong packaging ASP.Net Core stuff");
                        }
                    }
                    else
                    {
                        await PackageFiles(appData, serviceData, subPackage).ConfigureAwait(false);
                    }
                }
            }
        }

        private static void CopyServiceManifest(ServiceFabricServiceProject service, ServiceFabricApplicationProject appData)
        {
            var servicePackageFolder = $"{appData.PackagePath}\\{service.ServiceName}";
            Directory.CreateDirectory(servicePackageFolder);
            File.Copy(service.ServiceManifestFileFullPath, $"{servicePackageFolder}\\{service.ServiceManifestFile}");
        }

        private async Task PackageFiles(
            ServiceFabricApplicationProject appData,
            ServiceFabricServiceProject serviceProject,
            KeyValuePair<string, GlobalVersion> service)
        {
            DirectoryInfo directory;
            IEnumerable<FileInfo> files;
            var package = serviceProject.SubPackages
                .First(x => x.PackageType == service.Value.PackageType);

            var extraFiles = _packageConfig
                .ExternalIncludes
                .Where(x => x
                    .ApplicationTypeName.Equals(appData.ApplicationTypeName,
                        StringComparison.CurrentCultureIgnoreCase))
                .Where(x => x
                    .ServiceManifestName.Equals(serviceProject.ServiceName,
                        StringComparison.CurrentCultureIgnoreCase))
                .Where(x => x
                    .PackageName.Equals(package.Name,
                        StringComparison.CurrentCultureIgnoreCase));

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
                directory = new DirectoryInfo($"{serviceProject.PackageRoot}{package.Name}");
                files = directory
                    .GetFiles("*", SearchOption.AllDirectories)
                    .Select(x => x.FullName)
                    .OrderBy(x => x)
                    .Select(x => new FileInfo(x));
            }

            var basePathLength = directory.FullName.Length;
            var servicePackageFolder = $"{appData.PackagePath}\\{serviceProject.ServiceName}\\{package.Name}";
            Directory.CreateDirectory(servicePackageFolder);
            foreach (var file in files)
            {
                var relPath = file.FullName.Remove(0, basePathLength + 1);
                var targetFile = new FileInfo($"{servicePackageFolder}\\{relPath}");
                if (!Directory.Exists(targetFile.DirectoryName))
                    Directory.CreateDirectory(targetFile.DirectoryName);

                File.Copy(file.FullName, targetFile.FullName);
            }

            foreach (var externalFile in extraFiles)
            {
                var file = await _fileHandler
                    .GetFileAsBytesAsync(externalFile.SourceFileName)
                    .ConfigureAwait(false);

                if(!file.IsSuccessful)
                    throw new IOException("Failed to get external file from storage");

                File.WriteAllBytes($"{servicePackageFolder}\\{externalFile.TargetFileName}", file.ResponseContent);
            }
        }
        private static void CopyApplicationManifestToPackage(ServiceFabricApplicationProject appData)
        {
            File.Copy(appData.ApplicationManifestFileFullPath, appData.AppManifestPackageTarget, true);
        }
    }
}