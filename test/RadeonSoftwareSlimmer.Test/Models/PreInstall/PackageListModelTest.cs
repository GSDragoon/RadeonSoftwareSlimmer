using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using RadeonSoftwareSlimmer.Models.PreInstall;

namespace RadeonSoftwareSlimmer.Test.Models.PreInstall
{
    [SuppressMessage("System.IO.Abstractions", "IO0002:Replace File class with IFileSystem.File for improved testability", Justification = "Reading from test data file")]
    [SuppressMessage("Assertion", "NUnit2010:Use EqualConstraint for better assertion messages in case of failure", Justification = "Does not work, does not call Equals method")]
    public class PackageListModelTest
    {
        private MockFileSystem _fileSystem;
        private string _currentDirectory;

        [SetUp]
        [SuppressMessage("System.IO.Abstractions", "IO0006:Replace Path class with IFileSystem.Path for improved testability", Justification = "Used to set path to load files from VS and command line")]
        public void Setup()
        {
            _fileSystem = new MockFileSystem();
            _currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }


        [Test]
        public void LoadOrRefresh_LoadsManifest()
        {
            PackageListModel packageList = new PackageListModel(_fileSystem);
            string installRoot = @"C:\Parent\Child\InstallerFolder";
            _fileSystem.AddFile(installRoot + @"\Bin64\cccmanifest_64.json", new MockFileData(File.ReadAllText(_currentDirectory + @"\TestData\PackageModel_cccmanifest.json")));
            _fileSystem.AddFile(installRoot + @"\Config\InstallManifest.json", new MockFileData(File.ReadAllText(_currentDirectory + @"\TestData\PackageModel_installmanifest.json")));
            IDirectoryInfo installerDir = _fileSystem.DirectoryInfo.New(installRoot);

            packageList.LoadOrRefresh(installerDir);

            Assert.That(packageList.InstallerPackages, Is.Not.Null);
            List<PackageModel> actualPackages = packageList.InstallerPackages.ToList();
            IList<PackageModel> exectedPackages = ExpectedLoadedPackageModel(installRoot);
            Assert.Multiple(() =>
            {
                Assert.That(actualPackages, Is.Not.Null);
                Assert.That(actualPackages, Has.Count.EqualTo(4));
                Assert.That(actualPackages[0].Equals(exectedPackages[0]), Is.True);
                Assert.That(actualPackages[1].Equals(exectedPackages[1]), Is.True);
                Assert.That(actualPackages[2].Equals(exectedPackages[2]), Is.True);
                Assert.That(actualPackages[3].Equals(exectedPackages[3]), Is.True);
            });
        }

        [Test]
        public void LoadOrRefresh_CreatesBackupFiles()
        {
            PackageListModel packageList = new PackageListModel(_fileSystem);
            string installRoot = @"C:\Parent\Child\InstallerFolder";
            MockFileData ccmanifesData = new MockFileData(File.ReadAllText(_currentDirectory + @"\TestData\PackageModel_cccmanifest.json"));
            MockFileData installmanifestData = new MockFileData(File.ReadAllText(_currentDirectory + @"\TestData\PackageModel_installmanifest.json"));
            _fileSystem.AddFile(installRoot + @"\Bin64\cccmanifest_64.json", ccmanifesData);
            _fileSystem.AddFile(installRoot + @"\Config\InstallManifest.json", installmanifestData);
            IDirectoryInfo installerDir = _fileSystem.DirectoryInfo.New(installRoot);

            packageList.LoadOrRefresh(installerDir);

            MockFileData ccmanifestBak = _fileSystem.GetFile(installRoot + @"\RSS_Backup\Packages\Bin64\cccmanifest_64.json");
            MockFileData installmanifestBak = _fileSystem.GetFile(installRoot + @"\RSS_Backup\Packages\Config\InstallManifest.json");
            Assert.Multiple(() =>
            {
                Assert.That(ccmanifestBak, Is.Not.Null);
                Assert.That(installmanifestBak, Is.Not.Null);
                Assert.That(ccmanifestBak.TextContents.Equals(ccmanifesData.TextContents));
                Assert.That(installmanifestBak.TextContents.Equals(installmanifestData.TextContents));
            });
        }

