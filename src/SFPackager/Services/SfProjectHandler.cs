using System.Collections.Generic;
using System.IO;
using System.Xml;
using SFPackager.Helpers;
using SFPackager.Models;
using SFPackager.Services.Manifest;

namespace SFPackager.Services
{
    public class SfProjectHandler
    {
        private readonly ManifestParser _appManifestHandler;
        private readonly AppConfig _baseConfig;

        public SfProjectHandler(ManifestParser appManifestHandler, AppConfig baseConfig)
        {
            _appManifestHandler = appManifestHandler;
            _baseConfig = baseConfig;
        }

        public ServiceFabricApplicationProject Parse(
            ServiceFabricApplicationProject sfProject,
            DirectoryInfo srcBasePath)
        {
            var basePath = Path.GetDirectoryName(sfProject.ProjectFileFullPath);

            using (var fileStream = new FileStream(sfProject.ProjectFileFullPath, FileMode.Open))
            using (var reader = XmlReader.Create(fileStream))
            {
                var document = new XmlDocument();
                document.Load(reader);
                var manager = new XmlNamespaceManager(document.NameTable);
                manager.AddNamespace("x", "http://schemas.microsoft.com/developer/msbuild/2003");

                sfProject.ApplicationManifestPath = ExtractApplicationManifest(basePath, document, manager);
                sfProject = _appManifestHandler.ReadXml(sfProject);

                sfProject.Services = ExtractProjectReferences(basePath, sfProject.BuildOutputPathSuffix, document, manager);

                return sfProject;
            }
        }

        internal static string ExtractApplicationManifest(
            string basePath,
            XmlNode document,
            XmlNamespaceManager namespaceManager)
        {
            var contents = document.SelectSingleNode("//*[@Include='ApplicationPackageRoot\\ApplicationManifest.xml']/@Include", namespaceManager);

            if (!(contents is XmlAttribute))
                return string.Empty;

            var attr = contents as XmlAttribute;
            var path = Path.Combine(basePath, attr.Value);

            return Path.GetDirectoryName(path);
        }

        private Dictionary<string, ServiceFabricServiceProject> ExtractProjectReferences(
            string basePath,
            string buildOutputPathSuffix,
            XmlNode document,
            XmlNamespaceManager namespaceManager)
        {
            var projectReferences = new Dictionary<string, ServiceFabricServiceProject>();

            var projects = document.SelectNodes("//x:ProjectReference/@Include", namespaceManager);

            foreach (var service in projects)
            {
                if (!(service is XmlAttribute))
                    continue;

                var attr = service as XmlAttribute;
                var projectFile = new FileInfo(Path.Combine(basePath, attr.Value));
                
                var serviceProject = new ServiceFabricServiceProject
                {
                    ProjectFolder = projectFile.Directory,
                    ProjectFile = projectFile
                };

                // Ugly ASP.Net hack for now
                var buildOutputPath = projectFile.FullName.EndsWith(".xproj")
                    ? Path.Combine(serviceProject.ProjectFolder.FullName, "bin", _baseConfig.BuildConfiguration, "net451")// $"{projectFolder}bin\\{_baseConfig.BuildConfiguration}\\net451"
                    : Path.Combine(serviceProject.ProjectFolder.FullName, buildOutputPathSuffix);// $"{projectFolder}{buildOutputPathSuffix}";

                var projectInfo = _appManifestHandler.ReadXml(serviceProject, buildOutputPath);

                projectInfo.IsAspNetCore = projectFile.FullName.EndsWith(".xproj");
                projectReferences.Add(projectInfo.ServiceName, projectInfo);
            }

            return projectReferences;
        }
    }
}