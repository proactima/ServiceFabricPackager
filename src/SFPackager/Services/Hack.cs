using SFPackager.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SFPackager.Services
{
    public class Hack
    {
        private readonly AppConfig _appConfig;

        public Hack(AppConfig appConfig)
        {
            _appConfig = appConfig;
        }

        public Dictionary<string, byte[]> FindHackableThings(ServiceFabricApplicationProject randomProject)
        {
            var projectFolderToFind = "Common";
            var filePattern = "Common.resources.dll";

            var root = Directory.GetParent(randomProject.ProjectFolder);

            var projectRoot = Path.Combine(root.FullName, projectFolderToFind);
            var searchSubPath = $"obj\\{_appConfig.BuildConfiguration}";

            var searchPath = Path.Combine(projectRoot, searchSubPath);

            var allFiles = Directory.EnumerateFiles(searchPath, filePattern, SearchOption.AllDirectories);

            var results = new Dictionary<string, byte[]>();

            foreach (var foundFile in allFiles)
            {
                var data = File.ReadAllBytes(foundFile);
                results.Add(foundFile, data);
            }

            return results;
        }

        public void RecreateHackFiles(Dictionary<string, byte[]> originalFiles)
        {
            foreach(var file in originalFiles)
            {
                var path = Path.GetDirectoryName(file.Key);
                Directory.CreateDirectory(path);

                File.WriteAllBytes(file.Key, file.Value);
            }
        }
    }
}
