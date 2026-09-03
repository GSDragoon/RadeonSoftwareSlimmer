using System;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using NUnit.Framework;
using RadeonSoftwareSlimmer.Core.PreInstall;
using RadeonSoftwareSlimmer.Core.Test.TestDoubles;

namespace RadeonSoftwareSlimmer.Core.Test.Models.PreInstall
{
    public class DisplayComponentModelTest
    {
        private const string ComponentRelative = @"driver\path1\path2\display\component1";
        private static readonly string ComponentRelativeDisplay = TestPath.Relative(@"path1\path2\display\component1");

        private MockFileSystem _fileSystem;
        private FakeAppLogger _logger;
        private IDirectoryInfo _rootDir;
        private IDirectoryInfo _componentDir;


        [SetUp]
        public void Setup()
        {
            _fileSystem = new MockFileSystem();
            _logger = new FakeAppLogger();
            _rootDir = _fileSystem.DirectoryInfo.New(TestPath.Rooted("driver"));
            _componentDir = _fileSystem.DirectoryInfo.New(TestPath.Rooted(ComponentRelative));
        }


        [Test]
        public void Ctor_ValidComponent_IsSuccessful()
        {
            _fileSystem.AddFile(TestPath.Rooted(ComponentRelative + @"\driver.inf"), new MockFileData(
                string.Format("dummyline{0}dummyline2{0}[Strings]{0}desc\"test{0}", Environment.NewLine)));

            DisplayComponentModel displayComponentModel = new DisplayComponentModel(_rootDir, _componentDir, _logger);

            Assert.Multiple((System.Action)(() =>
            {
                Assert.That(displayComponentModel.Description, Is.EqualTo("test"));
                Assert.That(displayComponentModel.Directory, Is.EqualTo(ComponentRelativeDisplay));
                Assert.That(displayComponentModel.InfFile, Is.EqualTo("driver.inf"));
                Assert.That(displayComponentModel.Keep, Is.True);
            }));
        }

        [Test]
        public void Ctor_ExtendedGraphics_DescriptionIsValid()
        {
            _fileSystem.AddFile(TestPath.Rooted(ComponentRelative + @"\driver.inf"), new MockFileData(
                string.Format("dummyline{0}dummyline2{0}[Strings]{0}ExtendedGraphics\"test{0}", Environment.NewLine)));

            DisplayComponentModel displayComponentModel = new DisplayComponentModel(_rootDir, _componentDir, _logger);

            Assert.Multiple((System.Action)(() =>
            {
                Assert.That(displayComponentModel.Description, Is.EqualTo("test"));
                Assert.That(displayComponentModel.Directory, Is.EqualTo(ComponentRelativeDisplay));
                Assert.That(displayComponentModel.InfFile, Is.EqualTo("driver.inf"));
                Assert.That(displayComponentModel.Keep, Is.True);
            }));
        }

        [TestCase("AMDFDANSName")]
        [TestCase("AMDOCLName")]
        [TestCase("AMDWINName")]
        public void Ctor_DescriptionName_DescriptionIsValid(string description)
        {
            _fileSystem.AddFile(TestPath.Rooted(ComponentRelative + @"\driver.inf"), new MockFileData(
                string.Format("dummyline{0}[Strings]{0}dummyline2{0}{1} = \"Test Name\"{0}dummyline3{0}", Environment.NewLine, description)));

            DisplayComponentModel displayComponentModel = new DisplayComponentModel(_rootDir, _componentDir, _logger);

            Assert.Multiple((System.Action)(() =>
            {
                Assert.That(displayComponentModel.Description, Is.EqualTo("Test Name"));
                Assert.That(displayComponentModel.Directory, Is.EqualTo(ComponentRelativeDisplay));
                Assert.That(displayComponentModel.InfFile, Is.EqualTo("driver.inf"));
                Assert.That(displayComponentModel.Keep, Is.True);
            }));
        }

        [Test]
        public void Ctor_EmptyInf_DiscriptionIsNull()
        {
            _fileSystem.AddEmptyFile(TestPath.Rooted(ComponentRelative + @"\driver.inf"));

            DisplayComponentModel displayComponentModel = new DisplayComponentModel(_rootDir, _componentDir, _logger);

            Assert.Multiple((System.Action)(() =>
            {
                Assert.That(displayComponentModel.Description, Is.Null);
                Assert.That(displayComponentModel.Directory, Is.EqualTo(ComponentRelativeDisplay));
                Assert.That(displayComponentModel.InfFile, Is.EqualTo("driver.inf"));
                Assert.That(displayComponentModel.Keep, Is.True);
            }));
        }

        [Test]
        public void Ctor_MissingStrings_DiscriptionIsNull()
        {
            _fileSystem.AddFile(TestPath.Rooted(ComponentRelative + @"\driver.inf"), new MockFileData(
                string.Format("dummyline{0}dummyline2{0}desc\"test{0}", Environment.NewLine)));

            DisplayComponentModel displayComponentModel = new DisplayComponentModel(_rootDir, _componentDir, _logger);

            Assert.Multiple((System.Action)(() =>
            {
                Assert.That(displayComponentModel.Description, Is.Null);
                Assert.That(displayComponentModel.Directory, Is.EqualTo(ComponentRelativeDisplay));
                Assert.That(displayComponentModel.InfFile, Is.EqualTo("driver.inf"));
                Assert.That(displayComponentModel.Keep, Is.True);
            }));
        }