        [Test]
        public void LoadOrRefresh_DoesNotReplaceExistingBackupFiles()
        {
            PackageListModel packageList = new PackageListModel(_fileSystem);
            string installRoot = @"C:\Parent\Child\InstallerFolder";
            _fileSystem.AddFile(installRoot + @"\Bin64\cccmanifest_64.json", new MockFileData(File.ReadAllText(_currentDirectory + @"\TestData\PackageModel_cccmanifest.json")));
            _fileSystem.AddFile(installRoot + @"\Config\InstallManifest.json", new MockFileData(File.ReadAllText(_currentDirectory + @"\TestData\PackageModel_installmanifest.json")));
            _fileSystem.AddFile(installRoot + @"\RSS_Backup\Packages\Bin64\cccmanifest_64.json", new MockFileData(File.ReadAllText(_currentDirectory + @"\TestData\PackageModel_cccmanifest.json")));
            _fileSystem.AddFile(installRoot + @"\RSS_Backup\Packages\Config\InstallManifest.json", new MockFileData(File.ReadAllText(_currentDirectory + @"\TestData\PackageModel_installmanifest.json")));
            IDirectoryInfo installerDir = _fileSystem.DirectoryInfo.New(installRoot);
            MockFileData ccmanifestOrig = _fileSystem.GetFile(installRoot + @"\RSS_Backup\Packages\Bin64\cccmanifest_64.json");
            MockFileData installmanifestOrig = _fileSystem.GetFile(installRoot + @"\RSS_Backup\Packages\Config\InstallManifest.json");

            Thread.Sleep(200); // Just to make sure the timestamps don't match (if it fails and replaces the existing backup files)
            packageList.LoadOrRefresh(installerDir);

            MockFileData ccmanifestBak = _fileSystem.GetFile(installRoot + @"\RSS_Backup\Packages\Bin64\cccmanifest_64.json");
            MockFileData installmanifestBak = _fileSystem.GetFile(installRoot + @"\RSS_Backup\Packages\Config\InstallManifest.json");
            Assert.Multiple(() =>
            {
                Assert.That(ccmanifestBak, Is.Not.Null);
                Assert.That(installmanifestBak, Is.Not.Null);
                Assert.That(MockFileDataIsEqual(ccmanifestBak, ccmanifestOrig));
                Assert.That(MockFileDataIsEqual(installmanifestBak, installmanifestOrig));
            });
        }


        [Test]
        public void RemovePackage_PackageIsNull_ThrowsArgumentNullException()
        {
            Assert.That(() => PackageListModel.RemovePackage(null), Throws.ArgumentNullException);
        }

        [Test]
        public void RemovePackage_PackageDoesNotExist_DoesNotRemove()
        {
            string installRoot = @"C:\Parent\Child\InstallerFolder";
            _fileSystem.AddFile(installRoot + @"\Bin64\cccmanifest_64.json", new MockFileData(File.ReadAllText(_currentDirectory + @"\TestData\PackageModel_cccmanifest.json")));
            _fileSystem.AddFile(installRoot + @"\Config\InstallManifest.json", new MockFileData(File.ReadAllText(_currentDirectory + @"\TestData\PackageModel_installmanifest.json")));
            IDirectoryInfo installerDir = _fileSystem.DirectoryInfo.New(installRoot);

            PackageModel packageToRemove = new PackageModel(_fileSystem.FileInfo.New(installerDir + @"\Bin64\cccmanifest_64.json"))
            {
                Description = "Test Description DOES NOT EXIST",
                ProductName = "Test productName DOES NOT EXIST",
                Url = @"\Parent\child\fileDoesNotExist.extension",
                Type = "Test ptype DOES NOT EXIST",
                Keep = true
            };

            PackageListModel.RemovePackage(packageToRemove);

            PackageListModel packageList = new PackageListModel(_fileSystem);
            packageList.LoadOrRefresh(installerDir);
            List<PackageModel> actualPackages = packageList.InstallerPackages.ToList();
            IList<PackageModel> exectedPackages = ExpectedLoadedPackageModel(installRoot);
            Assert.Multiple(() =>
            {
                Assert.That(actualPackages, Is.Not.Null);
                Assert.That(actualPackages, Has.Count.EqualTo(4));            
                Assert.That(actualPackages[0].Equals(exectedPackages[0]), Is.True);
                Assert.That(actualPackages[1].Equals(exectedPackages[1]), Is.True);
                Assert.That(actualPackages[2].Equals(exectedPackages[2]), Is.True);
                Assert.That(actualPackages[3].Equals(exectedPackages[3]), Is.True);
            });
        }

