using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using NUnit.Framework;
using RadeonSoftwareSlimmer.Core.PreInstall;
using RadeonSoftwareSlimmer.Core.Test.TestDoubles;

namespace RadeonSoftwareSlimmer.Core.Test.Models.PreInstall
{
    public class DisplayComponentListModelTest
    {
        private const string DriverRoot = "driver";
        private const string ComponentBase = @"driver\Packages\Drivers\Display\WT6A_INF";
        private const string BackupBase = @"driver\RSS_Backup\DisplayComponents";

        private MockFileSystem _mockFileSystem;
        private FakeAppLogger _logger;
        private IDirectoryInfo _installerDir;

        [SetUp]
        public void SetUp()
        {
            _mockFileSystem = new MockFileSystem();
            _logger = new FakeAppLogger();
            _installerDir = _mockFileSystem.DirectoryInfo.New(TestPath.Rooted(DriverRoot));
            _mockFileSystem.AddDirectory(_installerDir);
        }

        [Test]
        public void LoadOrRefresh_DirectoryDoesNotExist_ThrowsDirectoryNotFoundException()
        {
            IDirectoryInfo missingDirectory = _mockFileSystem.DirectoryInfo.New(TestPath.Rooted(@"Directory\Does\Not\Exist"));

            DisplayComponentListModel displayComponentListModel = new DisplayComponentListModel(_mockFileSystem, _logger);

            Assert.That((System.Action)(() => { displayComponentListModel.LoadOrRefresh(missingDirectory); }), Throws.TypeOf<DirectoryNotFoundException>());
        }

        [Test]
        public void LoadOrRefresh_MissingFiles_ListIsEmpty()
        {
            _mockFileSystem.AddFile(TestPath.Rooted(ComponentBase + @"\component1\driver.in_"), new MockFileData(
                    string.Format("dummyline{0}dummyline2{0}[Strings]{0}desc\"test{0}", Environment.NewLine)));
            _mockFileSystem.AddFile(TestPath.Rooted(ComponentBase + @"\component1\driver2.in"), new MockFileData(
                    string.Format("dummyline{0}dummyline2{0}[Strings]{0}desc\"test2{0}", Environment.NewLine)));
            _mockFileSystem.AddFile(TestPath.Rooted(ComponentBase + @"\component1\inf.NotIt"), new MockFileData(
                    string.Format("dummyline{0}dummyline2{0}[Strings]{0}desc\"test3{0}", Environment.NewLine)));
            DisplayComponentListModel displayComponentListModel = new DisplayComponentListModel(_mockFileSystem, _logger);

            displayComponentListModel.LoadOrRefresh(_installerDir);

            List<DisplayComponentModel> displayComponentModels = new List<DisplayComponentModel>(displayComponentListModel.DisplayDriverComponents);
            Assert.That(displayComponentModels, Is.Empty);
        }

        [Test]
        public void LoadOrRefresh_MissingDirectory_ListIsEmpty()
        {
            _mockFileSystem.AddFile(TestPath.Rooted(@"driver\Packages\Drivers\Display\W116A_INF\component1\driver.inf"), new MockFileData(
                    string.Format("dummyline{0}dummyline2{0}[Strings]{0}desc\"test{0}", Environment.NewLine)));
            _mockFileSystem.AddFile(TestPath.Rooted(@"driver\Packages\Drivers\Audio\WT6A_INF\component1\driver2.inf"), new MockFileData(
                    string.Format("dummyline{0}dummyline2{0}[Strings]{0}desc\"test2{0}", Environment.NewLine)));
            _mockFileSystem.AddFile(TestPath.Rooted(@"driver\Packages\Apps\Display\WT6A_INF\component1\driver3.inf"), new MockFileData(
                    string.Format("dummyline{0}dummyline2{0}[Strings]{0}desc\"test3{0}", Environment.NewLine)));
            DisplayComponentListModel displayComponentListModel = new DisplayComponentListModel(_mockFileSystem, _logger);

            displayComponentListModel.LoadOrRefresh(_installerDir);

            List<DisplayComponentModel> displayComponentModels = new List<DisplayComponentModel>(displayComponentListModel.DisplayDriverComponents);
            Assert.That(displayComponentModels, Is.Empty);
        }

