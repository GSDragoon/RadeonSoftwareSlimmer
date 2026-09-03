using System.Linq;
using NUnit.Framework;
using RadeonSoftwareSlimmer.Core.Enums;
using RadeonSoftwareSlimmer.Core.PostInstall;
using RadeonSoftwareSlimmer.Core.Test.TestDoubles;

namespace RadeonSoftwareSlimmer.Core.Test.Models.PostInstall
{
    public class ServiceListModelTests
    {
        // Names that the model actually enumerates. Picked one from each family.
        private const string KnownKernelService = "amdfendr";
        private const string KnownAudioService = "AMDSAFD";
        private const string KnownNtService = "AMD External Events Utility";
        private const string UnknownService = "SomeCompletelyOtherService";

        private const string ServiceKeyPath = @"SYSTEM\CurrentControlSet\Services";
        private const string StartValueName = "Start";

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


        private void RegisterService(
            string name,
            CoreServiceStartMode startType = CoreServiceStartMode.Automatic,
            CoreServiceType serviceType = CoreServiceType.Win32OwnProcess,
            CoreServiceControllerStatus status = CoreServiceControllerStatus.Running)
        {
            _factory.AddTestService(name, new FakeServiceController
            {
                DisplayName = $"Display for {name}",
                StartType = startType,
                ServiceType = serviceType,
                Status = status,
            });

            _registry.MockLocalMachine
                .AddTestSubKey(ServiceKeyPath)
                .AddTestSubKey(name)
                .AddTestValue(StartValueName, (int)startType, CoreRegistryValueKind.DWord);
        }


        [Test]
        public void LoadOrRefresh_NoServicesRegistered_EmptyList()
        {
            ServiceListModel model = new ServiceListModel(_registry, _logger, _processRunner, _factory);

            model.LoadOrRefresh();

            Assert.That(model.Services, Is.Empty);
        }

        [Test]
        public void LoadOrRefresh_KnownServicesRegistered_ReturnsOnlyThose()
        {
            RegisterService(KnownKernelService, serviceType: CoreServiceType.KernelDriver);
            RegisterService(KnownAudioService);
            RegisterService(KnownNtService);
            ServiceListModel model = new ServiceListModel(_registry, _logger, _processRunner, _factory);

            model.LoadOrRefresh();

            Assert.That(model.Services.Select(s => s.Name), Is.EquivalentTo(new[] { KnownKernelService, KnownAudioService, KnownNtService }));
        }

        [Test]
        public void LoadOrRefresh_ServiceOutsideKnownList_IsExcluded()
        {
            RegisterService(UnknownService);
            ServiceListModel model = new ServiceListModel(_registry, _logger, _processRunner, _factory);

            model.LoadOrRefresh();

            Assert.That(model.Services, Is.Empty);
        }


        [Test]
        public void ApplyChanges_ModelEnabled_CallsEnableOnEachService()
        {
            RegisterService(KnownAudioService, startType: CoreServiceStartMode.Disabled, status: CoreServiceControllerStatus.Stopped);
            FakeRegistryKey serviceKey = (FakeRegistryKey)_registry.MockLocalMachine.OpenSubKey(ServiceKeyPath + "\\" + KnownAudioService, false);
            serviceKey.AddTestValue("RadeonSoftwareSlimmerOriginalStart", (int)CoreServiceStartMode.Automatic, CoreRegistryValueKind.DWord);
            ServiceListModel model = new ServiceListModel(_registry, _logger, _processRunner, _factory);
            model.LoadOrRefresh();
            model.Services.Single().Enabled = true;

            model.ApplyChanges();

            Assert.That(_processRunner.LastArguments, Does.Contain("start= auto"));
        }

        [Test]
        public void ApplyChanges_ModelDisabled_CallsDisableOnEachService()
        {
            RegisterService(KnownAudioService, startType: CoreServiceStartMode.Automatic, status: CoreServiceControllerStatus.Running);
            ServiceListModel model = new ServiceListModel(_registry, _logger, _processRunner, _factory);
            model.LoadOrRefresh();
            model.Services.Single().Enabled = false;

            model.ApplyChanges();

            Assert.That(_processRunner.LastArguments, Does.Contain("start= disabled"));
        }
    }
}
