using NUnit.Framework;
using RadeonSoftwareSlimmer.Core.Enums;
using RadeonSoftwareSlimmer.Core.PostInstall;
using RadeonSoftwareSlimmer.Core.Test.TestDoubles;

namespace RadeonSoftwareSlimmer.Core.Test.Models.PostInstall
{
    public class ServiceModelTests
    {
        private const string ServiceName = "TestService";
        private const string ServiceKeyPath = @"SYSTEM\CurrentControlSet\Services";
        private const string StartValueName = "Start";
        private const string OriginalStartValueName = "RadeonSoftwareSlimmerOriginalStart";

        private FakeRegistry _registry;
        private FakeAppLogger _logger;
        private FakeProcessRunner _processRunner;
        private FakeServiceControllerFactory _factory;

        [SetUp]
        public void Setup()
        {
            _registry = new FakeRegistry();
            _logger = new FakeAppLogger();
            _processRunner = new FakeProcessRunner();
            _factory = new FakeServiceControllerFactory();
        }


        // Registers a service with the factory. Defaults match a healthy Win32 service so tests only override what they care about.
        private FakeServiceController AddService(
            string displayName = "Test Display Name",
            CoreServiceStartMode startType = CoreServiceStartMode.Automatic,
            CoreServiceType serviceType = CoreServiceType.Win32OwnProcess,
            CoreServiceControllerStatus status = CoreServiceControllerStatus.Running)
        {
            return _factory.AddTestService(ServiceName, new FakeServiceController
            {
                DisplayName = displayName,
                StartType = startType,
                ServiceType = serviceType,
                Status = status,
            });
        }

        private FakeRegistryKey AddServiceRegistryKey(int startValue = (int)CoreServiceStartMode.Automatic, int? savedOriginalValue = null)
        {
            FakeRegistryKey serviceKey = _registry.MockLocalMachine
                .AddTestSubKey(ServiceKeyPath)
                .AddTestSubKey(ServiceName)
                .AddTestValue(StartValueName, startValue, CoreRegistryValueKind.DWord);

            if (savedOriginalValue.HasValue)
                serviceKey.AddTestValue(OriginalStartValueName, savedOriginalValue.Value, CoreRegistryValueKind.DWord);

            return serviceKey;
        }

        private ServiceModel BuildModel()
        {
            return new ServiceModel(ServiceName, _registry, _logger, _processRunner, _factory);
        }


        [Test]
        public void Ctor_NonExistentService_ExistsReturnsFalse()
        {
            ServiceModel model = new ServiceModel("nonexistent", _registry, _logger, _processRunner, _factory);

            Assert.That(model.Exists(), Is.False);
        }

        [Test]
        public void Ctor_ExistingService_LoadsBasicProperties()
        {
            AddService(displayName: "Test Display Name", startType: CoreServiceStartMode.Automatic,
                       serviceType: CoreServiceType.Win32OwnProcess, status: CoreServiceControllerStatus.Running);
            AddServiceRegistryKey();

            ServiceModel model = BuildModel();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(model.Exists(), Is.True);
                Assert.That(model.Name, Is.EqualTo(ServiceName));
                Assert.That(model.DisplayName, Is.EqualTo("Test Display Name"));
                Assert.That(model.StartMode, Is.EqualTo(CoreServiceStartMode.Automatic));
                Assert.That(model.Enabled, Is.True);
                Assert.That(model.Status, Is.EqualTo(CoreServiceControllerStatus.Running));
                Assert.That(model.Type, Is.EqualTo(CoreServiceType.Win32OwnProcess.ToString()));
            }
        }

        [Test]
        public void Ctor_DisabledStartMode_EnabledIsFalse()
        {
            AddService(startType: CoreServiceStartMode.Disabled, status: CoreServiceControllerStatus.Stopped);
            AddServiceRegistryKey(startValue: (int)CoreServiceStartMode.Disabled);

            ServiceModel model = BuildModel();

            Assert.That(model.Enabled, Is.False);
        }

        [Test]
        public void Ctor_OriginalStartModePresent_LoadsFromRegistry()
        {
            AddService(startType: CoreServiceStartMode.Disabled, status: CoreServiceControllerStatus.Stopped);
            AddServiceRegistryKey(startValue: (int)CoreServiceStartMode.Disabled,
                                  savedOriginalValue: (int)CoreServiceStartMode.Manual);

            ServiceModel model = BuildModel();

            Assert.That(model.OriginalStartMode, Is.EqualTo(CoreServiceStartMode.Manual));
        }

