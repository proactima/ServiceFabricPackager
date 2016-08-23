using System.IO;

namespace SFPackager.Helpers
{
    public static class FileHelper
    {
        public static string RemoveFileFromPath(string csProjFile)
        {
            var lastSeparator = csProjFile.LastIndexOf('\\');
            var t = csProjFile.Substring(0, lastSeparator + 1);
            return t;
        }

        public static string RemovePathFromPath(string path)
        {
            var fileInfoo = new FileInfo(path);
            return fileInfoo.Name;
        }

        public static byte[] ReadFile(string file)
        {
            return File.ReadAllBytes(file);
        }
    }
}