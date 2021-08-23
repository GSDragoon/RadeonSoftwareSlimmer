using System.IO.Abstractions;

namespace RadeonSoftwareSlimmer.Models.PreInstall
{
    public class PackageModel
    {
        private readonly IFileInfo _file;

        
        public PackageModel(IFileInfo file)
        {
            _file = file;

            Keep = true;
        }


        public bool Keep { get; set; }
        public string ProductName { get; internal set; }
        public string Url { get; internal set; }
        public string Type { get; internal set; }
        public string Description { get; internal set; }


        public IFileInfo GetFile()
        {
            return _file;
        }

        public bool Equals(PackageModel package)
        {
            if (package == null)
                return false;

            return GetComparisonHashCode().Equals(package.GetComparisonHashCode());
        }


        private int GetComparisonHashCode()
        {
            return $"{ProductName}|{Url}|{Description}|{Type}|{_file.FullName}".GetHashCode();
        }
    }
}