        [Test]
        public void RemovePackage_PackageExists_IsRemoved()
        {
            string installRoot = @"C:\Parent\Child\InstallerFolder";
            _fileSystem.AddFile(installRoot + @"\Bin64\cccmanifest_64.json", new MockFileData(File.ReadAllText(_currentDirectory + @"\TestData\PackageModel_cccmanifest.json")));
            _fileSystem.AddFile(installRoot + @"\Config\InstallManifest.json", new MockFileData(File.ReadAllText(_currentDirectory + @"\TestData\PackageModel_installmanifest.json")));
            IDirectoryInfo installerDir = _fileSystem.DirectoryInfo.New(installRoot);

            PackageModel packageToRemove = new PackageModel(_fileSystem.FileInfo.New(installerDir + @"\Bin64\cccmanifest_64.json"))
            {
                Description = "Test Description",
                ProductName = "Test productName",
                Url = @"\Parent\child\file.extension",
                Type = "Test ptype",
                Keep = true
            };

            PackageListModel.RemovePackage(packageToRemove);

            PackageListModel packageList = new PackageListModel(_fileSystem);
            packageList.LoadOrRefresh(installerDir);
            List<PackageModel> actualPackages = packageList.InstallerPackages.ToList();
            IList<PackageModel> exectedPackages = ExpectedRemovePackageModel(installRoot);
            Assert.Multiple(() =>
            {
                Assert.That(actualPackages, Is.Not.Null);
                Assert.That(actualPackages, Has.Count.EqualTo(3));
                Assert.That(actualPackages[0].Equals(exectedPackages[0]), Is.True);
                Assert.That(actualPackages[1].Equals(exectedPackages[1]), Is.True);
                Assert.That(actualPackages[2].Equals(exectedPackages[2]), Is.True);
            });
        }


        [Test]
        public void RestoreToDefault_RestoresPackageFilesFromBackup()
        {
            PackageListModel packageList = new PackageListModel(_fileSystem);
            string installRoot = @"C:\Parent\Child\InstallerFolder";
            _fileSystem.AddFile(installRoot + @"\Bin64\cccmanifest_64.json", new MockFileData(File.ReadAllText(_currentDirectory + @"\TestData\PackageModel_cccmanifest.json")));
            _fileSystem.AddFile(installRoot + @"\Config\InstallManifest.json", new MockFileData(File.ReadAllText(_currentDirectory + @"\TestData\PackageModel_installmanifest.json")));
            _fileSystem.AddFile(installRoot + @"\RSS_Backup\Packages\Bin64\cccmanifest_64.json", new MockFileData(File.ReadAllText(_currentDirectory + @"\TestData\PackageModel_cccmanifest.json")));
            _fileSystem.AddFile(installRoot + @"\RSS_Backup\Packages\Config\InstallManifest.json", new MockFileData(File.ReadAllText(_currentDirectory + @"\TestData\PackageModel_installmanifest.json")));
            IDirectoryInfo installerDir = _fileSystem.DirectoryInfo.New(installRoot);
            _fileSystem.GetFile(installRoot + @"\RSS_Backup\Packages\Bin64\cccmanifest_64.json").TextContents = "Different data to simulate modifying the file";
            _fileSystem.GetFile(installRoot + @"\RSS_Backup\Packages\Config\InstallManifest.json").TextContents = "Different data to simulate modifying the file";
            packageList.LoadOrRefresh(installerDir);

            packageList.RestoreToDefault();

            MockFileData ccmanifest = _fileSystem.GetFile(installRoot + @"\Bin64\cccmanifest_64.json");
            MockFileData installmanifest = _fileSystem.GetFile(installRoot + @"\Config\InstallManifest.json");
            MockFileData ccmanifestBak = _fileSystem.GetFile(installRoot + @"\RSS_Backup\Packages\Bin64\cccmanifest_64.json");
            MockFileData installmanifestBak = _fileSystem.GetFile(installRoot + @"\RSS_Backup\Packages\Config\InstallManifest.json");
            Assert.Multiple(() =>
            {
                Assert.That(ccmanifestBak, Is.Not.Null);
                Assert.That(installmanifestBak, Is.Not.Null);
                Assert.That(ccmanifest, Is.Not.Null);
                Assert.That(installmanifest, Is.Not.Null);
                Assert.That(ccmanifest.TextContents, Is.EqualTo(ccmanifestBak.TextContents));
                Assert.That(installmanifest.TextContents, Is.EqualTo(installmanifestBak.TextContents));
            });
        }


