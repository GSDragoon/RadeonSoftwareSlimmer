using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using NUnit.Framework;
using RadeonSoftwareSlimmer.Models.PreInstall;

namespace RadeonSoftwareSlimmer.Test.Models.PreInstall
{
    public class PackageModelTest
    {
        private MockFileSystem _fileSystem;
        private IFileInfo _dummyFile;

        [SetUp]
        public void Setup()
        {
            string filePath = @"C:\Some\Path\To\A\File\foobar.json";
            _fileSystem = new MockFileSystem();
            _fileSystem.AddFile(filePath, new MockFileData(string.Empty));
            _dummyFile = _fileSystem.FileInfo.New(filePath);
        }


        [Test]
        public void Ctor_DefaultsKeepToTrue()
        {
            PackageModel packageModel = new PackageModel(_dummyFile);

            Assert.That(packageModel.Keep, Is.True);
        }


        [Test]
        public void GetFile_ReturnsFile()
        {
            PackageModel packageModel = new PackageModel(_dummyFile);

            IFileInfo returnFile = packageModel.GetFile();

            Assert.That(returnFile, Is.Not.Null);
            Assert.That(returnFile, Is.EqualTo(_dummyFile));
        }


        [Test]
        public void Equals_SameProperties_ReturnsTrue()
        {
            PackageModel packageModel = new PackageModel(_dummyFile);
            packageModel.Keep = false;
            packageModel.ProductName = "Test Product Name";
            packageModel.Url = "Test URL";
            packageModel.Type = "Test Type";
            packageModel.Description = "Test Description";
            
            string filePath = @"C:\Some\Path\To\A\File\foobar.json";
            MockFileSystem fileSystem = new MockFileSystem();
            fileSystem.AddFile(filePath, new MockFileData(string.Empty));
            IFileInfo dummyFile = _fileSystem.FileInfo.New(filePath);
            PackageModel duplicatePackageModel = new PackageModel(dummyFile);
            duplicatePackageModel.Keep = false;
            duplicatePackageModel.ProductName = "Test Product Name";
            duplicatePackageModel.Url = "Test URL";
            duplicatePackageModel.Type = "Test Type";
            duplicatePackageModel.Description = "Test Description";


            bool equals = packageModel.Equals(duplicatePackageModel);
            
            Assert.That(equals, Is.True);
        }

        [Test]
        public void Equals_DifferentProperties_ReturnsFalse()
        {
            PackageModel packageModel = new PackageModel(_dummyFile);
            packageModel.Keep = false;
            packageModel.ProductName = "Test Product Name";
            packageModel.Url = "Test URL";
            packageModel.Type = "Test Type";
            packageModel.Description = "Test Description";

            string filePath = @"C:\Some\Path\To\A\File\Differentfoobar.json";
            MockFileSystem fileSystem = new MockFileSystem();
            fileSystem.AddFile(filePath, new MockFileData(string.Empty));
            IFileInfo dummyFile = _fileSystem.FileInfo.New(filePath);
            PackageModel duplicatePackageModel = new PackageModel(dummyFile);
            duplicatePackageModel.Keep = false;
            duplicatePackageModel.ProductName = "Different Test Product Name";
            duplicatePackageModel.Url = "Different Test URL";
            duplicatePackageModel.Type = "Different Test Type";
            duplicatePackageModel.Description = "Different Test Description";


            bool equals = packageModel.Equals(duplicatePackageModel);

            Assert.That(equals, Is.False);
        }

        [Test]
        public void Equals_IsNull_ReturnsFalse()
        {
            PackageModel packageModel = new PackageModel(_dummyFile);
            packageModel.Keep = false;
            packageModel.ProductName = "Test Product Name";
            packageModel.Url = "Test URL";
            packageModel.Type = "Test Type";
            packageModel.Description = "Test Description";


            bool equals = packageModel.Equals(null);

            Assert.That(equals, Is.False);
        }
    }
}
