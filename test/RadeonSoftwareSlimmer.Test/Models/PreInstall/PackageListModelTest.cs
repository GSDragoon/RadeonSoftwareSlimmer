using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using NUnit.Framework;
using RadeonSoftwareSlimmer.Models.PreInstall;

namespace RadeonSoftwareSlimmer.Test.Models.PreInstall
{
    [SuppressMessage("System.IO.Abstractions", "IO0002:Replace File class with IFileSystem.File for improved testability", Justification = "Reading from test data file")]
    [SuppressMessage("Assertion", "NUnit2010:Use EqualConstraint for better assertion messages in case of failure", Justification = "Does not work, does not call Equals method")]
    public class PackageListModelTest
    {
        private MockFileSystem _fileSystem;

        [SetUp]
        public void Setup()
        {
            _fileSystem = new MockFileSystem();
        }


        [Test]
        public void LoadOrRefresh_LoadsManifest()
        {
            PackageListModel packageList = new PackageListModel(_fileSystem);
            string installRoot = @"Parent\Child\InstallerFolder";
            _fileSystem.AddFile(installRoot + @"\Bin64\cccmanifest_64.json", new MockFileData(File.ReadAllText(@"TestData\PackageModel_cccmanifest.json")));
            _fileSystem.AddFile(installRoot + @"\Config\InstallManifest.json", new MockFileData(File.ReadAllText(@"TestData\PackageModel_installmanifest.json")));
            IDirectoryInfo installerDir = _fileSystem.DirectoryInfo.FromDirectoryName(installRoot);

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
        public void RemovePackage_PackageIsNull_ThrowsArgumentNullException()
        {
            Assert.That(() => PackageListModel.RemovePackage(null), Throws.ArgumentNullException);
        }

        [Test]
        public void RemovePackage_PackageDoesNotExist_DoesNotRemove()
        {
            string installRoot = @"Parent\Child\InstallerFolder";
            _fileSystem.AddFile(installRoot + @"\Bin64\cccmanifest_64.json", new MockFileData(File.ReadAllText(@"TestData\PackageModel_cccmanifest.json")));
            _fileSystem.AddFile(installRoot + @"\Config\InstallManifest.json", new MockFileData(File.ReadAllText(@"TestData\PackageModel_installmanifest.json")));
            IDirectoryInfo installerDir = _fileSystem.DirectoryInfo.FromDirectoryName(installRoot);

            PackageModel packageToRemove = new PackageModel(_fileSystem.FileInfo.FromFileName(installerDir + @"\Bin64\cccmanifest_64.json"))
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
            string installRoot = @"Parent\Child\InstallerFolder";
            _fileSystem.AddFile(installRoot + @"\Bin64\cccmanifest_64.json", new MockFileData(File.ReadAllText(@"TestData\PackageModel_cccmanifest.json")));
            _fileSystem.AddFile(installRoot + @"\Config\InstallManifest.json", new MockFileData(File.ReadAllText(@"TestData\PackageModel_installmanifest.json")));
            IDirectoryInfo installerDir = _fileSystem.DirectoryInfo.FromDirectoryName(installRoot);

            PackageModel packageToRemove = new PackageModel(_fileSystem.FileInfo.FromFileName(installerDir + @"\Bin64\cccmanifest_64.json"))
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

        private IList<PackageModel> ExpectedLoadedPackageModel(string installerRoot)
        {
            return new List<PackageModel>()
            {
                new PackageModel(_fileSystem.FileInfo.FromFileName(installerRoot + @"\Config\InstallManifest.json"))
                {
                Description = "Test2 Description",
                ProductName = "ATest2 productName",
                Url = @"\Parent2\child2\file2.extension",
                Type = "Test2 ptype",
                Keep = true
                },
                new PackageModel(_fileSystem.FileInfo.FromFileName(installerRoot + @"\Bin64\cccmanifest_64.json"))
                {
                Description = "Test Description",
                ProductName = "Test productName",
                Url = @"\Parent\child\file.extension",
                Type = "Test ptype",
                Keep = true
                },
                new PackageModel(_fileSystem.FileInfo.FromFileName(installerRoot + @"\Bin64\cccmanifest_64.json"))
                {
                    Description = "Test2 Description",
                    ProductName = "Test2 productName",
                    Url = @"\Parent2\child2\file2.extension",
                    Type = "Test2 ptype",
                    Keep = true
                },
                new PackageModel(_fileSystem.FileInfo.FromFileName(installerRoot + @"\Config\InstallManifest.json"))
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
                new PackageModel(_fileSystem.FileInfo.FromFileName(installerRoot + @"\Config\InstallManifest.json"))
                {
                Description = "Test2 Description",
                ProductName = "ATest2 productName",
                Url = @"\Parent2\child2\file2.extension",
                Type = "Test2 ptype",
                Keep = true
                },
                new PackageModel(_fileSystem.FileInfo.FromFileName(installerRoot + @"\Bin64\cccmanifest_64.json"))
                {
                    Description = "Test2 Description",
                    ProductName = "Test2 productName",
                    Url = @"\Parent2\child2\file2.extension",
                    Type = "Test2 ptype",
                    Keep = true
                },
                new PackageModel(_fileSystem.FileInfo.FromFileName(installerRoot + @"\Config\InstallManifest.json"))
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
