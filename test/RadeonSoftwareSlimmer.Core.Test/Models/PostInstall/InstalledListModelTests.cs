using System.Linq;
using NUnit.Framework;
using RadeonSoftwareSlimmer.Core.Enums;
using RadeonSoftwareSlimmer.Core.PostInstall;
using RadeonSoftwareSlimmer.Core.Test.TestDoubles;

namespace RadeonSoftwareSlimmer.Core.Test.Models.PostInstall
{
    public class InstalledListModelTests
    {
        private const string UninstallPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
        private const string Wow6432UninstallPath = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
        private const string AmdPublisher = "Advanced Micro Devices, Inc.";

        private FakeRegistry _registry;
        private FakeAppLogger _logger;
        private FakeProcessRunner _processRunner;

        [SetUp]
        public void Setup()
        {
            _registry = new FakeRegistry();
            _logger = new FakeAppLogger();
            _processRunner = new FakeProcessRunner();

            // Model unconditionally iterates both hives on a 64-bit OS; empty stubs prevent NREs.
            _registry.MockLocalMachine.AddTestSubKey(UninstallPath);
            _registry.MockLocalMachine.AddTestSubKey(Wow6432UninstallPath);
        }


        private FakeRegistryKey AddUninstallEntry(string subKeyName, string publisher, string displayName, bool underWow6432 = false)
        {
            string basePath = underWow6432 ? Wow6432UninstallPath : UninstallPath;
            FakeRegistryKey entry = _registry.MockLocalMachine
                .OpenSubKey(basePath, false) is FakeRegistryKey root
                    ? root.AddTestSubKey(subKeyName)
                    : throw new System.InvalidOperationException("Uninstall root key missing");

            if (publisher != null)
                entry.AddTestValue("Publisher", publisher);
            if (displayName != null)
                entry.AddTestValue("DisplayName", displayName);

            return entry;
        }


        [Test]
        public void LoadOrRefresh_NoUninstallEntries_EmptyList()
        {
            InstalledListModel model = new InstalledListModel(_registry, _logger, _processRunner);

            model.LoadOrRefresh();

            Assert.That(model.InstalledItems, Is.Empty);
        }

        [Test]
        public void LoadOrRefresh_EntryWithAmdPublisher_IsIncluded()
        {
            AddUninstallEntry("RadeonSoftware", AmdPublisher, "AMD Radeon Software");
            InstalledListModel model = new InstalledListModel(_registry, _logger, _processRunner);

            model.LoadOrRefresh();

            Assert.That(model.InstalledItems.Count(), Is.EqualTo(1));
        }

        [Test]
        public void LoadOrRefresh_AmdPublisherMatchIsCaseInsensitive()
        {
            AddUninstallEntry("RadeonSoftware", "advanced micro devices, inc.", "AMD Radeon Software");
            InstalledListModel model = new InstalledListModel(_registry, _logger, _processRunner);

            model.LoadOrRefresh();

            Assert.That(model.InstalledItems.Count(), Is.EqualTo(1));
        }

        [Test]
        public void LoadOrRefresh_EntryWithDifferentPublisher_IsExcluded()
        {
            AddUninstallEntry("MicrosoftUpdate", "Microsoft Corporation", "Microsoft Update");
            InstalledListModel model = new InstalledListModel(_registry, _logger, _processRunner);

            model.LoadOrRefresh();

            Assert.That(model.InstalledItems, Is.Empty);
        }

        [Test]
        public void LoadOrRefresh_EntryWithNullPublisher_IsExcluded()
        {
            AddUninstallEntry("NoPublisher", publisher: null, displayName: "Some Thing");
            InstalledListModel model = new InstalledListModel(_registry, _logger, _processRunner);

            model.LoadOrRefresh();

            Assert.That(model.InstalledItems, Is.Empty);
        }

        [Test]
        public void LoadOrRefresh_EntryWithNullDisplayName_IsExcluded()
        {
            AddUninstallEntry("NoDisplayName", publisher: AmdPublisher, displayName: null);
            InstalledListModel model = new InstalledListModel(_registry, _logger, _processRunner);

            model.LoadOrRefresh();

            Assert.That(model.InstalledItems, Is.Empty);
        }

        [TestCase("Chipset")]
        [TestCase("GPIO")]
        [TestCase("PCI")]
        [TestCase("PSP")]
        [TestCase("Ryzen")]
        [TestCase("SMBus")]
        [TestCase("3D V-Cache")]
        [TestCase("AMD Application Compatibility Database Driver")]
        [TestCase("PPM Provisioning")]
        [TestCase("AMD Interface")]
        [TestCase("I2C")]
        public void LoadOrRefresh_DisplayNameContainsChipsetKeyword_IsExcluded(string chipsetSubstring)
        {
            AddUninstallEntry("AmdChipsetSomething", AmdPublisher, $"AMD {chipsetSubstring} Driver");
            InstalledListModel model = new InstalledListModel(_registry, _logger, _processRunner);

            model.LoadOrRefresh();

            Assert.That(model.InstalledItems, Is.Empty);
        }

        [Test]
        public void LoadOrRefresh_ScansBothUninstallHivesOn64BitOs()
        {
            AddUninstallEntry("RadeonSoftware", AmdPublisher, "AMD Radeon Software");
            AddUninstallEntry("RadeonWow64", AmdPublisher, "AMD Radeon 32-bit Component", underWow6432: true);
            InstalledListModel model = new InstalledListModel(_registry, _logger, _processRunner);

            model.LoadOrRefresh();

            Assert.That(model.InstalledItems.Count(), Is.EqualTo(2));
        }


        [Test]
        public void ApplyChanges_UninstallTrue_RunsUninstaller()
        {
            FakeRegistryKey entry = AddUninstallEntry("RadeonSoftware", AmdPublisher, "AMD Radeon Software");
            entry.AddTestValue("WindowsInstaller", "1");
            entry.SetTestName(System.Guid.NewGuid().ToString("B"));
            InstalledListModel model = new InstalledListModel(_registry, _logger, _processRunner);
            model.LoadOrRefresh();
            model.InstalledItems.Single().Uninstall = true;

            model.ApplyChanges();

            Assert.That(_processRunner.LastFileName, Does.EndWith("msiexec.exe"));
        }

        [Test]
        public void ApplyChanges_UninstallFalse_DoesNotRunUninstaller()
        {
            AddUninstallEntry("RadeonSoftware", AmdPublisher, "AMD Radeon Software");
            InstalledListModel model = new InstalledListModel(_registry, _logger, _processRunner);
            model.LoadOrRefresh();
            model.InstalledItems.Single().Uninstall = false;

            model.ApplyChanges();

            Assert.That(_processRunner.LastFileName, Is.Null);
        }
    }
}
