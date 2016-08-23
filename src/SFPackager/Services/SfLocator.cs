using System.Collections.Generic;
using System.IO;
using System.Linq;
using SFPackager.Models;

namespace SFPackager.Services
{
    public class SfLocator
    {
        public List<ServiceFabricApplicationProject> LocateSfApplications(string basePath, string buildConfiguration)
        {
            var directory = new DirectoryInfo(basePath);
            var fileList = directory.GetFiles("*.sfproj", SearchOption.AllDirectories);

            var applications = fileList
                .Where(x => !string.Equals(x.Name, "SignupWeb.sfproj")) 
                .Select(fileInfo => new ServiceFabricApplicationProject
                {
                    ProjectFile = fileInfo.Name,
                    ProjectFolder = fileInfo.Directory.FullName,
                    BasePath = basePath,
                    BuildConfiguration = buildConfiguration
                })
                .ToList();

            return applications;
        }
    }
}