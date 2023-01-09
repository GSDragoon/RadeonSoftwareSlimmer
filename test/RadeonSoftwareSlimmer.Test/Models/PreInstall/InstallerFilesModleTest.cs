using System;
using System.IO.Abstractions.TestingHelpers;
using NUnit.Framework;
using RadeonSoftwareSlimmer.Models.PreInstall;

namespace RadeonSoftwareSlimmer.Test.Models.PreInstall
{
    public class InstallerFilesModleTest
    {
        private MockFileSystem _fileSystem;
        private InstallerFilesModel _installerFiles;
        //Contents of the files doesn't matter for any of these tests
        private readonly MockFileData _emptyFileData = new MockFileData(Array.Empty<byte>());


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

            Assert.That(_installerFiles.ValidateInstallerFile(), Is.False);
        }

        [Test]
        public void ValidateInstallerFile_FileDoesNotExist_ReturnsFalse()
        {
            _fileSystem.AddFile(@"C:\File\Does\Exist.exe", _emptyFileData);
            _installerFiles.InstallerFile = @"C:\File\Does\NotExist.exe";

            Assert.That(_installerFiles.ValidateInstallerFile(), Is.False);
        }

        [Test]
        public void ValidateInstallerFile_FileDoesExist_ReturnsTrue()
        {
            _fileSystem.AddFile(@"C:\File\Does\Exist.exe", _emptyFileData);
            _installerFiles.InstallerFile = @"C:\File\Does\Exist.exe";

            Assert.That(_installerFiles.ValidateInstallerFile(), Is.True);
        }


        [Test]
        public void ValidatePreExtractLocation_LocationIsNullOrEmpty_ReturnsFalse()
        {
            _installerFiles.ExtractedInstallerDirectory = string.Empty;

            Assert.That(_installerFiles.ValidatePreExtractLocation(), Is.False);
        }

        [Test]
        public void ValidatePreExtractLocation_LocationDoesNotExist_ReturnsTrue()
        {
            _fileSystem.AddDirectory(@"C:\Does\Exist");
            _installerFiles.ExtractedInstallerDirectory = @"C:\Does\Not\Exist";

            Assert.That(_installerFiles.ValidatePreExtractLocation(), Is.True);
        }

        [Test]
        public void ValidatePreExtractLocation_LocationHasFiles_ReturnsFalse()
        {
            _fileSystem.AddFile(@"C:\Does\Exist\file.something", _emptyFileData);
            _installerFiles.ExtractedInstallerDirectory = @"C:\Does\Exist";

            Assert.That(_installerFiles.ValidatePreExtractLocation(), Is.False);
        }

        [Test]
        public void ValidatePreExtractLocation_LocationHasFolders_ReturnsFalse()
        {
            _fileSystem.AddDirectory(@"C:\Does\Exist");
            _fileSystem.AddDirectory(@"C:\Does\Exist\ChildFolder");
            _installerFiles.ExtractedInstallerDirectory = @"C:\Does\Exist";

            Assert.That(_installerFiles.ValidatePreExtractLocation(), Is.False);
        }

        [Test]
        public void ValidatePreExtractLocation_LocationIsEmpty_ReturnsTrue()
        {
            _fileSystem.AddDirectory(@"C:\Does\Exist");
            _installerFiles.ExtractedInstallerDirectory = @"C:\Does\Exist";

            Assert.That(_installerFiles.ValidatePreExtractLocation(), Is.True);
        }


        [Test]
        public void ValidateExtractedLocation_LocationIsNullOrEmpty_ReturnsFalse()
        {
            _installerFiles.ExtractedInstallerDirectory = string.Empty;

            Assert.That(_installerFiles.ValidateExtractedLocation, Is.False);
        }

        [Test]
        public void ValidateExtractedLocation_LocationDoesNotExist_ReturnsFalse()
        {
            _fileSystem.AddDirectory(@"C:\Does\Exist");
            _installerFiles.ExtractedInstallerDirectory = @"C:\Does\Not\Exist";

            Assert.That(_installerFiles.ValidateExtractedLocation(), Is.False);
        }

