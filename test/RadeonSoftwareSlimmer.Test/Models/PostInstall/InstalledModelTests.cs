﻿using System;
using System.IO;
using NUnit.Framework;
using RadeonSoftwareSlimmer.Models.PostInstall;
using RadeonSoftwareSlimmer.Test.TestDoubles;

namespace RadeonSoftwareSlimmer.Test.Models.PostInstall
{
    public class InstalledModelTests
    {
        [Test]
        public void Ctor_LoadsRegistryValues()
        {
            FakeRegistryKey installKey = new FakeRegistryKey()
                .AddTestValue("DisplayName", "DisplayName Value")
                .AddTestValue("Publisher", "Publisher Value")
                .AddTestValue("DisplayVersion", "DisplayVersion Value")
                .AddTestValue("UninstallString", "UninstallString Value")
                .AddTestValue("WindowsInstaller", 0);

            InstalledModel installedModel = new InstalledModel(installKey, "Test");

            Assert.Multiple(() =>
            {
                Assert.That(installedModel, Is.Not.Null);
                Assert.That(installedModel.Uninstall, Is.False);

                Assert.That(installedModel.ProductCode, Is.EqualTo("Test"));
                Assert.That(installedModel.DisplayName, Is.EqualTo("DisplayName Value"));
                Assert.That(installedModel.Publisher, Is.EqualTo("Publisher Value"));
                Assert.That(installedModel.DisplayVersion, Is.EqualTo("DisplayVersion Value"));
                Assert.That(installedModel.UninstallCommand, Is.EqualTo("UninstallString Value"));
            });
        }

        [Test]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("System.IO.Abstractions", "IO0006:Replace Path class with IFileSystem.Path for improved testability", Justification = "Just combining a path")]
        public void Ctor_MsiInstaller_GeneratesUninstallCommand()
        {
            string keyName = Guid.NewGuid().ToString("B");
            FakeRegistryKey installKey = new FakeRegistryKey()
                .SetTestName(keyName)
                .AddTestValue("UninstallString", "UninstallString Value")
                .AddTestValue("WindowsInstaller", 1);

            InstalledModel installedModel = new InstalledModel(installKey, keyName);

            Assert.Multiple(() =>
            {
                Assert.That(installedModel, Is.Not.Null);
                Assert.That(installedModel.Uninstall, Is.False);

                Assert.That(installedModel.ProductCode, Is.EqualTo(keyName));
                string msiexec = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System, Environment.SpecialFolderOption.DoNotVerify), "msiexec.exe");
                Assert.That(installedModel.UninstallCommand, Is.EqualTo($"{msiexec} /uninstall {keyName}"));
            });
        }
    }
}