        [Test]
        public void Ctor_OriginalStartModeAbsent_BackupsCurrentStartValue()
        {
            AddService();
            FakeRegistryKey serviceKey = AddServiceRegistryKey(startValue: (int)CoreServiceStartMode.Automatic);

            ServiceModel model = BuildModel();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(model.OriginalStartMode, Is.EqualTo(CoreServiceStartMode.Automatic));
                Assert.That(serviceKey.Values.ContainsKey(OriginalStartValueName), Is.True);
                Assert.That(serviceKey.Values[OriginalStartValueName].Value, Is.EqualTo((int)CoreServiceStartMode.Automatic));
                Assert.That(serviceKey.Values[OriginalStartValueName].Kind, Is.EqualTo(CoreRegistryValueKind.DWord));
            }
        }


        [Test]
        public void TryStart_ServiceIsDisabled_DoesNotStart()
        {
            FakeServiceController controller = AddService(startType: CoreServiceStartMode.Disabled, status: CoreServiceControllerStatus.Stopped);
            AddServiceRegistryKey(startValue: (int)CoreServiceStartMode.Disabled);
            ServiceModel model = BuildModel();

            model.TryStart();

            Assert.That(controller.StartCalls, Is.Zero);
        }

        [Test]
        public void TryStart_KernelDriver_DoesNotStart()
        {
            FakeServiceController controller = AddService(serviceType: CoreServiceType.KernelDriver, status: CoreServiceControllerStatus.Stopped);
            AddServiceRegistryKey();
            ServiceModel model = BuildModel();

            model.TryStart();

            Assert.That(controller.StartCalls, Is.Zero);
        }

        [Test]
        public void TryStart_Win32OwnProcess_StopsThenStarts()
        {
            FakeServiceController controller = AddService(status: CoreServiceControllerStatus.Running);
            AddServiceRegistryKey();
            ServiceModel model = BuildModel();

            model.TryStart();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(controller.StopCalls, Is.EqualTo(1));
                Assert.That(controller.StartCalls, Is.EqualTo(1));
            }
        }


        [Test]
        public void TryStop_KernelDriver_DoesNotStop()
        {
            FakeServiceController controller = AddService(serviceType: CoreServiceType.KernelDriver);
            AddServiceRegistryKey();
            ServiceModel model = BuildModel();

            model.TryStop();

            Assert.That(controller.StopCalls, Is.Zero);
        }

        [Test]
        public void TryStop_RunningWin32Process_Stops()
        {
            FakeServiceController controller = AddService(status: CoreServiceControllerStatus.Running);
            AddServiceRegistryKey();
            ServiceModel model = BuildModel();

            model.TryStop();

            Assert.That(controller.StopCalls, Is.EqualTo(1));
        }

        [Test]
        public void TryStop_AlreadyStopped_DoesNotCallStop()
        {
            FakeServiceController controller = AddService(status: CoreServiceControllerStatus.Stopped);
            AddServiceRegistryKey();
            ServiceModel model = BuildModel();

            model.TryStop();

            Assert.That(controller.StopCalls, Is.Zero);
        }


        [Test]
        public void Delete_CallsScExeDeleteWithServiceName()
        {
            AddService(status: CoreServiceControllerStatus.Stopped);
            AddServiceRegistryKey();
            ServiceModel model = BuildModel();

            model.Delete();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_processRunner.LastFileName, Does.EndWith("sc.exe"));
                Assert.That(_processRunner.LastArguments, Is.EqualTo($"delete \"{ServiceName}\""));
            }
        }


        [Test]
        public void Enable_CurrentlyDisabledWithOriginalNotDisabled_RunsScExeWithOriginalMode()
        {
            AddService(startType: CoreServiceStartMode.Disabled, status: CoreServiceControllerStatus.Stopped);
            AddServiceRegistryKey(startValue: (int)CoreServiceStartMode.Disabled,
                                  savedOriginalValue: (int)CoreServiceStartMode.Automatic);
            ServiceModel model = BuildModel();

            model.Enable();

            Assert.That(_processRunner.LastArguments, Does.Contain("start= auto"));
        }

        [Test]
        public void Enable_CurrentlyNotDisabled_DoesNothing()
        {
            AddService(startType: CoreServiceStartMode.Automatic);
            AddServiceRegistryKey();
            ServiceModel model = BuildModel();

            model.Enable();

            Assert.That(_processRunner.LastFileName, Is.Null);
        }

        [Test]
        public void Enable_OriginalStartModeWasDisabled_DoesNothing()
        {
            AddService(startType: CoreServiceStartMode.Disabled, status: CoreServiceControllerStatus.Stopped);
            AddServiceRegistryKey(startValue: (int)CoreServiceStartMode.Disabled,
                                  savedOriginalValue: (int)CoreServiceStartMode.Disabled);
            ServiceModel model = BuildModel();

            model.Enable();

            Assert.That(_processRunner.LastFileName, Is.Null);
        }


        [Test]
        public void Disable_CurrentlyEnabled_RunsScExeWithDisabled()
        {
            AddService(startType: CoreServiceStartMode.Automatic, status: CoreServiceControllerStatus.Running);
            AddServiceRegistryKey();
            ServiceModel model = BuildModel();

            model.Disable();

            Assert.That(_processRunner.LastArguments, Does.Contain("start= disabled"));
        }

        [Test]
        public void Disable_Win32OwnProcess_StopsBeforeDisabling()
        {
            FakeServiceController controller = AddService(startType: CoreServiceStartMode.Automatic, status: CoreServiceControllerStatus.Running);
            AddServiceRegistryKey();
            ServiceModel model = BuildModel();

            model.Disable();

            Assert.That(controller.StopCalls, Is.EqualTo(1));
        }

        [Test]
        public void Disable_CurrentlyDisabled_DoesNothing()
        {
            AddService(startType: CoreServiceStartMode.Disabled, status: CoreServiceControllerStatus.Stopped);
            AddServiceRegistryKey(startValue: (int)CoreServiceStartMode.Disabled);
            ServiceModel model = BuildModel();

            model.Disable();

            Assert.That(_processRunner.LastFileName, Is.Null);
        }


        [TestCase("Boot", "boot")]
        [TestCase("System", "system")]
        [TestCase("Automatic", "auto")]
        [TestCase("Manual", "demand")]
        [TestCase("Disabled", "disabled")]
        public void SetStartMode_ValidMode_CallsScExeWithMatchingArg(string mode, string expectedScArg)
        {
            AddService();
            AddServiceRegistryKey();
            ServiceModel model = BuildModel();

            model.SetStartMode(mode);

            Assert.That(_processRunner.LastArguments, Does.Contain($"start= {expectedScArg}"));
        }
    }
}
