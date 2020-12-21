namespace RadeonSoftwareSlimmer.Models.PostInstall
{
    public class RunningHostServiceModel : RunningProcessModel
    {
        public RunningHostServiceModel(string fileName, string description) : base(fileName, description)
        {
            ProcessType = "Host Service";
        }
    }
}
