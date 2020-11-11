using System.IO;

namespace RadeonSoftwareSlimmer.Models.PreInstall
{
    public class ScheduledTaskXmlModel
    {
        public ScheduledTaskXmlModel() { }


        public bool Enabled { get; set; }
        public string Uri { get; internal set; }
        public string Command { get; internal set; }
        public string Description { get; internal set; }
        public FileInfo File { get; internal set; }
    }
}