        [Test]
        public void LoadOrRefresh_SingleComponent_ReturnsOneComponent()
        {
            _mockFileSystem.AddEmptyFile(TestPath.Rooted(ComponentBase + @"\component1\ccc2_install.exe"));
            _mockFileSystem.AddFile(TestPath.Rooted(ComponentBase + @"\component1\driver.inf"), new MockFileData(
                    string.Format("dummyline{0}dummyline2{0}[Strings]{0}desc\"test{0}", Environment.NewLine)));
            DisplayComponentListModel displayComponentListModel = new DisplayComponentListModel(_mockFileSystem, _logger);

            displayComponentListModel.LoadOrRefresh(_installerDir);

            List<DisplayComponentModel> displayComponentModels = new List<DisplayComponentModel>(displayComponentListModel.DisplayDriverComponents);
            Assert.That(displayComponentModels, Has.Count.EqualTo(1));
        }

        [Test]
        public void LoadOrRefresh_TwoComponents_ReturnsTwoComponents()
        {
            _mockFileSystem.AddEmptyFile(TestPath.Rooted(ComponentBase + @"\component1\ccc2_install.exe"));
            _mockFileSystem.AddFile(TestPath.Rooted(ComponentBase + @"\component1\driver.inf"), new MockFileData(
                    string.Format("dummyline{0}dummyline2{0}[Strings]{0}desc\"test{0}", Environment.NewLine)));
            _mockFileSystem.AddFile(TestPath.Rooted(ComponentBase + @"\component2\driver.inf"), new MockFileData(
                    string.Format("dummyline{0}dummyline2{0}[Strings]{0}desc\"test{0}", Environment.NewLine)));
            DisplayComponentListModel displayComponentListModel = new DisplayComponentListModel(_mockFileSystem, _logger);

            displayComponentListModel.LoadOrRefresh(_installerDir);

            List<DisplayComponentModel> displayComponentModels = new List<DisplayComponentModel>(displayComponentListModel.DisplayDriverComponents);
            Assert.That(displayComponentModels, Has.Count.EqualTo(2));
        }


        [Test]
        public void RemoveComponentsNotKeeping_KeepIsTrue_DoesNotRemoveComponent()
        {
            _mockFileSystem.AddEmptyFile(TestPath.Rooted(ComponentBase + @"\component1\ccc2_install.exe"));
            _mockFileSystem.AddEmptyFile(TestPath.Rooted(ComponentBase + @"\component1\driver.inf"));
            DisplayComponentListModel displayComponentListModel = new DisplayComponentListModel(_mockFileSystem, _logger);
            displayComponentListModel.LoadOrRefresh(_installerDir);
            displayComponentListModel.DisplayDriverComponents.First().Keep = true;

            displayComponentListModel.RemoveComponentsNotKeeping();

            Assert.Multiple((System.Action)(() =>
            {
                Assert.That(_mockFileSystem.Directory.Exists(TestPath.Rooted(ComponentBase + @"\component1")), Is.True);
                Assert.That(_mockFileSystem.Directory.Exists(TestPath.Rooted(BackupBase + @"\component1")), Is.False);
            }));
        }

        [Test]
        public void RemoveComponentsNotKeeping_KeepIsFalse_RemovesComponent()
        {
            _mockFileSystem.AddEmptyFile(TestPath.Rooted(ComponentBase + @"\component1\ccc2_install.exe"));
            _mockFileSystem.AddEmptyFile(TestPath.Rooted(ComponentBase + @"\component1\driver.inf"));
            DisplayComponentListModel displayComponentListModel = new DisplayComponentListModel(_mockFileSystem, _logger);
            displayComponentListModel.LoadOrRefresh(_installerDir);
            displayComponentListModel.DisplayDriverComponents.First().Keep = false;

            displayComponentListModel.RemoveComponentsNotKeeping();

            Assert.Multiple((System.Action)(() =>
            {
                Assert.That(_mockFileSystem.Directory.Exists(TestPath.Rooted(ComponentBase + @"\component1")), Is.False);
                Assert.That(_mockFileSystem.Directory.Exists(TestPath.Rooted(BackupBase + @"\component1")), Is.True);
            }));
        }

