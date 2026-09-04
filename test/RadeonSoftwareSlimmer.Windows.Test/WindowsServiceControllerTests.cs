using System;
using NUnit.Framework;
using RadeonSoftwareSlimmer.Core.Enums;

namespace RadeonSoftwareSlimmer.Windows.Test
{
    public class WindowsServiceControllerTests
    {
        // Windows Event Log — always present, auto-start, expected to be Running on any healthy Windows machine.
        private const string KnownServiceName = "EventLog";
        private const string KnownServiceDisplayName = "Windows Event Log";
        private const string NonExistentServiceName = "RadeonSoftwareSlimmerDoesNotExistService";


        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void Ctor_NullOrEmptyServiceName_ThrowsArgumentException(string name)
        {
            Assert.That((System.Action)(() => new WindowsServiceController(name)), Throws.ArgumentException);
        }


        [Test]
        public void Ctor_KnownService_ExistsIsTrue()
        {
            using (WindowsServiceController controller = new WindowsServiceController(KnownServiceName))
            {
                Assert.That(controller.Exists, Is.True);
            }
        }

        [Test]
        public void Ctor_UnknownService_ExistsIsFalse()
        {
            using (WindowsServiceController controller = new WindowsServiceController(NonExistentServiceName))
            {
                Assert.That(controller.Exists, Is.False);
            }
        }

        [Test]
        public void ServiceName_ReturnsProvidedName()
        {
            using (WindowsServiceController controller = new WindowsServiceController(KnownServiceName))
            {
                Assert.That(controller.ServiceName, Is.EqualTo(KnownServiceName));
            }
        }

        [Test]
        public void DisplayName_KnownService_ReturnsExpected()
        {
            using (WindowsServiceController controller = new WindowsServiceController(KnownServiceName))
            {
                Assert.That(controller.DisplayName, Is.EqualTo(KnownServiceDisplayName));
            }
        }

        [Test]
        public void StartType_KnownService_IsAutomatic()
        {
            using (WindowsServiceController controller = new WindowsServiceController(KnownServiceName))
            {
                Assert.That(controller.StartType, Is.EqualTo(CoreServiceStartMode.Automatic));
            }
        }

        [Test]
        public void ServiceType_KnownService_HasWin32ShareProcessFlag()
        {
            using (WindowsServiceController controller = new WindowsServiceController(KnownServiceName))
            {
                Assert.That(controller.ServiceType.HasFlag(CoreServiceType.Win32ShareProcess), Is.True);
            }
        }

        [Test]
        public void Status_KnownService_IsRunning()
        {
            using (WindowsServiceController controller = new WindowsServiceController(KnownServiceName))
            {
                Assert.That(controller.Status, Is.EqualTo(CoreServiceControllerStatus.Running));
            }
        }


        [Test]
        public void Refresh_KnownService_DoesNotThrow()
        {
            using (WindowsServiceController controller = new WindowsServiceController(KnownServiceName))
            {
                Assert.That((System.Action)controller.Refresh, Throws.Nothing);
            }
        }

        [Test]
        public void WaitForStatus_KnownServiceAlreadyRunning_ReturnsImmediately()
        {
            using (WindowsServiceController controller = new WindowsServiceController(KnownServiceName))
            {
                Assert.That((System.Action)(() => controller.WaitForStatus(CoreServiceControllerStatus.Running, TimeSpan.FromSeconds(5))),
                    Throws.Nothing);
            }
        }


        [Test]
        public void Dispose_CalledMultipleTimes_DoesNotThrow()
        {
            WindowsServiceController controller = new WindowsServiceController(KnownServiceName);

            controller.Dispose();

            Assert.That((System.Action)controller.Dispose, Throws.Nothing);
        }
    }
}
