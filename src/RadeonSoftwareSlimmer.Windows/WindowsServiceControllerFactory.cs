using RadeonSoftwareSlimmer.Core.Interfaces;

namespace RadeonSoftwareSlimmer.Windows
{
    public class WindowsServiceControllerFactory : IServiceControllerFactory
    {
        public IServiceController Create(string serviceName)
        {
            return new WindowsServiceController(serviceName);
        }
    }
}