        [Test]
        public void RemoveComponentsNotKeeping_CanRemoveMultipleComponents()
        {
            _mockFileSystem.AddEmptyFile(TestPath.Rooted(ComponentBase + @"\component1\ccc2_install.exe"));
            _mockFileSystem.AddEmptyFile(TestPath.Rooted(ComponentBase + @"\component1\driver.inf"));
            _mockFileSystem.AddEmptyFile(TestPath.Rooted(ComponentBase + @"\component2\driver.inf"));
            DisplayComponentListModel displayComponentListModel = new DisplayComponentListModel(_mockFileSystem, _logger);
            displayComponentListModel.LoadOrRefresh(_installerDir);
            foreach (DisplayComponentModel displayComponentModel in displayComponentListModel.DisplayDriverComponents)
            {
                displayComponentModel.Keep = false;
            }

            displayComponentListModel.RemoveComponentsNotKeeping();

            Assert.Multiple((System.Action)(() =>
            {
                Assert.That(_mockFileSystem.Directory.Exists(TestPath.Rooted(ComponentBase + @"\component1")), Is.False);
                Assert.That(_mockFileSystem.Directory.Exists(TestPath.Rooted(ComponentBase + @"\component2")), Is.False);
                Assert.That(_mockFileSystem.Directory.Exists(TestPath.Rooted(BackupBase + @"\component1")), Is.True);
                Assert.That(_mockFileSystem.Directory.Exists(TestPath.Rooted(BackupBase + @"\component2")), Is.True);
            }));
        }

        [Test]
        public void RemoveComponentsNotKeeping_MultpleComponents_RemovesOnlyNotKept()
        {
            _mockFileSystem.AddEmptyFile(TestPath.Rooted(ComponentBase + @"\component1\ccc2_install.exe"));
            _mockFileSystem.AddEmptyFile(TestPath.Rooted(ComponentBase + @"\component1\driver.inf"));
            _mockFileSystem.AddEmptyFile(TestPath.Rooted(ComponentBase + @"\component2\driver.inf"));
            DisplayComponentListModel displayComponentListModel = new DisplayComponentListModel(_mockFileSystem, _logger);
            displayComponentListModel.LoadOrRefresh(_installerDir);
            displayComponentListModel.DisplayDriverComponents.Last().Keep = false;

            displayComponentListModel.RemoveComponentsNotKeeping();

            Assert.Multiple((System.Action)(() =>
            {
                Assert.That(_mockFileSystem.Directory.Exists(TestPath.Rooted(ComponentBase + @"\component1")), Is.True);
                Assert.That(_mockFileSystem.Directory.Exists(TestPath.Rooted(ComponentBase + @"\component2")), Is.False);
                Assert.That(_mockFileSystem.Directory.Exists(TestPath.Rooted(BackupBase + @"\component1")), Is.False);
                Assert.That(_mockFileSystem.Directory.Exists(TestPath.Rooted(BackupBase + @"\component2")), Is.True);
            }));
        }


        [Test]
        public void RestoreToDefault_RestoresBackedUpComponents()
        {
            _mockFileSystem.AddDirectory(TestPath.Rooted(ComponentBase));
            _mockFileSystem.AddEmptyFile(TestPath.Rooted(BackupBase + @"\component1\ccc2_install.exe"));
            _mockFileSystem.AddEmptyFile(TestPath.Rooted(BackupBase + @"\component1\driver.inf"));
            _mockFileSystem.AddEmptyFile(TestPath.Rooted(BackupBase + @"\component2\driver.inf"));
            DisplayComponentListModel displayComponentListModel = new DisplayComponentListModel(_mockFileSystem, _logger);
            displayComponentListModel.LoadOrRefresh(_installerDir);

            displayComponentListModel.RestoreToDefault();

            Assert.Multiple((System.Action)(() =>
            {
                Assert.That(_mockFileSystem.Directory.Exists(TestPath.Rooted(BackupBase + @"\component1")), Is.False);
                Assert.That(_mockFileSystem.Directory.Exists(TestPath.Rooted(BackupBase + @"\component2")), Is.False);
                Assert.That(_mockFileSystem.Directory.Exists(TestPath.Rooted(ComponentBase + @"\component1")), Is.True);
                Assert.That(_mockFileSystem.File.Exists(TestPath.Rooted(ComponentBase + @"\component1\ccc2_install.exe")), Is.True);
                Assert.That(_mockFileSystem.File.Exists(TestPath.Rooted(ComponentBase + @"\component1\driver.inf")), Is.True);
                Assert.That(_mockFileSystem.File.Exists(TestPath.Rooted(ComponentBase + @"\component2\driver.inf")), Is.True);
            }));
        }
    }
}
