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
        private static readonly string RSX_LAUNCHER_REG_STARTUPONCE_NAME = "UserEnabledStartupOnce";
        private static readonly object STATE_STARTUPONCE_YES = 1;
        private static readonly object STATE_STARTUPONCE_NO = 0;


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
            dummyRegistry.MockCurrentUser.AddTestSubKey(RSX_LAUNCHER_REG_PATH);
            WindowsAppStartupModel appStartup = new WindowsAppStartupModel(dummyRegistry);

            appStartup.LoadOrRefresh();

            Assert.That(appStartup.Exists, Is.True);
        }

        [Test]
        public void LoadStartupSettings_StateIsEnabled_SetsEnabledToTrue()
        {
            FakeRegistry dummyRegistry = new FakeRegistry();
            dummyRegistry.MockCurrentUser.AddTestSubKey(RSX_LAUNCHER_REG_PATH)
                .AddTestValue(RSX_LAUNCHER_REG_STATUS_NAME, STATE_ENABLED_VALUE)
                .AddTestValue(RSX_LAUNCHER_REG_STARTUPONCE_NAME, STATE_STARTUPONCE_YES);
            WindowsAppStartupModel appStartup = new WindowsAppStartupModel(dummyRegistry);

            appStartup.LoadOrRefresh();

            Assert.Multiple(() =>
            {
                Assert.That(appStartup.Exists, Is.True);
                Assert.That(appStartup.Enabled, Is.True);
            });
        }

        [Test]
        public void LoadStartupSettings_StateIsDisabled_SetsEnabledToFalse()
        {
            FakeRegistry dummyRegistry = new FakeRegistry();
            dummyRegistry.MockCurrentUser.AddTestSubKey(RSX_LAUNCHER_REG_PATH)
                .AddTestValue(RSX_LAUNCHER_REG_STATUS_NAME, STATE_DISABLED_VALUE)
                .AddTestValue(RSX_LAUNCHER_REG_STARTUPONCE_NAME, STATE_STARTUPONCE_YES);
            WindowsAppStartupModel appStartup = new WindowsAppStartupModel(dummyRegistry);

            appStartup.LoadOrRefresh();

            Assert.Multiple(() =>
            {
                Assert.That(appStartup.Exists, Is.True);
                Assert.That(appStartup.Enabled, Is.False);
            });
        }

        [Test]
        public void LoadStartupSettings_StateIsDisabledNotEnabledOnce_SetsEnabledToTrue()
        {
            FakeRegistry dummyRegistry = new FakeRegistry();
            dummyRegistry.MockCurrentUser.AddTestSubKey(RSX_LAUNCHER_REG_PATH)
                .AddTestValue(RSX_LAUNCHER_REG_STATUS_NAME, STATE_DISABLED_VALUE)
                .AddTestValue(RSX_LAUNCHER_REG_STARTUPONCE_NAME, STATE_STARTUPONCE_NO);
            WindowsAppStartupModel appStartup = new WindowsAppStartupModel(dummyRegistry);

            appStartup.LoadOrRefresh();

            Assert.Multiple(() =>
            {
                Assert.That(appStartup.Exists, Is.True);
                Assert.That(appStartup.Enabled, Is.True);
            });
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S4144:Methods should not have identical implementations", Justification = "Sonar analyzers are wrong")]
        [Test]
        public void LoadStartupSettings_StateIsDisabledEnabledOnce_SetsEnabledToFalse()
        {
            FakeRegistry dummyRegistry = new FakeRegistry();
            dummyRegistry.MockCurrentUser.AddTestSubKey(RSX_LAUNCHER_REG_PATH)
                .AddTestValue(RSX_LAUNCHER_REG_STATUS_NAME, STATE_DISABLED_VALUE)
                .AddTestValue(RSX_LAUNCHER_REG_STARTUPONCE_NAME, STATE_STARTUPONCE_YES);
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
