using System;
using System.IO.Abstractions.TestingHelpers;
using NUnit.Framework;
using RadeonSoftwareSlimmer.Core.PreInstall;
using RadeonSoftwareSlimmer.Core.Test.TestDoubles;

namespace RadeonSoftwareSlimmer.Core.Test.Models.PreInstall
{
    public class InstallerFilesModelTest
    {
        private MockFileSystem _fileSystem;
        private FakeAppLogger _logger;
        private FakeProcessRunner _processRunner;
        private InstallerFilesModel _installerFiles;
        //Contents of the files doesn't matter for any of these tests
        private readonly MockFileData _emptyFileData = new MockFileData(Array.Empty<byte>());


        [SetUp]
        public void Setup()
        {
            _fileSystem = new MockFileSystem();
            _logger = new FakeAppLogger();
            _processRunner = new FakeProcessRunner();
            _installerFiles = new InstallerFilesModel(_fileSystem, _processRunner, _logger);
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
            _fileSystem.AddFile(TestPath.Rooted(@"File\Does\Exist.exe"), _emptyFileData);
            _installerFiles.InstallerFile = TestPath.Rooted(@"File\Does\NotExist.exe");

            Assert.That(_installerFiles.ValidateInstallerFile(), Is.False);
        }

        [Test]
        public void ValidateInstallerFile_FileDoesExist_ReturnsTrue()
        {
            _fileSystem.AddFile(TestPath.Rooted(@"File\Does\Exist.exe"), _emptyFileData);
            _installerFiles.InstallerFile = TestPath.Rooted(@"File\Does\Exist.exe");

            Assert.That(_installerFiles.ValidateInstallerFile(), Is.True);
        }

        [Platform(Include = "Win")]
        [TestCase(@"C:\Path\ValidPath\Invalid;name.exe")]
        [TestCase(@"C:\Path\Invalid|Name\ValidName.exe")]
        public void ValidateInstallerFile_PathContainsInvalidCharacters_ReturnsFalse(string installerFile)
        {
            _installerFiles.InstallerFile = installerFile;

            Assert.That(_installerFiles.ValidateInstallerFile(), Is.False);
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
            _fileSystem.AddDirectory(TestPath.Rooted(@"Does\Exist"));
            _installerFiles.ExtractedInstallerDirectory = TestPath.Rooted(@"Does\Not\Exist");

            Assert.That(_installerFiles.ValidatePreExtractLocation(), Is.True);
        }

        [Test]
        public void ValidatePreExtractLocation_LocationHasFiles_ReturnsFalse()
        {
            _fileSystem.AddFile(TestPath.Rooted(@"Does\Exist\file.something"), _emptyFileData);
            _installerFiles.ExtractedInstallerDirectory = TestPath.Rooted(@"Does\Exist");

            Assert.That(_installerFiles.ValidatePreExtractLocation(), Is.False);
        }

        [Test]
        public void ValidatePreExtractLocation_LocationHasFolders_ReturnsFalse()
        {
            _fileSystem.AddDirectory(TestPath.Rooted(@"Does\Exist"));
            _fileSystem.AddDirectory(TestPath.Rooted(@"Does\Exist\ChildFolder"));
            _installerFiles.ExtractedInstallerDirectory = TestPath.Rooted(@"Does\Exist");

            Assert.That(_installerFiles.ValidatePreExtractLocation(), Is.False);
        }

        [Test]
        public void ValidatePreExtractLocation_LocationIsEmpty_ReturnsTrue()
        {
            _fileSystem.AddDirectory(TestPath.Rooted(@"Does\Exist"));
            _installerFiles.ExtractedInstallerDirectory = TestPath.Rooted(@"Does\Exist");

            Assert.That(_installerFiles.ValidatePreExtractLocation(), Is.True);
        }

