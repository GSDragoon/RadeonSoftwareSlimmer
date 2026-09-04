using System;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using NUnit.Framework;
using RadeonSoftwareSlimmer.Core.PostInstall;
using RadeonSoftwareSlimmer.Core.Test.TestDoubles;

namespace RadeonSoftwareSlimmer.Core.Test.Models.PostInstall
{
    public class TempFileListModelTest
    {
        // Same non-portable substitutions the model performs, so test and model agree on any OS.
        private static readonly string SystemDrive = Environment.GetEnvironmentVariable("SystemDrive", EnvironmentVariableTarget.Process) ?? string.Empty;
        private static readonly string ProgramFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles, Environment.SpecialFolderOption.DoNotVerify);
        private static readonly string LocalAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify);
        private static readonly string System = Environment.GetFolderPath(Environment.SpecialFolder.System, Environment.SpecialFolderOption.DoNotVerify);

        // Rooted paths outside of any AMD folder, used to prove the model rejects unrelated directories.
        private static readonly string NonAmdRoot1 = TestPath.Rooted(@"OtherDrive\AMD");
        private static readonly string NonAmdRoot2 = TestPath.Rooted(@"This\is\Not\The\Directory\You\Are\Looking\For");

        private MockFileSystem _fileSystem;
        private FakeAppLogger _logger;
        private TempFileListModel _tempFileListModel;

        [SetUp]
        public void Setup()
        {
            _fileSystem = new MockFileSystem();
            _logger = new FakeAppLogger();
            _tempFileListModel = new TempFileListModel(_fileSystem, _logger);
        }


        [Test]
        public void LoadOrRefresh_NoFoldersExist_EmptyList()
        {
            _fileSystem.AddDirectory(NonAmdRoot2);
            _fileSystem.AddDirectory(TestPath.Rooted(@"All\Your\Base\Are\Belong\To Us"));
            _fileSystem.AddDirectory(NonAmdRoot1);
            _fileSystem.AddDirectory(TestPath.Rooted(@"RadeonInstaller\cache"));

            _tempFileListModel.LoadOrRefresh();

            Assert.That(_tempFileListModel.TempFiles.Count(), Is.Zero);
        }

        [Test]
        [Platform(Include = "Win", Reason = "Relies on distinct values for SystemDrive vs SpecialFolder.System; both collapse to empty on non-Windows.")]
        public void LoadOrRefresh_SomeFoldersExist_ListOfMatchedFolders()
        {
            _fileSystem.AddDirectory($@"{SystemDrive}\AMD");
            _fileSystem.AddDirectory($@"{ProgramFiles}\AMD\CIM\Log");
            _fileSystem.AddDirectory($@"{LocalAppData}\AMD_Common");
            _fileSystem.AddDirectory(NonAmdRoot1);
            _fileSystem.AddDirectory($@"{LocalAppData}\AMD\DxCache");
            _fileSystem.AddDirectory($@"{LocalAppData}\AMD\Radeonsoftware\cache");
            _fileSystem.AddDirectory(NonAmdRoot2);
            _fileSystem.AddDirectory($@"{System}\AMD");
            _fileSystem.AddDirectory($@"{System}\AMD\EeuDumps");

            _tempFileListModel.LoadOrRefresh();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_tempFileListModel.TempFiles.Count(), Is.EqualTo(6));
                Assert.That(_tempFileListModel.TempFiles.Where(t => t.Folder == NonAmdRoot1), Is.Empty);
                Assert.That(_tempFileListModel.TempFiles.Where(t => t.Folder == $@"{System}\AMD"), Is.Empty);
                Assert.That(_tempFileListModel.TempFiles.Where(t => t.Folder == NonAmdRoot2), Is.Empty);
            }
        }


        [Test]
        public void ApplyChanges_ClearIsFalse_DoesNotClearsFolders()
        {
            string[] tempFolders =
            {
                $@"{SystemDrive}\AMD",
                $@"{ProgramFiles}\AMD\CIM\Reports",
                $@"{LocalAppData}\AMD_Common",
                $@"{LocalAppData}\AMD\DxCache",
                $@"{LocalAppData}\AMD\Radeonsoftware\cache",
                $@"{System}\AMD",
                NonAmdRoot1,
                NonAmdRoot2,
            };

            MockFileSystem expectedFileSystem = new MockFileSystem();

            foreach (string tempFolder in tempFolders)
            {
                _fileSystem.AddDirectory(tempFolder);
                expectedFileSystem.AddDirectory(tempFolder);
            }

            _fileSystem.AddFile($@"{SystemDrive}\AMD\file.something", new MockFileData(string.Empty));
            _fileSystem.AddFile($@"{SystemDrive}\AMD\Path\file2.something", new MockFileData(string.Empty));
            expectedFileSystem.AddFile($@"{SystemDrive}\AMD\file.something", new MockFileData(string.Empty));
            expectedFileSystem.AddFile($@"{SystemDrive}\AMD\Path\file2.something", new MockFileData(string.Empty));

            _tempFileListModel.LoadOrRefresh();

            foreach (TempFileModel tempFileModel in _tempFileListModel.TempFiles)
            {
                tempFileModel.Clear = false;
            }


            _tempFileListModel.ApplyChanges();


            foreach (string folder in tempFolders)
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
                $@"{SystemDrive}\AMD",
                $@"{ProgramFiles}\AMD\CIM\Reports",
                $@"{LocalAppData}\AMD_Common",
                $@"{LocalAppData}\AMD\DxCache",
                $@"{LocalAppData}\AMD\Radeonsoftware\cache",
                $@"{System}\AMD",
            };
            string[] tempFoldersToNotClear =
            {
                NonAmdRoot1,
                NonAmdRoot2,
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

            _fileSystem.AddFile($@"{SystemDrive}\AMD\file.something", new MockFileData(string.Empty));
            _fileSystem.AddFile($@"{SystemDrive}\AMD\Path\file2.something", new MockFileData(string.Empty));
            _fileSystem.AddFile(_fileSystem.Path.Combine(NonAmdRoot1, "file.something"), new MockFileData(string.Empty));
            _fileSystem.AddFile(_fileSystem.Path.Combine(NonAmdRoot1, "file.something", "Path", "file2.something"), new MockFileData(string.Empty));
            expectedFileSystem.AddFile(_fileSystem.Path.Combine(NonAmdRoot1, "file.something"), new MockFileData(string.Empty));
            expectedFileSystem.AddFile(_fileSystem.Path.Combine(NonAmdRoot1, "file.something", "Path", "file2.something"), new MockFileData(string.Empty));

            _tempFileListModel.LoadOrRefresh();

            foreach (TempFileModel tempFileModel in _tempFileListModel.TempFiles)
            {
                tempFileModel.Clear = true;
            }


            _tempFileListModel.ApplyChanges();

            using (Assert.EnterMultipleScope())
            {

                foreach (string folder in tempFoldersToClear)
                {
                    Assert.That(_fileSystem.Directory.Exists(folder), Is.True);
                    Assert.That(_fileSystem.Directory.GetDirectories(folder, "*"), Has.Length.EqualTo(0));
                    Assert.That(_fileSystem.Directory.GetFiles(folder, "*"), Has.Length.EqualTo(0));
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
}
