using System.Collections.Generic;
using System.IO;
using System.Xml;
using SFPackager.Helpers;
using SFPackager.Models;

namespace SFPackager.Services
{
    public class SfProjectHandler
    {
        private readonly ServiceFabricApplicationManifestHandler _appManifestHandler;
        private readonly ServiceFabricServiceManifestHandler _serviceManifestHandler;

        public SfProjectHandler(ServiceFabricApplicationManifestHandler appManifestHandler, ServiceFabricServiceManifestHandler serviceManifestHandler)
        {
            _appManifestHandler = appManifestHandler;
            _serviceManifestHandler = serviceManifestHandler;
        }

        public ServiceFabricApplicationProject Parse(ServiceFabricApplicationProject sfProject, string srcBasePath)
        {
            var basePath = FileHelper.RemoveFileFromPath(sfProject.ProjectFileFullPath);

            using (var fileStream = new FileStream(sfProject.ProjectFileFullPath, FileMode.Open))
            using (var reader = XmlReader.Create(fileStream))
            {
                var document = new XmlDocument();
                document.Load(reader);
                var manager = new XmlNamespaceManager(document.NameTable);
                manager.AddNamespace("x", "http://schemas.microsoft.com/developer/msbuild/2003");
                
                sfProject.ApplicationManifestPath = ExtractApplicationManifest(basePath, document, manager);
                sfProject.BasePath = srcBasePath;

                sfProject = _appManifestHandler.ReadXml(sfProject);

                sfProject.Services = ExtractProjectReferences(basePath, sfProject.BuildOutputPathSuffix ,document, manager);

                return sfProject;
            }
        }

        private static string ExtractApplicationManifest(string basePath, XmlNode document,
            XmlNamespaceManager namespaceManager)
        {
            var contents = document.SelectNodes("//x:Content/@Include", namespaceManager);

            foreach (var content in contents)
            {
                if (!(content is XmlAttribute))
                    continue;

                var attr = content as XmlAttribute;
                if (!attr.Value.ToLowerInvariant().Contains("applicationmanifest"))
                    continue;

                var path = FileHelper.RemoveFileFromPath(attr.Value);
                return Path.GetFullPath($"{basePath}{path}");
            }

            return string.Empty;
        }

        private List<ServiceFabricServiceProject> ExtractProjectReferences(string basePath, string buildOutputPathSuffix, XmlNode document,
            XmlNamespaceManager namespaceManager)
        {
            var projectReferences = new List<ServiceFabricServiceProject>();

            var projects = document.SelectNodes("//x:ProjectReference/@Include", namespaceManager);

            foreach (var service in projects)
            {
                if (!(service is XmlAttribute))
                    continue;

                var attr = service as XmlAttribute;
                var path = FileHelper.RemoveFileFromPath(attr.Value);
                var absolutePath = Path.GetFullPath($"{basePath}{path}");
                var projectFolder = FileHelper.RemoveFileFromPath(absolutePath);

                var serviceProject = new ServiceFabricServiceProject
                {
                    PackageRoot = $"{projectFolder}PackageRoot\\",
                    ProjectFolder = projectFolder,
                    ProjectFile = FileHelper.RemovePathFromPath(attr.Value)
                };

                var buildOutputPath = $"{projectFolder}{buildOutputPathSuffix}";
                var stuff = _serviceManifestHandler.ReadXml(serviceProject, buildOutputPath);

                projectReferences.Add(stuff);
            }

            return projectReferences;
        }
    }
}