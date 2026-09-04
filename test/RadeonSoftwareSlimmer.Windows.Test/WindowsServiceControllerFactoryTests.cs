using NUnit.Framework;
using RadeonSoftwareSlimmer.Core.Interfaces;

namespace RadeonSoftwareSlimmer.Windows.Test
{
    public class WindowsServiceControllerFactoryTests
    {
        // "EventLog" (Windows Event Log) is guaranteed to be present on every supported Windows install.
        private const string KnownServiceName = "EventLog";
        private const string NonExistentServiceName = "RadeonSoftwareSlimmerDoesNotExistService";


        [Test]
        public void Create_KnownService_ReturnsControllerThatExists()
        {
            WindowsServiceControllerFactory factory = new WindowsServiceControllerFactory();

            using (IServiceController controller = factory.Create(KnownServiceName))
            {
                Assert.Multiple((System.Action)(() =>
                {
                    Assert.That(controller, Is.Not.Null);
                    Assert.That(controller.Exists, Is.True);
                    Assert.That(controller.ServiceName, Is.EqualTo(KnownServiceName));
                }));
            }
        }

        [Test]
        public void Create_UnknownService_ReturnsControllerThatDoesNotExist()
        {
            WindowsServiceControllerFactory factory = new WindowsServiceControllerFactory();

            using (IServiceController controller = factory.Create(NonExistentServiceName))
            {
                Assert.Multiple((System.Action)(() =>
                {
                    Assert.That(controller, Is.Not.Null);
                    Assert.That(controller.Exists, Is.False);
                    Assert.That(controller.ServiceName, Is.EqualTo(NonExistentServiceName));
                }));
            }
        }
    }
}
