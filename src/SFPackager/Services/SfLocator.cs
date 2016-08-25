using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using SFPackager.Models;

namespace SFPackager.Services
{
    public class SfLocator
    {
        public List<ServiceFabricApplicationProject> LocateSfApplications(string basePath, string buildConfiguration)
        {
            Console.WriteLine("Locating ServiceFabric Applications.");
            var directory = new DirectoryInfo(basePath);
            var fileList = directory.GetFiles("*.sfproj", SearchOption.AllDirectories);

            var applications = fileList
                //.Where(x => !string.Equals(x.Name, "SignupWeb.sfproj")) 
                .Select(fileInfo =>
                {
                    Console.WriteLine($"\tFound {fileInfo.Directory.FullName}");
                    return new ServiceFabricApplicationProject
                    {
                        ProjectFile = fileInfo.Name,
                        ProjectFolder = fileInfo.Directory.FullName,
                        BasePath = basePath,
                        BuildConfiguration = buildConfiguration
                    };
                })
                .ToList();

            Console.WriteLine();
            return applications;
        }
    }
}