        private static bool MockFileDataIsEqual(MockFileData actual, MockFileData expected)
        {
            if (actual == null || expected == null)
                return false;

            if (!actual.CreationTime.Equals(expected.CreationTime))
                return false;

            if (!actual.LastWriteTime.Equals(expected.LastWriteTime))
                return false;

            if (!string.Equals(actual.TextContents, expected.TextContents))
                return false;

            return true;
        }

        private IList<PackageModel> ExpectedLoadedPackageModel(string installerRoot)
        {
            return new List<PackageModel>()
            {
                new PackageModel(_fileSystem.FileInfo.New(installerRoot + @"\Config\InstallManifest.json"))
                {
                Description = "Test2 Description",
                ProductName = "ATest2 productName",
                Url = @"\Parent2\child2\file2.extension",
                Type = "Test2 ptype",
                Keep = true
                },
                new PackageModel(_fileSystem.FileInfo.New(installerRoot + @"\Bin64\cccmanifest_64.json"))
                {
                Description = "Test Description",
                ProductName = "Test productName",
                Url = @"\Parent\child\file.extension",
                Type = "Test ptype",
                Keep = true
                },
                new PackageModel(_fileSystem.FileInfo.New(installerRoot + @"\Bin64\cccmanifest_64.json"))
                {
                    Description = "Test2 Description",
                    ProductName = "Test2 productName",
                    Url = @"\Parent2\child2\file2.extension",
                    Type = "Test2 ptype",
                    Keep = true
                },
                new PackageModel(_fileSystem.FileInfo.New(installerRoot + @"\Config\InstallManifest.json"))
                {
                    Description = "Test Description",
                    ProductName = "ZTest productName",
                    Url = @"\Parent\child\file.extension",
                    Type = "Test ptype",
                    Keep = true
                }
            };
        }

        private IList<PackageModel> ExpectedRemovePackageModel(string installerRoot)
        {
            return new List<PackageModel>()
            {
                new PackageModel(_fileSystem.FileInfo.New(installerRoot + @"\Config\InstallManifest.json"))
                {
                Description = "Test2 Description",
                ProductName = "ATest2 productName",
                Url = @"\Parent2\child2\file2.extension",
                Type = "Test2 ptype",
                Keep = true
                },
                new PackageModel(_fileSystem.FileInfo.New(installerRoot + @"\Bin64\cccmanifest_64.json"))
                {
                    Description = "Test2 Description",
                    ProductName = "Test2 productName",
                    Url = @"\Parent2\child2\file2.extension",
                    Type = "Test2 ptype",
                    Keep = true
                },
                new PackageModel(_fileSystem.FileInfo.New(installerRoot + @"\Config\InstallManifest.json"))
                {
                    Description = "Test Description",
                    ProductName = "ZTest productName",
                    Url = @"\Parent\child\file.extension",
                    Type = "Test ptype",
                    Keep = true
                }
            };
        }
    }
}
