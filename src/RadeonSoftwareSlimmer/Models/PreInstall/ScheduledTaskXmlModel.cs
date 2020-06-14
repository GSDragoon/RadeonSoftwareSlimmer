using System.IO;

namespace RadeonSoftwareSlimmer.Models.PreInstall
{
    public class ScheduledTaskXmlModel
    {
        public ScheduledTaskXmlModel() { }


        public bool Enabled { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1056:Uri properties should not be strings", Justification = "This is what it's called in the XML file by Microsoft.")]
        public string Uri { get; internal set; }
        public string Command { get; internal set; }
        public string Description { get; internal set; }
        public FileInfo File { get; internal set; }
    }
}