        [Test]
        public void Ctor_MissingDescription_DiscriptionIsNull()
        {
            _fileSystem.AddFile(TestPath.Rooted(ComponentRelative + @"\driver.inf"), new MockFileData(
                string.Format("dummyline{0}dummyline2{0}[Strings]{0}\"test{0}", Environment.NewLine)));

            DisplayComponentModel displayComponentModel = new DisplayComponentModel(_rootDir, _componentDir, _logger);

            Assert.Multiple((System.Action)(() =>
            {
                Assert.That(displayComponentModel.Description, Is.Null);
                Assert.That(displayComponentModel.Directory, Is.EqualTo(ComponentRelativeDisplay));
                Assert.That(displayComponentModel.InfFile, Is.EqualTo("driver.inf"));
                Assert.That(displayComponentModel.Keep, Is.True);
            }));
        }

        [Test]
        public void Ctor_EOFAfterStrings_DiscriptionIsNull()
        {
            _fileSystem.AddFile(TestPath.Rooted(ComponentRelative + @"\driver.inf"), new MockFileData(
                string.Format("dummyline{0}dummyline2{0}[Strings]", Environment.NewLine)));

            DisplayComponentModel displayComponentModel = new DisplayComponentModel(_rootDir, _componentDir, _logger);

            Assert.Multiple((System.Action)(() =>
            {
                Assert.That(displayComponentModel.Description, Is.Null);
                Assert.That(displayComponentModel.Directory, Is.EqualTo(ComponentRelativeDisplay));
                Assert.That(displayComponentModel.InfFile, Is.EqualTo("driver.inf"));
                Assert.That(displayComponentModel.Keep, Is.True);
            }));
        }


        [Test]
        public void Remove_DirectoryDoesNotExist_DoesNothing()
        {
            _fileSystem.AddEmptyFile(TestPath.Rooted(ComponentRelative + @"\driver.inf"));
            _componentDir.Refresh();
            DisplayComponentModel displayComponentModel = new DisplayComponentModel(_rootDir, _componentDir, _logger);
            _componentDir.Delete(true);
            displayComponentModel.Keep = false;

            displayComponentModel.Remove();

            Assert.That(_fileSystem.Directory.Exists(TestPath.Rooted(ComponentRelative)), Is.False);
        }

        [Test]
        public void Remove_KeepTrue_DoesNotRemove()
        {
            _fileSystem.AddEmptyFile(TestPath.Rooted(ComponentRelative + @"\driver.inf"));
            _componentDir.Refresh();
            DisplayComponentModel displayComponentModel = new DisplayComponentModel(_rootDir, _componentDir, _logger);
            displayComponentModel.Keep = true;

            displayComponentModel.Remove();

            Assert.Multiple((System.Action)(() =>
            {
                Assert.That(_fileSystem.Directory.Exists(TestPath.Rooted(ComponentRelative)), Is.True);
                Assert.That(_fileSystem.Directory.Exists(TestPath.Rooted(@"driver\RSS_Backup\DisplayComponents\component1")), Is.False);
            }));
        }

        [Test]
        public void Remove_MovesReadonlyFiles()
        {
            _fileSystem.AddEmptyFile(TestPath.Rooted(ComponentRelative + @"\driver.inf"));
            _componentDir.Refresh();
            DisplayComponentModel displayComponentModel = new DisplayComponentModel(_rootDir, _componentDir, _logger);
            displayComponentModel.Keep = false;
            _componentDir.GetFiles()[0].IsReadOnly = true;

            displayComponentModel.Remove();

            Assert.Multiple((System.Action)(() =>
            {
                Assert.That(_fileSystem.Directory.Exists(TestPath.Rooted(ComponentRelative)), Is.False);
                Assert.That(_fileSystem.Directory.Exists(TestPath.Rooted(@"driver\RSS_Backup\DisplayComponents\component1")), Is.True);
            }));
        }

        [Test]
        public void Remove_KeepIsFalse_MovesComponent()
        {
            _fileSystem.AddEmptyFile(TestPath.Rooted(ComponentRelative + @"\driver.inf"));
            _componentDir.Refresh();
            DisplayComponentModel displayComponentModel = new DisplayComponentModel(_rootDir, _componentDir, _logger);
            displayComponentModel.Keep = false;

            displayComponentModel.Remove();

            Assert.Multiple((System.Action)(() =>
            {
                Assert.That(_fileSystem.Directory.Exists(TestPath.Rooted(ComponentRelative)), Is.False);
                Assert.That(_fileSystem.Directory.Exists(TestPath.Rooted(@"driver\RSS_Backup\DisplayComponents\component1")), Is.True);
            }));
        }
    }
}
