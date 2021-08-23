using System.IO.Abstractions.TestingHelpers;
using NUnit.Framework;
using RadeonSoftwareSlimmer.Models.PreInstall;

namespace RadeonSoftwareSlimmer.Test.Models.PreInstall
{
    [TestFixture]
    public class InstallerFilesModleTest
    {
        private MockFileSystem _fileSystem;
        private InstallerFilesModel _installerFiles;


        [SetUp]
        public void Setup()
        {
            _fileSystem = new MockFileSystem();
            _installerFiles = new InstallerFilesModel(_fileSystem);
        }


        [Test]
        public void ValidateInstallerFile_FileIsNullOrEmpty_ReturnsFalse()
        {
            _installerFiles.InstallerFile = string.Empty;

            bool ret = _installerFiles.ValidateInstallerFile();

            Assert.That(ret, Is.False);
        }

        [Test]
        public void ValidateInstallerFile_FileDoesNotExist_ReturnsFalse()
        {
            _fileSystem.AddFile(@"C:\File\Does\Exist.exe", new MockFileData(string.Empty));
            _installerFiles.InstallerFile = @"C:\File\Does\NotExist.exe";

            bool ret = _installerFiles.ValidateInstallerFile();

            Assert.That(ret, Is.False);
        }

        [Test]
        public void ValidateInstallerFile_FileDoesExist_ReturnsTrue()
        {
            _fileSystem.AddFile(@"C:\File\Does\Exist.exe", new MockFileData(string.Empty));
            _installerFiles.InstallerFile = @"C:\File\Does\Exist.exe";

            bool ret = _installerFiles.ValidateInstallerFile();

            Assert.That(ret, Is.True);
        }


        [Test]
        public void ValidatePreExtractLocation_LocationIsNullOrEmpty_ReturnsFalse()
        {
            _installerFiles.ExtractedInstallerDirectory = string.Empty;

            bool ret = _installerFiles.ValidatePreExtractLocation();

            Assert.That(ret, Is.False);
        }

        [Test]
        public void ValidatePreExtractLocation_LocationDoesNotExist_ReturnsTrue()
        {
            _fileSystem.AddDirectory(@"C:\Does\Exist");
            _installerFiles.ExtractedInstallerDirectory = @"C:\Does\Not\Exist";

            bool ret = _installerFiles.ValidatePreExtractLocation();

            Assert.That(ret, Is.True);
        }

        [Test]
        public void ValidatePreExtractLocation_LocationHasFiles_ReturnsFalse()
        {
            _fileSystem.AddFile(@"C:\Does\Exist\file.something", new MockFileData(string.Empty));
            _installerFiles.ExtractedInstallerDirectory = @"C:\Does\Exist";

            bool ret = _installerFiles.ValidatePreExtractLocation();

            Assert.That(ret, Is.False);
        }

        [Test]
        public void ValidatePreExtractLocation_LocationHasFolders_ReturnsFalse()
        {
            _fileSystem.AddDirectory(@"C:\Does\Exist");
            _fileSystem.AddDirectory(@"C:\Does\Exist\ChildFolder");
            _installerFiles.ExtractedInstallerDirectory = @"C:\Does\Exist";

            bool ret = _installerFiles.ValidatePreExtractLocation();

            Assert.That(ret, Is.False);
        }

        [Test]
        public void ValidatePreExtractLocation_LocationIsEmpty_ReturnsTrue()
        {
            _fileSystem.AddDirectory(@"C:\Does\Exist");
            _installerFiles.ExtractedInstallerDirectory = @"C:\Does\Exist";

            bool ret = _installerFiles.ValidatePreExtractLocation();

            Assert.That(ret, Is.True);
        }


        [Test]
        public void ValidateExtractedLocation_LocationIsNullOrEmpty_ReturnsFalse()
        {
            _installerFiles.ExtractedInstallerDirectory = string.Empty;

            bool ret = _installerFiles.ValidateExtractedLocation();

            Assert.That(ret, Is.False);
        }

