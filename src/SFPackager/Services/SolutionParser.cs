using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SFPackager.Services
{
    public class SolutionParser
    {
        private const string ProjectPattern =
            "Project\\(\"{A07B5EB6-E848-4116-A8D0-A826331D98C6}\"\\)\\ =\\ \".*\", \"(.*)\", \"{.*}\"";

        public async Task<List<FileInfo>> ExtractSfProjects(FileInfo solutionFile)
        {
            if (!solutionFile.Exists)
                throw new ArgumentException("Solution file not found", nameof(solutionFile));

            var result = new List<FileInfo>();

            using (var fs = new FileStream(solutionFile.FullName, FileMode.Open))
            using (var reader = new StreamReader(fs))
            {
                string line;
                while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                {
                    var matchResult = Regex.Match(line, ProjectPattern);
                    if (!matchResult.Success) 
                        continue;
                    
                    var match = matchResult.Groups[1];
                    var path = Path.Combine(solutionFile.DirectoryName, match.Value);
                    var file = new FileInfo(path);
                    result.Add(file);
                }
            }

            return result;
        }
    }
}