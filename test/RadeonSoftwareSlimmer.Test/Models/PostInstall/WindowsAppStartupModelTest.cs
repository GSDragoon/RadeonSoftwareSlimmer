using Microsoft.Win32;
using NUnit.Framework;
using RadeonSoftwareSlimmer.Models.PostInstall;
using RadeonSoftwareSlimmer.Test.TestDoubles;

namespace RadeonSoftwareSlimmer.Test.Models.PostInstall
{
    public class WindowsAppStartupModelTest
    {
        private static readonly string RSX_LAUNCHER_REG_PATH = @"Software\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\AppModel\SystemAppData\AdvancedMicroDevicesInc-RSXCM_fhmx3h6dzfmvj\launcherrsxruntimeTask";
        private static readonly string RSX_LAUNCHER_REG_STATUS_NAME = "State";
        private static readonly string RSX_LAUNCHER_REG_LASTDISABLEDTIME_NAME = "LastDisabledTime";
        private static readonly object STATE_ENABLED_VALUE = 2;
        private static readonly object STATE_DISABLED_VALUE = 1;
        //if we set this, can we bypass the reboot requirement after a new install to disalbe this right away?
        //otherwise the first reboot after a new install enables this startup task
        private static readonly string RSX_LAUNCHER_REG_STARTUPONCE_NAME = "UserEnabledStartupOnce";
        private static readonly object STATE_STARTUPONCE_YES = 1;


        [Test]
        public void LoadStartupSettings_KeyDoesNotExist_SetsExistsToFalse()
        {
            FakeRegistry dummyRegistry = new FakeRegistry();
            dummyRegistry.MockCurrentUser.AddTestSubKey(@"Some\other\key\path");
            WindowsAppStartupModel appStartup = new WindowsAppStartupModel(dummyRegistry);

            appStartup.LoadOrRefresh();

            Assert.That(appStartup.Exists, Is.False);
        }

        [Test]
        public void LoadStartupSettings_KeyDoesExist_SetsExistsToTrue()
        {
            FakeRegistry dummyRegistry = new FakeRegistry();
            dummyRegistry.MockCurrentUser.AddTestSubKey(RSX_LAUNCHER_REG_PATH)
                .AddTestValue(RSX_LAUNCHER_REG_STATUS_NAME, 123);
            WindowsAppStartupModel appStartup = new WindowsAppStartupModel(dummyRegistry);

            appStartup.LoadOrRefresh();

            Assert.That(appStartup.Exists, Is.True);
        }

        [Test]
        public void LoadStartupSettings_IsDisabled_SetsEnabledToFalse()
        {
            FakeRegistry dummyRegistry = new FakeRegistry();
            dummyRegistry.MockCurrentUser.AddTestSubKey(RSX_LAUNCHER_REG_PATH)
                .AddTestValue(RSX_LAUNCHER_REG_STATUS_NAME, STATE_DISABLED_VALUE);
            WindowsAppStartupModel appStartup = new WindowsAppStartupModel(dummyRegistry);

            appStartup.LoadOrRefresh();

            Assert.Multiple(() =>
            {
                Assert.That(appStartup.Exists, Is.True);
                Assert.That(appStartup.Enabled, Is.False);
            });
        }

        [Test]
        public void LoadStartupSettings_IsEnabled_SetsEnabledToTrue()
        {
            FakeRegistry dummyRegistry = new FakeRegistry();
            dummyRegistry.MockCurrentUser.AddTestSubKey(RSX_LAUNCHER_REG_PATH)
                .AddTestValue(RSX_LAUNCHER_REG_STATUS_NAME, STATE_ENABLED_VALUE);
            WindowsAppStartupModel appStartup = new WindowsAppStartupModel(dummyRegistry);

            appStartup.LoadOrRefresh();

            Assert.Multiple(() =>
            {
                Assert.That(appStartup.Exists, Is.True);
                Assert.That(appStartup.Enabled, Is.False);
            });
        }

