using System;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using NUnit.Framework;
using RadeonSoftwareSlimmer.Models.PreInstall;

namespace RadeonSoftwareSlimmer.Test.Models.PreInstall
{
    public class DisplayComponentModelTest
    {
        private MockFileSystem _fileSystem;
        private IDirectoryInfo _rootDir;
        private IDirectoryInfo _componentDir;


        [SetUp]
        public void Setup()
        {
            _fileSystem = new MockFileSystem();
            _rootDir = _fileSystem.DirectoryInfo.New(@"C:\driver");
            _componentDir = _fileSystem.DirectoryInfo.New(@"C:\driver\path1\path2\display\component1");
        }


        [Test]
        public void Ctor_ValidComponent_IsSuccessful()
        {
            _fileSystem.AddFile(@"C:\driver\path1\path2\display\component1\driver.inf", new MockFileData(
                string.Format("dummyline{0}dummyline2{0}[Strings]{0}desc\"test{0}", Environment.NewLine)));

            DisplayComponentModel displayComponentModel = new DisplayComponentModel(_rootDir, _componentDir);

            Assert.Multiple(() =>
            {
                Assert.That(displayComponentModel.Description, Is.EqualTo("test"));
                Assert.That(displayComponentModel.Directory, Is.EqualTo(@"\path1\path2\display\component1"));
                Assert.That(displayComponentModel.InfFile, Is.EqualTo("driver.inf"));
                Assert.That(displayComponentModel.Keep, Is.True);
            });
        }

        [Test]
        public void Ctor_ExtendedGraphics_DescriptionIsValid()
        {
            _fileSystem.AddFile(@"C:\driver\path1\path2\display\component1\driver.inf", new MockFileData(
                string.Format("dummyline{0}dummyline2{0}[Strings]{0}ExtendedGraphics\"test{0}", Environment.NewLine)));

            DisplayComponentModel displayComponentModel = new DisplayComponentModel(_rootDir, _componentDir);

            Assert.Multiple(() =>
            {
                Assert.That(displayComponentModel.Description, Is.EqualTo("test"));
                Assert.That(displayComponentModel.Directory, Is.EqualTo(@"\path1\path2\display\component1"));
                Assert.That(displayComponentModel.InfFile, Is.EqualTo("driver.inf"));
                Assert.That(displayComponentModel.Keep, Is.True);
            });
        }

        [Test]
        public void Ctor_EmptyInf_DiscriptionIsNull()
        {
            _fileSystem.AddEmptyFile(@"C:\driver\path1\path2\display\component1\driver.inf");

            DisplayComponentModel displayComponentModel = new DisplayComponentModel(_rootDir, _componentDir);

            Assert.Multiple(() =>
            {
                Assert.That(displayComponentModel.Description, Is.Null);
                Assert.That(displayComponentModel.Directory, Is.EqualTo(@"\path1\path2\display\component1"));
                Assert.That(displayComponentModel.InfFile, Is.EqualTo("driver.inf"));
                Assert.That(displayComponentModel.Keep, Is.True);
            });
        }

        [Test]
        public void Ctor_MissingStrings_DiscriptionIsNull()
        {
            _fileSystem.AddFile(@"C:\driver\path1\path2\display\component1\driver.inf", new MockFileData(
                string.Format("dummyline{0}dummyline2{0}desc\"test{0}", Environment.NewLine)));

            DisplayComponentModel displayComponentModel = new DisplayComponentModel(_rootDir, _componentDir);

            Assert.Multiple(() =>
            {
                Assert.That(displayComponentModel.Description, Is.Null);
                Assert.That(displayComponentModel.Directory, Is.EqualTo(@"\path1\path2\display\component1"));
                Assert.That(displayComponentModel.InfFile, Is.EqualTo("driver.inf"));
                Assert.That(displayComponentModel.Keep, Is.True);
            });
        }

