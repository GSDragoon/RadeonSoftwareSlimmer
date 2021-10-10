using System;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using NUnit.Framework;
using RadeonSoftwareSlimmer.Models.PostInstall;

namespace RadeonSoftwareSlimmer.Test.Models.PostInstall
{
    public class TempFileListModelTest
    {
        private MockFileSystem _fileSystem;
        private TempFileListModel _tempFileListModel;

        [SetUp]
        public void Setup()
        {
            _fileSystem = new MockFileSystem();
            _tempFileListModel = new TempFileListModel(_fileSystem);
        }


        [Test]
        public void LoadOrRefresh_NoFoldersExist_EmptyList()
        {
            _fileSystem.AddDirectory(@"C:\This\is\Not\The\Directory\You\Are\Looking\For");
            _fileSystem.AddDirectory(@"C:\All\Your\Base\Are\Belong\To Us");
            _fileSystem.AddDirectory(@"D:\AMD");
            _fileSystem.AddDirectory(@"c:\RadeonInstaller\cache");

            _tempFileListModel.LoadOrRefresh();

            Assert.That(_tempFileListModel.TempFiles.Count(), Is.Zero);
        }

        [Test]
        public void LoadOrRefresh_SomeFoldersExist_ListOfMatchedFolders()
        {
            _fileSystem.AddDirectory($@"{Environment.GetEnvironmentVariable("SystemDrive", EnvironmentVariableTarget.Process)}\AMD");
            _fileSystem.AddDirectory($@"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles, Environment.SpecialFolderOption.DoNotVerify)}\AMD\CIM\Log");
            _fileSystem.AddDirectory($@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify)}\AMD_Common");
            _fileSystem.AddDirectory(@"D:\AMD");
            _fileSystem.AddDirectory($@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify)}\AMD\DxCache");
            _fileSystem.AddDirectory($@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify)}\AMD\Radeonsoftware\cache");
            _fileSystem.AddDirectory(@"c:\This\is\Not\The\Directory\You\Are\Looking\For");
            _fileSystem.AddDirectory($@"{Environment.GetFolderPath(Environment.SpecialFolder.System, Environment.SpecialFolderOption.DoNotVerify)}\AMD");

            _tempFileListModel.LoadOrRefresh();

            Assert.That(_tempFileListModel.TempFiles.Count(), Is.EqualTo(6));
            Assert.That(_tempFileListModel.TempFiles.Where(t => t.Folder == @"D:\AMD"), Is.Empty);
            Assert.That(_tempFileListModel.TempFiles.Where(t => t.Folder == @"c:\This\is\Not\The\Directory\You\Are\Looking\For"), Is.Empty);
        }


        [Test]
        public void ApplyChanges_ClearIsFalse_DoesNotClearsFolders()
        {
            string[] tempFolders =
            {
                $@"{Environment.GetEnvironmentVariable("SystemDrive", EnvironmentVariableTarget.Process)}\AMD",
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles, Environment.SpecialFolderOption.DoNotVerify)}\AMD\CIM\Reports",
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify)}\AMD_Common",
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify)}\AMD\DxCache",
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify)}\AMD\Radeonsoftware\cache",
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.System, Environment.SpecialFolderOption.DoNotVerify)}\AMD",
                @"D:\AMD",
                @"c:\This\is\Not\The\Directory\You\Are\Looking\For"
            };

            MockFileSystem expectedFileSystem = new MockFileSystem();

            foreach (string tempFolder in tempFolders)
            {
                _fileSystem.AddDirectory(tempFolder);
                expectedFileSystem.AddDirectory(tempFolder);
            }

            _fileSystem.AddFile($@"{Environment.GetEnvironmentVariable("SystemDrive", EnvironmentVariableTarget.Process)}\AMD\file.something", new MockFileData(string.Empty));
            _fileSystem.AddFile($@"{Environment.GetEnvironmentVariable("SystemDrive", EnvironmentVariableTarget.Process)}\AMD\Path\file2.something", new MockFileData(string.Empty));
            expectedFileSystem.AddFile($@"{Environment.GetEnvironmentVariable("SystemDrive", EnvironmentVariableTarget.Process)}\AMD\file.something", new MockFileData(string.Empty));
            expectedFileSystem.AddFile($@"{Environment.GetEnvironmentVariable("SystemDrive", EnvironmentVariableTarget.Process)}\AMD\Path\file2.something", new MockFileData(string.Empty));

            _tempFileListModel.LoadOrRefresh();

            foreach (TempFileModel tempFileModel in _tempFileListModel.TempFiles)
            {
                tempFileModel.Clear = false;
            }


            _tempFileListModel.ApplyChanges();


            foreach(string folder in tempFolders)
            {
                Assert.That(_fileSystem.Directory.Exists(folder), Is.True);
            }
            foreach (string folder in expectedFileSystem.AllDirectories)
            {
                Assert.That(_fileSystem.Directory.Exists(folder), Is.True);
            }
            foreach (string file in expectedFileSystem.AllFiles)
            {
                Assert.That(_fileSystem.File.Exists(file), Is.True);
            }
            foreach (TempFileModel tempFileModel in _tempFileListModel.TempFiles)
            {
                Assert.That(tempFileModel.Clear, Is.False);
            }
        }

        [Test]
        public void ApplyChanges_ClearIsTrue_DoesClearsFolders()
        {
            string[] tempFoldersToClear =
            {
                $@"{Environment.GetEnvironmentVariable("SystemDrive", EnvironmentVariableTarget.Process)}\AMD",
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles, Environment.SpecialFolderOption.DoNotVerify)}\AMD\CIM\Reports",
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify)}\AMD_Common",
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify)}\AMD\DxCache",
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify)}\AMD\Radeonsoftware\cache",
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.System, Environment.SpecialFolderOption.DoNotVerify)}\AMD",
            };
            string[] tempFoldersToNotClear =
            {
                @"D:\AMD",
                @"c:\This\is\Not\The\Directory\You\Are\Looking\For"
            };

            MockFileSystem expectedFileSystem = new MockFileSystem();

            foreach (string tempFolder in tempFoldersToClear)
            {
                _fileSystem.AddDirectory(tempFolder);
            }
            foreach (string tempFolder in tempFoldersToNotClear)
            {
                _fileSystem.AddDirectory(tempFolder);
                expectedFileSystem.AddDirectory(tempFolder);
            }

            _fileSystem.AddFile($@"{Environment.GetEnvironmentVariable("SystemDrive", EnvironmentVariableTarget.Process)}\AMD\file.something", new MockFileData(string.Empty));
            _fileSystem.AddFile($@"{Environment.GetEnvironmentVariable("SystemDrive", EnvironmentVariableTarget.Process)}\AMD\Path\file2.something", new MockFileData(string.Empty));
            _fileSystem.AddFile(@"D:\AMD\file.something", new MockFileData(string.Empty));
            _fileSystem.AddFile(@"D:\AMD\file.something\Path\file2.something", new MockFileData(string.Empty));
            expectedFileSystem.AddFile(@"D:\AMD\file.something", new MockFileData(string.Empty));
            expectedFileSystem.AddFile(@"D:\AMD\file.something\Path\file2.something", new MockFileData(string.Empty));

            _tempFileListModel.LoadOrRefresh();

            foreach (TempFileModel tempFileModel in _tempFileListModel.TempFiles)
            {
                tempFileModel.Clear = true;
            }


            _tempFileListModel.ApplyChanges();


            foreach (string folder in tempFoldersToClear)
            {
                Assert.That(_fileSystem.Directory.Exists(folder), Is.True);
                Assert.That(_fileSystem.Directory.GetDirectories(folder, "*").Length, Is.Zero);
                Assert.That(_fileSystem.Directory.GetFiles(folder, "*").Length, Is.Zero);
            }
            foreach (string folder in expectedFileSystem.AllDirectories)
            {
                Assert.That(_fileSystem.Directory.Exists(folder), Is.True);
            }
            foreach (string file in expectedFileSystem.AllFiles)
            {
                Assert.That(_fileSystem.File.Exists(file), Is.True);
            }
            foreach (TempFileModel tempFileModel in _tempFileListModel.TempFiles)
            {
                Assert.That(tempFileModel.Clear, Is.True);
            }
        }
    }
}