        [Test]
        public void Enable_SetsEnabledValues()
        {
            FakeRegistry dummyRegistry = new FakeRegistry();
            dummyRegistry.MockCurrentUser.AddTestSubKey(RSX_LAUNCHER_REG_PATH)
                .AddTestValue(RSX_LAUNCHER_REG_STATUS_NAME, STATE_DISABLED_VALUE);
            WindowsAppStartupModel appStartup = new WindowsAppStartupModel(dummyRegistry);
            appStartup.Enabled = false;

            appStartup.Enable();

            Assert.Multiple(() =>
            {
                Assert.That(appStartup.Enabled, Is.True);

                Assert.That(dummyRegistry.MockCurrentUser.SubKeys[RSX_LAUNCHER_REG_PATH].Values.ContainsKey(RSX_LAUNCHER_REG_STATUS_NAME), Is.True);
                Assert.That(dummyRegistry.MockCurrentUser.SubKeys[RSX_LAUNCHER_REG_PATH].Values[RSX_LAUNCHER_REG_STATUS_NAME].Value, Is.EqualTo(STATE_ENABLED_VALUE));
                Assert.That(dummyRegistry.MockCurrentUser.SubKeys[RSX_LAUNCHER_REG_PATH].Values[RSX_LAUNCHER_REG_STATUS_NAME].Kind, Is.EqualTo(RegistryValueKind.DWord));
            });
        }

        [Test]
        public void Disable_SetsDisabledKeys()
        {
            FakeRegistry dummyRegistry = new FakeRegistry();
            dummyRegistry.MockCurrentUser.AddTestSubKey(RSX_LAUNCHER_REG_PATH)
                .AddTestValue(RSX_LAUNCHER_REG_STATUS_NAME, STATE_ENABLED_VALUE);
            WindowsAppStartupModel appStartup = new WindowsAppStartupModel(dummyRegistry);
            appStartup.Enabled = false;

            appStartup.Disable();

            Assert.Multiple(() =>
            {
                Assert.That(appStartup.Enabled, Is.False);

                Assert.That(dummyRegistry.MockCurrentUser.SubKeys[RSX_LAUNCHER_REG_PATH].Values.ContainsKey(RSX_LAUNCHER_REG_STATUS_NAME), Is.True);
                Assert.That(dummyRegistry.MockCurrentUser.SubKeys[RSX_LAUNCHER_REG_PATH].Values[RSX_LAUNCHER_REG_STATUS_NAME].Value, Is.EqualTo(STATE_DISABLED_VALUE));
                Assert.That(dummyRegistry.MockCurrentUser.SubKeys[RSX_LAUNCHER_REG_PATH].Values[RSX_LAUNCHER_REG_STATUS_NAME].Kind, Is.EqualTo(RegistryValueKind.DWord));

                Assert.That(dummyRegistry.MockCurrentUser.SubKeys[RSX_LAUNCHER_REG_PATH].Values.ContainsKey(RSX_LAUNCHER_REG_STARTUPONCE_NAME), Is.True);
                Assert.That(dummyRegistry.MockCurrentUser.SubKeys[RSX_LAUNCHER_REG_PATH].Values[RSX_LAUNCHER_REG_STARTUPONCE_NAME].Value, Is.EqualTo(STATE_STARTUPONCE_YES));
                Assert.That(dummyRegistry.MockCurrentUser.SubKeys[RSX_LAUNCHER_REG_PATH].Values[RSX_LAUNCHER_REG_STARTUPONCE_NAME].Kind, Is.EqualTo(RegistryValueKind.DWord));

                Assert.That(dummyRegistry.MockCurrentUser.SubKeys[RSX_LAUNCHER_REG_PATH].Values.ContainsKey(RSX_LAUNCHER_REG_LASTDISABLEDTIME_NAME), Is.True);
                Assert.That(dummyRegistry.MockCurrentUser.SubKeys[RSX_LAUNCHER_REG_PATH].Values[RSX_LAUNCHER_REG_LASTDISABLEDTIME_NAME].Kind, Is.EqualTo(RegistryValueKind.QWord));
            });
        }
    }
}