        [Test]
        public void Ctor_MissingDescription_DiscriptionIsNull()
        {
            _fileSystem.AddFile(@"C:\driver\path1\path2\display\component1\driver.inf", new MockFileData(
                string.Format("dummyline{0}dummyline2{0}[Strings]{0}\"test{0}", Environment.NewLine)));

            DisplayComponentModel displayComponentModel = new DisplayComponentModel(_rootDir, _componentDir);

            Assert.Multiple(() =>
            {
                Assert.That(displayComponentModel.Description, Is.Null);
                Assert.That(displayComponentModel.Directory, Is.EqualTo(@"\path1\path2\display\component1"));
                Assert.That(displayComponentModel.InfFile, Is.EqualTo("driver.inf"));
                Assert.That(displayComponentModel.Keep, Is.True);
            });
        }

        [Test]
        public void Ctor_EOFAfterStrings_DiscriptionIsNull()
        {
            _fileSystem.AddFile(@"C:\driver\path1\path2\display\component1\driver.inf", new MockFileData(
                string.Format("dummyline{0}dummyline2{0}[Strings]", Environment.NewLine)));

            DisplayComponentModel displayComponentModel = new DisplayComponentModel(_rootDir, _componentDir);

            Assert.Multiple(() =>
            {
                Assert.That(displayComponentModel.Description, Is.Null);
                Assert.That(displayComponentModel.Directory, Is.EqualTo(@"\path1\path2\display\component1"));
                Assert.That(displayComponentModel.InfFile, Is.EqualTo("driver.inf"));
                Assert.That(displayComponentModel.Keep, Is.True);
            });
        }


        [Test]
        public void Remove_DirectoryDoesNotExist_DoesNothing()
        {
            _fileSystem.AddEmptyFile(@"C:\driver\path1\path2\display\component1\driver.inf");
            _componentDir.Refresh(); // The directory doesn't exist yet until the file has been added
            DisplayComponentModel displayComponentModel = new DisplayComponentModel(_rootDir, _componentDir);
            _componentDir.Delete(true);
            displayComponentModel.Keep = false;

            displayComponentModel.Remove();

            Assert.That(_fileSystem.Directory.Exists(@"C:\driver\path1\path2\display\component1"), Is.False);
        }

        [Test]
        public void Remove_KeepTrue_DoesNotRemove()
        {
            _fileSystem.AddEmptyFile(@"C:\driver\path1\path2\display\component1\driver.inf");
            _componentDir.Refresh();
            DisplayComponentModel displayComponentModel = new DisplayComponentModel(_rootDir, _componentDir);
            displayComponentModel.Keep = true;

            displayComponentModel.Remove();

            Assert.Multiple(() =>
            {
                Assert.That(_fileSystem.Directory.Exists(@"C:\driver\path1\path2\display\component1\"), Is.True);
                Assert.That(_fileSystem.Directory.Exists(@"C:\driver\RSS_Backup\DisplayComponents\component1\"), Is.False);
            });
        }

        [Test]
        public void Remove_MovesReadonlyFiles()
        {
            _fileSystem.AddEmptyFile(@"C:\driver\path1\path2\display\component1\driver.inf");
            _componentDir.Refresh();
            DisplayComponentModel displayComponentModel = new DisplayComponentModel(_rootDir, _componentDir);
            displayComponentModel.Keep = false;
            _componentDir.GetFiles()[0].IsReadOnly = true;

            displayComponentModel.Remove();

            Assert.Multiple(() =>
            {
                Assert.That(_fileSystem.Directory.Exists(@"C:\driver\path1\path2\display\component1\"), Is.False);
                Assert.That(_fileSystem.Directory.Exists(@"C:\driver\RSS_Backup\DisplayComponents\component1\"), Is.True);
            });
        }

        [Test]
        public void Remove_KeepIsFalse_MovesComponent()
        {
            _fileSystem.AddEmptyFile(@"C:\driver\path1\path2\display\component1\driver.inf");
            _componentDir.Refresh();
            DisplayComponentModel displayComponentModel = new DisplayComponentModel(_rootDir, _componentDir);
            displayComponentModel.Keep = false;

            displayComponentModel.Remove();

            Assert.Multiple(() =>
            {
                Assert.That(_fileSystem.Directory.Exists(@"C:\driver\path1\path2\display\component1\"), Is.False);
                Assert.That(_fileSystem.Directory.Exists(@"C:\driver\RSS_Backup\DisplayComponents\component1\"), Is.True);
            });
        }
    }
}
