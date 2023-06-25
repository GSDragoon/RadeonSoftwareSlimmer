using System.IO.Abstractions.TestingHelpers;
using NUnit.Framework;
using RadeonSoftwareSlimmer.Models.PostInstall;
using RadeonSoftwareSlimmer.Test.TestDoubles;

namespace RadeonSoftwareSlimmer.Test.Models.PostInstall
{
    public class HostServicesModelTests
    {
        private MockFileSystem _fileSystem;
        private FakeRegistry _registry;

        [SetUp]
        public void Setup()
        {
            _fileSystem = new MockFileSystem();
            _registry = new FakeRegistry();
        }


        [Test]
        public void Ctor_InstalledIsFalse()
        {
            HostServiceModel hostServiceModel = new HostServiceModel(_fileSystem, _registry);

            Assert.That(hostServiceModel.Installed, Is.False);
        }


        [Test]
        public void LoadOrRefresh_MissingAMDRegistryKeyFileDoNotExist_InstalledIsFalse()
        {
            _registry.MockLocalMachine.AddTestSubKey("SOFTWARE");
            _fileSystem.AddDirectory(@"C:\Program Files");
            HostServiceModel hostServiceModel = new HostServiceModel(_fileSystem, _registry);

            hostServiceModel.LoadOrRefresh();

            Assert.That(hostServiceModel.Installed, Is.False);
        }

        [Test]
        public void LoadOrRefresh_MissingCNRegistryKeyFileDoNotExist_InstalledIsFalse()
        {
            _registry.MockLocalMachine.AddTestSubKey("SOFTWARE").AddTestSubKey("AMD");
            _fileSystem.AddDirectory(@"C:\Program Files");
            HostServiceModel hostServiceModel = new HostServiceModel(_fileSystem, _registry);

            hostServiceModel.LoadOrRefresh();

            Assert.That(hostServiceModel.Installed, Is.False);
        }

        [Test]
        public void LoadOrRefresh_MissingInstallDirValueFileDoNotExist_InstalledIsFalse()
        {
            _registry.MockLocalMachine.AddTestSubKey("SOFTWARE").AddTestSubKey("AMD").AddTestSubKey("CN");
            _fileSystem.AddDirectory(@"C:\Program Files");
            HostServiceModel hostServiceModel = new HostServiceModel(_fileSystem, _registry);

            hostServiceModel.LoadOrRefresh();

            Assert.That(hostServiceModel.Installed, Is.False);
        }

        [Test]
        public void LoadOrRefresh_RegistryValueExistsDirectoryDoesNotExist_InstalledIsFalse()
        {
            // I believe this is the exact scenario for this issue
            // https://github.com/GSDragoon/RadeonSoftwareSlimmer/discussions/37
            _registry.MockLocalMachine.AddTestSubKey("SOFTWARE").AddTestSubKey("AMD").AddTestSubKey("CN").
                AddTestValue("InstallDir", @"C:\Program Files\AMD\CNext\CNext\");
            _fileSystem.AddDirectory(@"C:\Program Files");
            HostServiceModel hostServiceModel = new HostServiceModel(_fileSystem, _registry);

            hostServiceModel.LoadOrRefresh();

            Assert.That(hostServiceModel.Installed, Is.False);
        }

        [Test]
        public void LoadOrRefresh_RegistryValueExistsFileDoesNotExist_InstalledIsFalse()
        {
            _registry.MockLocalMachine.AddTestSubKey("SOFTWARE").AddTestSubKey("AMD").AddTestSubKey("CN").
                AddTestValue("InstallDir", @"C:\Program Files\AMD\CNext\CNext\");
            _fileSystem.AddDirectory(@"C:\Program Files\AMD\CNext\CNext");
            HostServiceModel hostServiceModel = new HostServiceModel(_fileSystem, _registry);

            hostServiceModel.LoadOrRefresh();

            Assert.That(hostServiceModel.Installed, Is.False);
        }

        [Test]
        public void LoadOrRefresh_RegistryValueExistsFileExists_InstalledIsTrue()
        {
            _registry.MockLocalMachine.AddTestSubKey("SOFTWARE").AddTestSubKey("AMD").AddTestSubKey("CN").
                AddTestValue("InstallDir", @"C:\Program Files\AMD\CNext\CNext\");
            _fileSystem.AddDirectory(@"C:\Program Files\AMD\CNext\CNext");
            _fileSystem.AddEmptyFile(@"C:\Program Files\AMD\CNext\CNext\cncmd.exe");
            HostServiceModel hostServiceModel = new HostServiceModel(_fileSystem, _registry);

            hostServiceModel.LoadOrRefresh();

            Assert.That(hostServiceModel.Installed, Is.True);
        }

        [Test]
        public void LoadOrRefresh_MissingRegistryKeyDefaultFileExist_InstalledIsTrue()
        {
            _registry.MockLocalMachine.AddTestSubKey("SOFTWARE");
            _fileSystem.AddDirectory(@"C:\Program Files");
            _fileSystem.AddDirectory(@"C:\Program Files\AMD\CNext\CNext");
            _fileSystem.AddEmptyFile(@"C:\Program Files\AMD\CNext\CNext\cncmd.exe");

            HostServiceModel hostServiceModel = new HostServiceModel(_fileSystem, _registry);

            hostServiceModel.LoadOrRefresh();

            Assert.That(hostServiceModel.Installed, Is.True);
        }
    }
}
