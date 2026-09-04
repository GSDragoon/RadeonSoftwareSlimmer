using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using NUnit.Framework;
using RadeonSoftwareSlimmer.Core.PostInstall;
using RadeonSoftwareSlimmer.Core.Test.TestDoubles;

namespace RadeonSoftwareSlimmer.Core.Test.Models.PostInstall
{
    public class HostServicesModelTests
    {
        // Registry keys are Windows registry paths (not filesystem paths), so backslashes are correct regardless of OS.
        private const string SoftwareKey = "SOFTWARE";
        private const string AmdKey = "AMD";
        private const string CnKey = "CN";
        private const string InstallDirValue = "InstallDir";
        private const string CncmdFileName = "cncmd.exe";

        private static readonly string RadeonInstallDir = TestPath.Rooted(@"Program Files\AMD\CNext\CNext");

        private MockFileSystem _fileSystem;
        private FakeRegistry _registry;
        private FakeAppLogger _logger;
        private FakeProcessHandler _processHandler;
        private FakeProcessRunner _processRunner;

        [SetUp]
        public void Setup()
        {
            _fileSystem = new MockFileSystem();
            _registry = new FakeRegistry();
            _logger = new FakeAppLogger();
            _processHandler = new FakeProcessHandler();
            _processRunner = new FakeProcessRunner();
        }


        [Test]
        public void Ctor_InstalledIsFalse()
        {
            HostServiceModel hostServiceModel = new HostServiceModel(_fileSystem, _registry, _logger, _processHandler, _processRunner);

            Assert.That(hostServiceModel.Installed, Is.False);
        }


        [Test]
        public void LoadOrRefresh_MissingAMDRegistryKeyFileDoNotExist_InstalledIsFalse()
        {
            _registry.MockLocalMachine.AddTestSubKey(SoftwareKey);
            HostServiceModel hostServiceModel = new HostServiceModel(_fileSystem, _registry, _logger, _processHandler, _processRunner);

            hostServiceModel.LoadOrRefresh();

            Assert.That(hostServiceModel.Installed, Is.False);
        }

        [Test]
        public void LoadOrRefresh_MissingCNRegistryKeyFileDoNotExist_InstalledIsFalse()
        {
            _registry.MockLocalMachine.AddTestSubKey(SoftwareKey).AddTestSubKey(AmdKey);
            HostServiceModel hostServiceModel = new HostServiceModel(_fileSystem, _registry, _logger, _processHandler, _processRunner);

            hostServiceModel.LoadOrRefresh();

            Assert.That(hostServiceModel.Installed, Is.False);
        }

        [Test]
        public void LoadOrRefresh_MissingInstallDirValueFileDoNotExist_InstalledIsFalse()
        {
            _registry.MockLocalMachine.AddTestSubKey(SoftwareKey).AddTestSubKey(AmdKey).AddTestSubKey(CnKey);
            HostServiceModel hostServiceModel = new HostServiceModel(_fileSystem, _registry, _logger, _processHandler, _processRunner);

            hostServiceModel.LoadOrRefresh();

            Assert.That(hostServiceModel.Installed, Is.False);
        }

        [Test]
        public void LoadOrRefresh_RegistryValueExistsDirectoryDoesNotExist_InstalledIsFalse()
        {
            // https://github.com/GSDragoon/RadeonSoftwareSlimmer/discussions/37
            _registry.MockLocalMachine.AddTestSubKey(SoftwareKey).AddTestSubKey(AmdKey).AddTestSubKey(CnKey)
                .AddTestValue(InstallDirValue, RadeonInstallDir);
            HostServiceModel hostServiceModel = new HostServiceModel(_fileSystem, _registry, _logger, _processHandler, _processRunner);

            hostServiceModel.LoadOrRefresh();

            Assert.That(hostServiceModel.Installed, Is.False);
        }

        [Test]
        public void LoadOrRefresh_RegistryValueExistsFileDoesNotExist_InstalledIsFalse()
        {
            _registry.MockLocalMachine.AddTestSubKey(SoftwareKey).AddTestSubKey(AmdKey).AddTestSubKey(CnKey)
                .AddTestValue(InstallDirValue, RadeonInstallDir);
            _fileSystem.AddDirectory(RadeonInstallDir);
            HostServiceModel hostServiceModel = new HostServiceModel(_fileSystem, _registry, _logger, _processHandler, _processRunner);

            hostServiceModel.LoadOrRefresh();

            Assert.That(hostServiceModel.Installed, Is.False);
        }

        [Test]
        public void LoadOrRefresh_RegistryValueExistsFileExists_InstalledIsTrue()
        {
            _registry.MockLocalMachine.AddTestSubKey(SoftwareKey).AddTestSubKey(AmdKey).AddTestSubKey(CnKey)
                .AddTestValue(InstallDirValue, RadeonInstallDir);
            _fileSystem.AddDirectory(RadeonInstallDir);
            _fileSystem.AddEmptyFile(_fileSystem.Path.Combine(RadeonInstallDir, CncmdFileName));
            HostServiceModel hostServiceModel = new HostServiceModel(_fileSystem, _registry, _logger, _processHandler, _processRunner);

            hostServiceModel.LoadOrRefresh();

            Assert.That(hostServiceModel.Installed, Is.True);
        }