        [Test]
        public void ValidateExtractedLocation_LocationDoesNotExist_ReturnsFalse()
        {
            _fileSystem.AddDirectory(@"C:\Does\Exist");
            _installerFiles.ExtractedInstallerDirectory = @"C:\Does\Not\Exist";

            bool ret = _installerFiles.ValidateExtractedLocation();

            Assert.That(ret, Is.False);
        }

        [Test]
        public void ValidateExtractedLocation_LocationMissingSetupExe_ReturnsFalse()
        {
            _fileSystem.AddDirectory(@"C:\Does\Exist");
            _fileSystem.AddFile(@"C:\Does\Exist\Setup.bat", new MockFileData(string.Empty));
            _fileSystem.AddFile(@"C:\Does\Exist\NotSetup.exe", new MockFileData(string.Empty));
            _fileSystem.AddDirectory(@"C:\Does\Exist\Bin64");
            _fileSystem.AddDirectory(@"C:\Does\Exist\Config");
            _installerFiles.ExtractedInstallerDirectory = @"C:\Does\Exist";

            bool ret = _installerFiles.ValidateExtractedLocation();

            Assert.That(ret, Is.False);
        }

        [Test]
        public void ValidateExtractedLocation_LocationMissingBin64Folder_ReturnsFalse()
        {
            _fileSystem.AddDirectory(@"C:\Does\Exist");
            _fileSystem.AddFile(@"C:\Does\Exist\Setup.exe", new MockFileData(string.Empty));
            _fileSystem.AddDirectory(@"C:\Does\Exist\Bin");
            _fileSystem.AddDirectory(@"C:\Does\Exist\Bin32");
            _fileSystem.AddDirectory(@"C:\Does\Exist\Config");
            _installerFiles.ExtractedInstallerDirectory = @"C:\Does\Exist";

            bool ret = _installerFiles.ValidateExtractedLocation();

            Assert.That(ret, Is.False);
        }

        [Test]
        public void ValidateExtractedLocation_LocationConfigFolder_ReturnsFalse()
        {
            _fileSystem.AddDirectory(@"C:\Does\Exist");
            _fileSystem.AddFile(@"C:\Does\Exist\Setup.exe", new MockFileData(string.Empty));
            _fileSystem.AddDirectory(@"C:\Does\Exist\Bin64");
            _fileSystem.AddDirectory(@"C:\Does\Exist\Cfg");
            _fileSystem.AddDirectory(@"C:\Does\Exist\Configuration");
            _installerFiles.ExtractedInstallerDirectory = @"C:\Does\Exist";

            bool ret = _installerFiles.ValidateExtractedLocation();

            Assert.That(ret, Is.False);
        }

        [Test]
        public void ValidateExtractedLocation_LocationIsValid_ReturnsTrue()
        {
            _fileSystem.AddDirectory(@"C:\Does\Exist");
            _fileSystem.AddFile(@"C:\Does\Exist\Setup.bat", new MockFileData(string.Empty));
            _fileSystem.AddFile(@"C:\Does\Exist\Setup.exe", new MockFileData(string.Empty));
            _fileSystem.AddFile(@"C:\Does\Exist\NotSetup.exe", new MockFileData(string.Empty));
            _fileSystem.AddDirectory(@"C:\Does\Exist\Bin");
            _fileSystem.AddDirectory(@"C:\Does\Exist\Bin32");
            _fileSystem.AddDirectory(@"C:\Does\Exist\Bin64");
            _fileSystem.AddDirectory(@"C:\Does\Exist\Cfg");
            _fileSystem.AddDirectory(@"C:\Does\Exist\Config");
            _fileSystem.AddDirectory(@"C:\Does\Exist\Configuration");
            _installerFiles.ExtractedInstallerDirectory = @"C:\Does\Exist";

            bool ret = _installerFiles.ValidateExtractedLocation();

            Assert.That(ret, Is.True);
        }
    }
}