        [Test]
        public void ValidateExtractedLocation_LocationMissingSetupExe_ReturnsFalse()
        {
            _fileSystem.AddDirectory(@"C:\Does\Exist");
            _fileSystem.AddFile(@"C:\Does\Exist\Setup.bat", _emptyFileData);
            _fileSystem.AddFile(@"C:\Does\Exist\NotSetup.exe", _emptyFileData);
            _fileSystem.AddDirectory(@"C:\Does\Exist\Bin64");
            _fileSystem.AddFile(@"C:\Does\Exist\Bin64\AMDCleanupUtility.exe", _emptyFileData);
            _fileSystem.AddDirectory(@"C:\Does\Exist\Config");
            _installerFiles.ExtractedInstallerDirectory = @"C:\Does\Exist";

            Assert.That(_installerFiles.ValidateExtractedLocation(), Is.False);
        }

        [Test]
        public void ValidateExtractedLocation_LocationMissingCleanupUtility_ReturnsFalse()
        {
            _fileSystem.AddDirectory(@"C:\Does\Exist");
            _fileSystem.AddFile(@"C:\Does\Exist\Setup.exe", _emptyFileData);
            _fileSystem.AddDirectory(@"C:\Does\Exist\Bin64");
            _fileSystem.AddFile(@"C:\Does\Exist\Bin64\AMDCleanupUtility.bat", _emptyFileData);
            _fileSystem.AddFile(@"C:\Does\Exist\Bin64\NotAMDCleanupUtility.exe", _emptyFileData);
            _fileSystem.AddDirectory(@"C:\Does\Exist\Config");
            _installerFiles.ExtractedInstallerDirectory = @"C:\Does\Exist";

            Assert.That(_installerFiles.ValidateExtractedLocation(), Is.False);
        }

        [Test]
        public void ValidateExtractedLocation_LocationMissingBin64Folder_ReturnsFalse()
        {
            _fileSystem.AddDirectory(@"C:\Does\Exist");
            _fileSystem.AddFile(@"C:\Does\Exist\Setup.exe", _emptyFileData);
            _fileSystem.AddDirectory(@"C:\Does\Exist\Bin");
            _fileSystem.AddDirectory(@"C:\Does\Exist\Bin32");
            _fileSystem.AddDirectory(@"C:\Does\Exist\Config");
            _installerFiles.ExtractedInstallerDirectory = @"C:\Does\Exist";

            Assert.That(_installerFiles.ValidateExtractedLocation(), Is.False);
        }

        [Test]
        public void ValidateExtractedLocation_LocationConfigFolder_ReturnsFalse()
        {
            _fileSystem.AddDirectory(@"C:\Does\Exist");
            _fileSystem.AddFile(@"C:\Does\Exist\Setup.exe", _emptyFileData);
            _fileSystem.AddDirectory(@"C:\Does\Exist\Bin64");
            _fileSystem.AddFile(@"C:\Does\Exist\Bin64\AMDCleanupUtility.exe", _emptyFileData);
            _fileSystem.AddDirectory(@"C:\Does\Exist\Cfg");
            _fileSystem.AddDirectory(@"C:\Does\Exist\Configuration");
            _installerFiles.ExtractedInstallerDirectory = @"C:\Does\Exist";

            Assert.That(_installerFiles.ValidateExtractedLocation(), Is.False);
        }

        [Test]
        public void ValidateExtractedLocation_LocationIsValid_ReturnsTrue()
        {
            _fileSystem.AddDirectory(@"C:\Does\Exist");
            _fileSystem.AddFile(@"C:\Does\Exist\Setup.bat", _emptyFileData);
            _fileSystem.AddFile(@"C:\Does\Exist\Setup.exe", _emptyFileData);
            _fileSystem.AddFile(@"C:\Does\Exist\NotSetup.exe", _emptyFileData);
            _fileSystem.AddDirectory(@"C:\Does\Exist\Bin");
            _fileSystem.AddDirectory(@"C:\Does\Exist\Bin32");
            _fileSystem.AddDirectory(@"C:\Does\Exist\Bin64");
            _fileSystem.AddFile(@"C:\Does\Exist\Bin64\AMDCleanupUtility.exe", _emptyFileData);
            _fileSystem.AddDirectory(@"C:\Does\Exist\Cfg");
            _fileSystem.AddDirectory(@"C:\Does\Exist\Config");
            _fileSystem.AddDirectory(@"C:\Does\Exist\Configuration");
            _installerFiles.ExtractedInstallerDirectory = @"C:\Does\Exist";

            Assert.That(_installerFiles.ValidateExtractedLocation(), Is.True);
        }
    }
}
