using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using NUnit.Framework;
using RadeonSoftwareSlimmer.Models.PreInstall;

namespace RadeonSoftwareSlimmer.Test.Models.PreInstall
{
    public class DisplayComponentListModelTest
    {
        private MockFileSystem _mockFileSystem;
        private IDirectoryInfo _installerDir;

        [SetUp]
        public void SetUp()
        {
            _mockFileSystem = new MockFileSystem();
            _installerDir = _mockFileSystem.DirectoryInfo.New(@"C:\driver");
            _mockFileSystem.AddDirectory(_installerDir);
        }

        [Test]
        public void LoadOrRefresh_DirectoryDoesNotExist_ThrowsDirectoryNotFoundException()
        {
            IDirectoryInfo missingDirectory = _mockFileSystem.DirectoryInfo.New(@"C:\Directory\Does\Not\Exist");

            DisplayComponentListModel displayComponentListModel = new DisplayComponentListModel(_mockFileSystem);

            Assert.That(() => { displayComponentListModel.LoadOrRefresh(missingDirectory); }, Throws.TypeOf<DirectoryNotFoundException>());
        }

        [Test]
        public void LoadOrRefresh_MissingFiles_ListIsEmpty()
        {
            _mockFileSystem.AddFile(@"C:\driver\Packages\Drivers\Display\WT6A_INF\component1\driver.in_", new MockFileData(
                    string.Format("dummyline{0}dummyline2{0}[Strings]{0}desc\"test{0}", Environment.NewLine)));
            _mockFileSystem.AddFile(@"C:\driver\Packages\Drivers\Display\WT6A_INF\component1\driver2.in", new MockFileData(
                    string.Format("dummyline{0}dummyline2{0}[Strings]{0}desc\"test2{0}", Environment.NewLine)));
            _mockFileSystem.AddFile(@"C:\driver\Packages\Drivers\Display\WT6A_INF\component1\inf.NotIt", new MockFileData(
                    string.Format("dummyline{0}dummyline2{0}[Strings]{0}desc\"test3{0}", Environment.NewLine)));
            DisplayComponentListModel displayComponentListModel = new DisplayComponentListModel(_mockFileSystem);

            displayComponentListModel.LoadOrRefresh(_installerDir);

            List<DisplayComponentModel> displayComponentModels = new List<DisplayComponentModel>(displayComponentListModel.DisplayDriverComponents);
            Assert.That(displayComponentModels, Is.Empty);
        }

        [Test]
        public void LoadOrRefresh_MissingDirectory_ListIsEmpty()
        {
            _mockFileSystem.AddFile(@"C:\driver\Packages\Drivers\Display\W116A_INF\component1\driver.inf", new MockFileData(
                    string.Format("dummyline{0}dummyline2{0}[Strings]{0}desc\"test{0}", Environment.NewLine)));
            _mockFileSystem.AddFile(@"C:\driver\Packages\Drivers\Audio\WT6A_INF\component1\driver2.inf", new MockFileData(
                    string.Format("dummyline{0}dummyline2{0}[Strings]{0}desc\"test2{0}", Environment.NewLine)));
            _mockFileSystem.AddFile(@"C:\driver\Packages\Apps\Display\WT6A_INF\component1\driver3.inf", new MockFileData(
                    string.Format("dummyline{0}dummyline2{0}[Strings]{0}desc\"test3{0}", Environment.NewLine)));
            DisplayComponentListModel displayComponentListModel = new DisplayComponentListModel(_mockFileSystem);

            displayComponentListModel.LoadOrRefresh(_installerDir);

            List<DisplayComponentModel> displayComponentModels = new List<DisplayComponentModel>(displayComponentListModel.DisplayDriverComponents);
            Assert.That(displayComponentModels, Is.Empty);
        }

        [Test]
        public void LoadOrRefresh_SingleComponent_ReturnsOneComponent()
        {
            _mockFileSystem.AddEmptyFile(@"C:\driver\Packages\Drivers\Display\WT6A_INF\component1\ccc2_install.exe");
            _mockFileSystem.AddFile(@"C:\driver\Packages\Drivers\Display\WT6A_INF\component1\driver.inf", new MockFileData(
                    string.Format("dummyline{0}dummyline2{0}[Strings]{0}desc\"test{0}", Environment.NewLine)));
            DisplayComponentListModel displayComponentListModel = new DisplayComponentListModel(_mockFileSystem);

            displayComponentListModel.LoadOrRefresh(_installerDir);

            List<DisplayComponentModel> displayComponentModels = new List<DisplayComponentModel>(displayComponentListModel.DisplayDriverComponents);
            Assert.That(displayComponentModels, Has.Count.EqualTo(1));
        }

