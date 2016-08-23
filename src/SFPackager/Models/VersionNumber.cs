using Newtonsoft.Json;

namespace SFPackager.Models
{
    public class VersionNumber
    {
        private VersionNumber()
        {
        }

        public string CommitHash { get; set; }

        public int RollingNumber { get; set; }

        [JsonIgnore]
        public string Version => $"{RollingNumber}-{CommitHash}";

        [JsonIgnore]
        public string FileName => $"{Version}.json";

        public static VersionNumber Default()
        {
            return new VersionNumber
            {
                RollingNumber = 0,
                CommitHash = string.Empty,
            };
        }

        public static VersionNumber Parse(string version)
        {
            if (!version.Contains("-"))
                return Default();

            var split = version.Split('-');
            if (string.IsNullOrWhiteSpace(split[0]) || string.IsNullOrWhiteSpace(split[1]))
                return Default();

            return new VersionNumber
            {
                RollingNumber = int.Parse(split[0]),
                CommitHash = split[1]
            };
        }

        public static VersionNumber Create(int version, string commitHash)
        {
            return new VersionNumber
            {
                RollingNumber = version,
                CommitHash = commitHash
            };
        }
    }

    public static class VersioNumberExtensions
    {
        public static VersionNumber Increment(this VersionNumber current, string commitHash)
        {
            return VersionNumber.Create(current.RollingNumber + 1, commitHash);
        }
    }
}