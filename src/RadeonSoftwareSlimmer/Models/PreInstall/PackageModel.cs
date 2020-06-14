using System.IO;

namespace RadeonSoftwareSlimmer.Models.PreInstall
{
    public class PackageModel
    {
        public PackageModel()
        {
            Keep = true;
        }


        public bool Keep { get; set; }
        public string ProductName { get; internal set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1056:Uri properties should not be strings", Justification = "This is what it's called in the XML file by AMD.")]
        public string Url { get; internal set; }
        public string Type { get; internal set; }
        public string Description { get; internal set; }
        public FileInfo File { get; internal set; }


        public bool Equals(PackageModel package)
        {
            if (package == null)
                return false;

            return GetComparisonHashCode().Equals(package.GetComparisonHashCode());
        }
        private int GetComparisonHashCode()
        {
            return $"{ProductName}|{Url}|{Description}|{Type}|{File.FullName}".GetHashCode();
        }
    }
}