        [Test]
        public void LoadOrRefresh_TwoComponents_ReturnsTwoComponents()
        {
            _mockFileSystem.AddEmptyFile(@"C:\driver\Packages\Drivers\Display\WT6A_INF\component1\ccc2_install.exe");
            _mockFileSystem.AddFile(@"C:\driver\Packages\Drivers\Display\WT6A_INF\component1\driver.inf", new MockFileData(
                    string.Format("dummyline{0}dummyline2{0}[Strings]{0}desc\"test{0}", Environment.NewLine)));
            _mockFileSystem.AddFile(@"C:\driver\Packages\Drivers\Display\WT6A_INF\component2\driver.inf", new MockFileData(
                    string.Format("dummyline{0}dummyline2{0}[Strings]{0}desc\"test{0}", Environment.NewLine)));
            DisplayComponentListModel displayComponentListModel = new DisplayComponentListModel(_mockFileSystem);

            displayComponentListModel.LoadOrRefresh(_installerDir);

            List<DisplayComponentModel> displayComponentModels = new List<DisplayComponentModel>(displayComponentListModel.DisplayDriverComponents);
            Assert.That(displayComponentModels, Has.Count.EqualTo(2));
        }


        [Test]
        public void RemoveComponentsNotKeeping_KeepIsTrue_DoesNotRemoveComponent()
        {
            _mockFileSystem.AddEmptyFile(@"C:\driver\Packages\Drivers\Display\WT6A_INF\component1\ccc2_install.exe");
            _mockFileSystem.AddEmptyFile(@"C:\driver\Packages\Drivers\Display\WT6A_INF\component1\driver.inf");
            DisplayComponentListModel displayComponentListModel = new DisplayComponentListModel(_mockFileSystem);
            displayComponentListModel.LoadOrRefresh(_installerDir);
            displayComponentListModel.DisplayDriverComponents.First().Keep = true;

            displayComponentListModel.RemoveComponentsNotKeeping();

            Assert.Multiple(() =>
            {
                Assert.That(_mockFileSystem.Directory.Exists(@"C:\driver\Packages\Drivers\Display\WT6A_INF\component1\"), Is.True);
                Assert.That(_mockFileSystem.Directory.Exists(@"C:\driver\RSS_Backup\DisplayComponents\component1\"), Is.False);
            });
        }

        [Test]
        public void RemoveComponentsNotKeeping_KeepIsFalse_RemovesComponent()
        {
            _mockFileSystem.AddEmptyFile(@"C:\driver\Packages\Drivers\Display\WT6A_INF\component1\ccc2_install.exe");
            _mockFileSystem.AddEmptyFile(@"C:\driver\Packages\Drivers\Display\WT6A_INF\component1\driver.inf");
            DisplayComponentListModel displayComponentListModel = new DisplayComponentListModel(_mockFileSystem);
            displayComponentListModel.LoadOrRefresh(_installerDir);
            displayComponentListModel.DisplayDriverComponents.First().Keep = false;

            displayComponentListModel.RemoveComponentsNotKeeping();

            Assert.Multiple(() =>
            {
                Assert.That(_mockFileSystem.Directory.Exists(@"C:\driver\Packages\Drivers\Display\WT6A_INF\component1\"), Is.False);
                Assert.That(_mockFileSystem.Directory.Exists(@"C:\driver\RSS_Backup\DisplayComponents\component1\"), Is.True);
            });
        }

        [Test]
        public void RemoveComponentsNotKeeping_CanRemoveMultipleComponents()
        {
            _mockFileSystem.AddEmptyFile(@"C:\driver\Packages\Drivers\Display\WT6A_INF\component1\ccc2_install.exe");
            _mockFileSystem.AddEmptyFile(@"C:\driver\Packages\Drivers\Display\WT6A_INF\component1\driver.inf");
            _mockFileSystem.AddEmptyFile(@"C:\driver\Packages\Drivers\Display\WT6A_INF\component2\driver.inf");
            DisplayComponentListModel displayComponentListModel = new DisplayComponentListModel(_mockFileSystem);
            displayComponentListModel.LoadOrRefresh(_installerDir);
            foreach (DisplayComponentModel displayComponentModel in displayComponentListModel.DisplayDriverComponents)
            {
                displayComponentModel.Keep = false;
            }

            displayComponentListModel.RemoveComponentsNotKeeping();

            Assert.Multiple(() =>
            {
                Assert.That(_mockFileSystem.Directory.Exists(@"C:\driver\Packages\Drivers\Display\WT6A_INF\component1\"), Is.False);
                Assert.That(_mockFileSystem.Directory.Exists(@"C:\driver\Packages\Drivers\Display\WT6A_INF\component2\"), Is.False);
                Assert.That(_mockFileSystem.Directory.Exists(@"C:\driver\RSS_Backup\DisplayComponents\component1\"), Is.True);
                Assert.That(_mockFileSystem.Directory.Exists(@"C:\driver\RSS_Backup\DisplayComponents\component2\"), Is.True);
            });
        }

        [Test]
        public void RemoveComponentsNotKeeping_MultpleComponents_RemovesOnlyNotKept()
        {
            _mockFileSystem.AddEmptyFile(@"C:\driver\Packages\Drivers\Display\WT6A_INF\component1\ccc2_install.exe");
            _mockFileSystem.AddEmptyFile(@"C:\driver\Packages\Drivers\Display\WT6A_INF\component1\driver.inf");
            _mockFileSystem.AddEmptyFile(@"C:\driver\Packages\Drivers\Display\WT6A_INF\component2\driver.inf");
            DisplayComponentListModel displayComponentListModel = new DisplayComponentListModel(_mockFileSystem);
            displayComponentListModel.LoadOrRefresh(_installerDir);
            displayComponentListModel.DisplayDriverComponents.Last().Keep = false;

            displayComponentListModel.RemoveComponentsNotKeeping();

            Assert.Multiple(() =>
            {
                Assert.That(_mockFileSystem.Directory.Exists(@"C:\driver\Packages\Drivers\Display\WT6A_INF\component1\"), Is.True);
                Assert.That(_mockFileSystem.Directory.Exists(@"C:\driver\Packages\Drivers\Display\WT6A_INF\component2\"), Is.False);
                Assert.That(_mockFileSystem.Directory.Exists(@"C:\driver\RSS_Backup\DisplayComponents\component1\"), Is.False);
                Assert.That(_mockFileSystem.Directory.Exists(@"C:\driver\RSS_Backup\DisplayComponents\component2\"), Is.True);
            });
        }


        [Test]
        public void RestoreToDefault_RestoresBackedUpComponents()
        {
            _mockFileSystem.AddDirectory(@"C:\driver\Packages\Drivers\Display\WT6A_INF");
            _mockFileSystem.AddEmptyFile(@"C:\driver\RSS_Backup\DisplayComponents\component1\ccc2_install.exe");
            _mockFileSystem.AddEmptyFile(@"C:\driver\RSS_Backup\DisplayComponents\component1\driver.inf");
            _mockFileSystem.AddEmptyFile(@"C:\driver\RSS_Backup\DisplayComponents\component2\driver.inf");
            DisplayComponentListModel displayComponentListModel = new DisplayComponentListModel(_mockFileSystem);
            displayComponentListModel.LoadOrRefresh(_installerDir);

            displayComponentListModel.RestoreToDefault();

            Assert.Multiple(() =>
            {
                Assert.That(_mockFileSystem.Directory.Exists(@"C:\driver\RSS_Backup\DisplayComponents\component1"), Is.False);
                Assert.That(_mockFileSystem.Directory.Exists(@"C:\driver\RSS_Backup\DisplayComponents\component2"), Is.False);
                Assert.That(_mockFileSystem.Directory.Exists(@"C:\driver\Packages\Drivers\Display\WT6A_INF\component1"), Is.True);
                Assert.That(_mockFileSystem.File.Exists(@"C:\driver\Packages\Drivers\Display\WT6A_INF\component1\ccc2_install.exe"), Is.True);
                Assert.That(_mockFileSystem.File.Exists(@"C:\driver\Packages\Drivers\Display\WT6A_INF\component1\driver.inf"), Is.True);
                Assert.That(_mockFileSystem.File.Exists(@"C:\driver\Packages\Drivers\Display\WT6A_INF\component2\driver.inf"), Is.True);
            });
        }
    }
}
