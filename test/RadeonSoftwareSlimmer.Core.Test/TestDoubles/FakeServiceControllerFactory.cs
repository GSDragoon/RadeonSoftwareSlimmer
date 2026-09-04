using System.Collections.Generic;
using RadeonSoftwareSlimmer.Core.Interfaces;

namespace RadeonSoftwareSlimmer.Core.Test.TestDoubles
{
    public class FakeServiceControllerFactory : IServiceControllerFactory
    {
        public IDictionary<string, FakeServiceController> Controllers { get; } = new Dictionary<string, FakeServiceController>();


        public FakeServiceController AddTestService(string serviceName, FakeServiceController controller)
        {
            controller.ServiceName = serviceName;
            Controllers.Add(serviceName, controller);
            return controller;
        }


        public IServiceController Create(string serviceName)
        {
            return Controllers.TryGetValue(serviceName, out FakeServiceController controller)
                ? controller
                : new FakeServiceController { ServiceName = serviceName, Exists = false };
        }
    }
}