        [Test]
        public void LoadOrRefresh_MissingRegistryKeyDefaultFileExist_InstalledIsTrue()
        {
            _registry.MockLocalMachine.AddTestSubKey(SoftwareKey);
            // Model falls back to ProgramFiles\AMD\CNext\CNext. Test mirrors that composition so it works on any OS.
            string defaultDir = _fileSystem.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles), "AMD", "CNext", "CNext");
            _fileSystem.AddDirectory(defaultDir);
            _fileSystem.AddEmptyFile(_fileSystem.Path.Combine(defaultDir, CncmdFileName));

            HostServiceModel hostServiceModel = new HostServiceModel(_fileSystem, _registry, _logger, _processHandler, _processRunner);

            hostServiceModel.LoadOrRefresh();

            Assert.That(hostServiceModel.Installed, Is.True);
        }


        [Test]
        public void LoadOrRefresh_PopulatesKnownHostServiceList()
        {
            HostServiceModel hostServiceModel = new HostServiceModel(_fileSystem, _registry, _logger, _processHandler, _processRunner);

            hostServiceModel.LoadOrRefresh();

            System.Collections.Generic.IList<RunningHostServiceModel> services = hostServiceModel.HostServices;
            Assert.That(services.Select(s => s.Name), Is.EquivalentTo(new[] { "RadeonSoftware", "AMDRSServ", "amdow", "AMDRSSrcExt" }));
        }


        [Test]
        public void StopRadeonSoftware_NotInstalled_DoesNothing()
        {
            HostServiceModel hostServiceModel = new HostServiceModel(_fileSystem, _registry, _logger, _processHandler, _processRunner);
            hostServiceModel.LoadOrRefresh();

            hostServiceModel.StopRadeonSoftware();

            Assert.That(_processRunner.LastFileName, Is.Null);
        }

        [Test]
        public void StopRadeonSoftware_Installed_RunsCncmdExitAndWaitsForHostProcesses()
        {
            _registry.MockLocalMachine.AddTestSubKey(SoftwareKey).AddTestSubKey(AmdKey).AddTestSubKey(CnKey)
                .AddTestValue(InstallDirValue, RadeonInstallDir);
            _fileSystem.AddDirectory(RadeonInstallDir);
            _fileSystem.AddEmptyFile(_fileSystem.Path.Combine(RadeonInstallDir, CncmdFileName));
            _processHandler.RunningProcesses.Add("AMDRSServ");
            _processHandler.RunningProcesses.Add("RadeonSoftware");
            HostServiceModel hostServiceModel = new HostServiceModel(_fileSystem, _registry, _logger, _processHandler, _processRunner);
            hostServiceModel.LoadOrRefresh();

            hostServiceModel.StopRadeonSoftware();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_processRunner.LastFileName, Does.EndWith(CncmdFileName));
                Assert.That(_processRunner.LastArguments, Is.EqualTo("exit"));
                Assert.That(_processHandler.RunningProcesses, Does.Not.Contain("AMDRSServ"));
                Assert.That(_processHandler.RunningProcesses, Does.Not.Contain("RadeonSoftware"));
            }
        }


        [Test]
        public void RestartRadeonSoftware_NotInstalled_DoesNothing()
        {
            HostServiceModel hostServiceModel = new HostServiceModel(_fileSystem, _registry, _logger, _processHandler, _processRunner);
            hostServiceModel.LoadOrRefresh();

            hostServiceModel.RestartRadeonSoftware();

            Assert.That(_processRunner.LastFileName, Is.Null);
        }

        [Test]
        public void RestartRadeonSoftware_Installed_RunsCncmdRestartAndWaitsForRadeonSoftware()
        {
            _registry.MockLocalMachine.AddTestSubKey(SoftwareKey).AddTestSubKey(AmdKey).AddTestSubKey(CnKey)
                .AddTestValue(InstallDirValue, RadeonInstallDir);
            _fileSystem.AddDirectory(RadeonInstallDir);
            _fileSystem.AddEmptyFile(_fileSystem.Path.Combine(RadeonInstallDir, CncmdFileName));
            HostServiceModel hostServiceModel = new HostServiceModel(_fileSystem, _registry, _logger, _processHandler, _processRunner);
            hostServiceModel.LoadOrRefresh();

            hostServiceModel.RestartRadeonSoftware();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_processRunner.LastFileName, Does.EndWith(CncmdFileName));
                Assert.That(_processRunner.LastArguments, Is.EqualTo("restart"));
                Assert.That(_processHandler.RunningProcesses, Does.Contain("RadeonSoftware"));
            }
        }
    }
}
