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
        private readonly AppConfig _baseConfig;
        private readonly ConsoleWriter _log;
        private readonly Hack _hack;

        public Packager(AspNetCorePackager aspNetCorePackager, IHandleFiles fileHandler, PackageConfig packageConfig,
            AppConfig baseConfig, ConsoleWriter log, Hack hack)
        {
            _aspNetCorePackager = aspNetCorePackager;
            _fileHandler = fileHandler;
            _packageConfig = packageConfig;
            _baseConfig = baseConfig;
            _log = log;
            _hack = hack;
        }

        public async Task PackageApplications(
            VersionMap thingsToPackage,
            Dictionary<string, ServiceFabricApplicationProject> appList,
            Dictionary<string, byte[]> hackedFiles)
        {
            if (_baseConfig.PackageOutputPath.Exists && _baseConfig.CleanOutputFolder)
            {
                try
                {
                    _baseConfig.PackageOutputPath.Delete(true);
                }
                catch (Exception ex)
                {
                    _log.WriteLine($"Problems removing packaging folder. Is something holding file lock?", LogLevel.Error);
                    throw;
                }
            }

            if(!_baseConfig.PackageOutputPath.Exists)
                _baseConfig.PackageOutputPath.Create();

            var applications = thingsToPackage
                .PackageVersions
                .Where(x => x.Value.VersionType == VersionType.Application)
                .Where(x => x.Value.IncludeInPackage);

            foreach (var source in applications)
            {
                var appData = appList[source.Key];

                var applicationPackagePath = appData.GetPackagePath(_baseConfig.PackageOutputPath);
                applicationPackagePath.Create();
                CopyApplicationManifestToPackage(appData, applicationPackagePath);

                var servicesToCopy = thingsToPackage
                    .PackageVersions
                    .Where(x => x.Value.VersionType == VersionType.Service)
                    .Where(x => x.Value.IncludeInPackage)
                    .Where(x => x.Value.ParentRef.Equals(source.Key))
                    .ToList();

                await CopyServicesToPackage(servicesToCopy, thingsToPackage.PackageVersions, appData, applicationPackagePath, hackedFiles).ConfigureAwait(false);
            }
        }

        private async Task CopyServicesToPackage(
            IEnumerable<KeyValuePair<string, GlobalVersion>> services,
            Dictionary<string, GlobalVersion> thingsToPackage,
            ServiceFabricApplicationProject appData,
            DirectoryInfo basePackagePath,
            Dictionary<string, byte[]> hackedFiles)
        {
            foreach (var service in services)
            {
                var serviceKey = service.Key.Split('-').Last();
                var serviceData = appData.Services[serviceKey];

                CopyServiceManifest(serviceData, basePackagePath);

                var subPackages = thingsToPackage
                    .Where(x => x.Value.VersionType == VersionType.ServicePackage)
                    .Where(x => x.Value.IncludeInPackage)
                    .Where(x => x.Value.ParentRef.Equals(service.Key));

                foreach (var subPackage in subPackages)
                {
                    if (serviceData.IsAspNetCore && subPackage.Value.PackageType == PackageType.Code)
                    {
                        _hack.RecreateHackFiles(hackedFiles);

                        var package = serviceData.SubPackages
                            .First(x => x.PackageType == PackageType.Code);
                        var servicePackageFolder = Path.Combine(basePackagePath.FullName, serviceData.ServiceName, package.Name);
                        var resultCode = _aspNetCorePackager.Package(serviceData.ProjectFolder.FullName, servicePackageFolder, _baseConfig.BuildConfiguration, _baseConfig.DotNetPublishExtraArgs);
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

        private static void CopyServiceManifest(ServiceFabricServiceProject service, DirectoryInfo basePackagePath)
        {
            var servicePackageFolder = service.GetServicePackageFolder(basePackagePath);
            if(!servicePackageFolder.Exists)
                servicePackageFolder.Create();

            File.Copy(service.SourceServiceManifestPath, service.GetServiceManifestTargetFile(servicePackageFolder).FullName);
        }

        private async Task PackageFiles(
            ServiceFabricApplicationProject appData,
            ServiceFabricServiceProject serviceProject,
            KeyValuePair<string, GlobalVersion> service)
        {
            var appPackagePath = appData.GetPackagePath(_baseConfig.PackageOutputPath);
            var servicePackagePath = serviceProject.GetServicePackageFolder(appPackagePath);
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
                if (!serviceProject.IsGuestExecutable)
                {
                    directory = new DirectoryInfo(Path.Combine(serviceProject.ProjectFolder.FullName,
                        appData.BuildOutputPathSuffix));

                    files = directory
                        .GetFiles("*", SearchOption.AllDirectories)
                        .Where(x => _packageConfig.HashIncludeExtensions.Any(include =>
                            x.FullName.ToLowerInvariant().EndsWith(include.ToLowerInvariant())))
                        .Select(x => x.FullName)
                        .OrderBy(x => x)
                        .Select(x => new FileInfo(x));
                }
                else
                {
                    directory = new DirectoryInfo(Path.Combine(serviceProject.ProjectFolder.FullName, "Code"));

                    files = directory
                        .GetFiles("*", SearchOption.AllDirectories)
                        .Select(x => x.FullName)
                        .OrderBy(x => x)
                        .Select(x => new FileInfo(x));
                }
            }
            else
            {
                directory = new DirectoryInfo(Path.Combine(serviceProject.PackageRoot.FullName, package.Name));
                files = directory
                    .GetFiles("*", SearchOption.AllDirectories)
                    .Select(x => x.FullName)
                    .OrderBy(x => x)
                    .Select(x => new FileInfo(x));
            }

            var basePathLength = directory.FullName.Length;
            var subPackageFolder = package.GetSubPackageTargetPath(servicePackagePath);
            if(!subPackageFolder.Exists)
                subPackageFolder.Create();

            foreach (var file in files)
            {
                var relPath = file.FullName.Remove(0, basePathLength + 1);
                var targetFile = new FileInfo(Path.Combine(subPackageFolder.FullName, relPath));
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

                File.WriteAllBytes(Path.Combine(subPackageFolder.FullName, externalFile.TargetFileName), file.ResponseContent);
            }
        }

        private static void CopyApplicationManifestToPackage(ServiceFabricApplicationProject appData, DirectoryInfo applicationPackagePath)
        {
            File.Copy(appData.ApplicationManifestFileFullPath, appData.GetAppManifestTargetFile(applicationPackagePath).FullName, true);
        }
    }
}