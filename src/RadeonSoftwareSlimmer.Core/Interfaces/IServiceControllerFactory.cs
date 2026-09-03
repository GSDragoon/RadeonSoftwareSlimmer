namespace RadeonSoftwareSlimmer.Core.Interfaces
{
    public interface IServiceControllerFactory
    {
        IServiceController Create(string serviceName);
    }
}