        [Platform(Include = "Win")]
        [TestCase(@"C:\Path\In>InvalidName\")]
        [TestCase(@"C:\Path\Invalid|Name\")]
        [TestCase("C:\\Path\\In\"valid|Name")]
        public void ValidatePreExtractLocation_PathContainsInvalidCharacters_ReturnsFalse(string directory)
        {
            _installerFiles.ExtractedInstallerDirectory = directory;

            Assert.That(_installerFiles.ValidatePreExtractLocation(), Is.False);
        }


        [Test]
        public void ValidateExtractedLocation_LocationIsNullOrEmpty_ReturnsFalse()
        {
            _installerFiles.ExtractedInstallerDirectory = string.Empty;

            Assert.That(_installerFiles.ValidateExtractedLocation(), Is.False);
        }

        [Test]
        public void ValidateExtractedLocation_LocationDoesNotExist_ReturnsFalse()
        {
            _fileSystem.AddDirectory(TestPath.Rooted(@"Does\Exist"));
            _installerFiles.ExtractedInstallerDirectory = TestPath.Rooted(@"Does\Not\Exist");

            Assert.That(_installerFiles.ValidateExtractedLocation(), Is.False);
        }

        [Test]
        public void ValidateExtractedLocation_LocationMissingSetupExe_ReturnsFalse()
        {
            _fileSystem.AddDirectory(TestPath.Rooted(@"Does\Exist"));
            _fileSystem.AddFile(TestPath.Rooted(@"Does\Exist\Setup.bat"), _emptyFileData);
            _fileSystem.AddFile(TestPath.Rooted(@"Does\Exist\NotSetup.exe"), _emptyFileData);
            _fileSystem.AddDirectory(TestPath.Rooted(@"Does\Exist\Bin64"));
            _fileSystem.AddFile(TestPath.Rooted(@"Does\Exist\Bin64\AMDCleanupUtility.exe"), _emptyFileData);
            _fileSystem.AddDirectory(TestPath.Rooted(@"Does\Exist\Config"));
            _installerFiles.ExtractedInstallerDirectory = TestPath.Rooted(@"Does\Exist");

            Assert.That(_installerFiles.ValidateExtractedLocation(), Is.False);
        }

        [Test]
        public void ValidateExtractedLocation_LocationMissingCleanupUtility_ReturnsFalse()
        {
            _fileSystem.AddDirectory(TestPath.Rooted(@"Does\Exist"));
            _fileSystem.AddFile(TestPath.Rooted(@"Does\Exist\Setup.exe"), _emptyFileData);
            _fileSystem.AddDirectory(TestPath.Rooted(@"Does\Exist\Bin64"));
            _fileSystem.AddFile(TestPath.Rooted(@"Does\Exist\Bin64\AMDCleanupUtility.bat"), _emptyFileData);
            _fileSystem.AddFile(TestPath.Rooted(@"Does\Exist\Bin64\NotAMDCleanupUtility.exe"), _emptyFileData);
            _fileSystem.AddDirectory(TestPath.Rooted(@"Does\Exist\Config"));
            _installerFiles.ExtractedInstallerDirectory = TestPath.Rooted(@"Does\Exist");

            Assert.That(_installerFiles.ValidateExtractedLocation(), Is.False);
        }

        [Test]
        public void ValidateExtractedLocation_LocationMissingBin64Folder_ReturnsFalse()
        {
            _fileSystem.AddDirectory(TestPath.Rooted(@"Does\Exist"));
            _fileSystem.AddFile(TestPath.Rooted(@"Does\Exist\Setup.exe"), _emptyFileData);
            _fileSystem.AddDirectory(TestPath.Rooted(@"Does\Exist\Bin"));
            _fileSystem.AddDirectory(TestPath.Rooted(@"Does\Exist\Bin32"));
            _fileSystem.AddDirectory(TestPath.Rooted(@"Does\Exist\Config"));
            _installerFiles.ExtractedInstallerDirectory = TestPath.Rooted(@"Does\Exist");

            Assert.That(_installerFiles.ValidateExtractedLocation(), Is.False);
        }

        [Test]
        public void ValidateExtractedLocation_LocationConfigFolder_ReturnsFalse()
        {
            _fileSystem.AddDirectory(TestPath.Rooted(@"Does\Exist"));
            _fileSystem.AddFile(TestPath.Rooted(@"Does\Exist\Setup.exe"), _emptyFileData);
            _fileSystem.AddDirectory(TestPath.Rooted(@"Does\Exist\Bin64"));
            _fileSystem.AddFile(TestPath.Rooted(@"Does\Exist\Bin64\AMDCleanupUtility.exe"), _emptyFileData);
            _fileSystem.AddDirectory(TestPath.Rooted(@"Does\Exist\Cfg"));
            _fileSystem.AddDirectory(TestPath.Rooted(@"Does\Exist\Configuration"));
            _installerFiles.ExtractedInstallerDirectory = TestPath.Rooted(@"Does\Exist");

            Assert.That(_installerFiles.ValidateExtractedLocation(), Is.False);
        }

        [Test]
        public void ValidateExtractedLocation_LocationIsValid_ReturnsTrue()
        {
            _fileSystem.AddDirectory(TestPath.Rooted(@"Does\Exist"));
            _fileSystem.AddFile(TestPath.Rooted(@"Does\Exist\Setup.bat"), _emptyFileData);
            _fileSystem.AddFile(TestPath.Rooted(@"Does\Exist\Setup.exe"), _emptyFileData);
            _fileSystem.AddFile(TestPath.Rooted(@"Does\Exist\NotSetup.exe"), _emptyFileData);
            _fileSystem.AddDirectory(TestPath.Rooted(@"Does\Exist\Bin"));
            _fileSystem.AddDirectory(TestPath.Rooted(@"Does\Exist\Bin32"));
            _fileSystem.AddDirectory(TestPath.Rooted(@"Does\Exist\Bin64"));
            _fileSystem.AddFile(TestPath.Rooted(@"Does\Exist\Bin64\AMDCleanupUtility.exe"), _emptyFileData);
            _fileSystem.AddDirectory(TestPath.Rooted(@"Does\Exist\Cfg"));
            _fileSystem.AddDirectory(TestPath.Rooted(@"Does\Exist\Config"));
            _fileSystem.AddDirectory(TestPath.Rooted(@"Does\Exist\Configuration"));
            _installerFiles.ExtractedInstallerDirectory = TestPath.Rooted(@"Does\Exist");

            Assert.That(_installerFiles.ValidateExtractedLocation(), Is.True);
        }

        [Platform(Include = "Win")]
        [TestCase(@"C:\Path\In>InvalidName\")]
        [TestCase(@"C:\Path\Invalid|Name\")]
        [TestCase("C:\\Path\\In\"valid|Name")]
        public void ValidateExtractedLocation_PathContainsInvalidCharacters_ReturnsFalse(string directory)
        {
            _installerFiles.ExtractedInstallerDirectory = directory;

            Assert.That(_installerFiles.ValidateExtractedLocation(), Is.False);
        }


        [Test]
        public void ExtractInstallerFiles_InvokesSevenZipWithInstallerAndOutputPaths()
        {
            _installerFiles.InstallerFile = TestPath.Rooted(@"Some\Installer.exe");
            _installerFiles.ExtractedInstallerDirectory = TestPath.Rooted(@"Some\Extracted");

            _installerFiles.ExtractInstallerFiles();

            Assert.Multiple((System.Action)(() =>
            {
                Assert.That(_processRunner.LastFileName, Does.EndWith("7z.exe"));
                Assert.That(_processRunner.LastArguments, Does.StartWith("x "));
                Assert.That(_processRunner.LastArguments, Does.Contain($"\"{_installerFiles.InstallerFile}\""));
                Assert.That(_processRunner.LastArguments, Does.Contain($"-o\"{_installerFiles.ExtractedInstallerDirectory}\""));
            }));
        }

        [Test]
        public void ExtractInstallerFiles_SevenZipReturnsFatalError_ThrowsIOException()
        {
            _installerFiles.InstallerFile = TestPath.Rooted(@"Some\Installer.exe");
            _installerFiles.ExtractedInstallerDirectory = TestPath.Rooted(@"Some\Extracted");
            _processRunner.ExitCode = 7; // 7-Zip: fatal error

            Assert.That((System.Action)(() => _installerFiles.ExtractInstallerFiles()), Throws.TypeOf<System.IO.IOException>());
        }

        [Test]
        public void ExtractInstallerFiles_SevenZipReturnsZero_DoesNotThrow()
        {
            _installerFiles.InstallerFile = TestPath.Rooted(@"Some\Installer.exe");
            _installerFiles.ExtractedInstallerDirectory = TestPath.Rooted(@"Some\Extracted");
            _processRunner.ExitCode = 0;

            Assert.That((System.Action)(() => _installerFiles.ExtractInstallerFiles()), Throws.Nothing);
        }
    }
}
