using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using NUnit.Framework;
using RadeonSoftwareSlimmer.Models.PostInstall;

namespace RadeonSoftwareSlimmer.Test.Models.PostInstall
{
    public class TempFileModelTest
    {
        [Test]
        public void Ctor_FolderDoesNotExist_ThrowsIONotFoundException()
        {
            MockFileSystem fileSystem = new MockFileSystem();
            fileSystem.AddDirectory(@"C:\SomePath\FolderExists");

            Assert.That(() => { _ = new TempFileModel(@"c:\SomePath\FolderDoesNotExist", fileSystem); }, Throws.TypeOf<DirectoryNotFoundException>());
        }

        [Test]
        public void Ctor_FolderIsEmpty_FolderSizeAndCountAreZero()
        {
            string folderPath = @"C:\Path\Folder\EmptyFolder";
            MockFileSystem fileSystem = new MockFileSystem();
            fileSystem.AddDirectory(folderPath);

            TempFileModel tempFileModel = new TempFileModel(folderPath, fileSystem);

            Assert.Multiple(() =>
            {
                Assert.That(tempFileModel.Files, Is.Zero);
                Assert.That(tempFileModel.Size, Is.EqualTo("0 bytes"));
            });
        }

        [Test]
        public void Ctor_FolderNameIsSetAndClearIsFalse()
        {
            string folderPath = @"C:\Path\Folder";
            MockFileSystem fileSystem = new MockFileSystem();
            fileSystem.AddDirectory(folderPath);

            TempFileModel tempFileModel = new TempFileModel(folderPath, fileSystem);

            Assert.Multiple(() =>
            {
                Assert.That(tempFileModel.Folder, Is.EqualTo(folderPath));
                Assert.That(tempFileModel.Clear, Is.False);
            });
        }

        [Test]
        public void Ctor_OneFile_FileCountIsOne()
        {
            string folderPath = @"C:\Path\Folder";
            MockFileSystem fileSystem = new MockFileSystem();
            fileSystem.AddFile(folderPath + "\\file", new MockFileData(string.Empty));

            TempFileModel tempFileModel = new TempFileModel(folderPath, fileSystem);

            Assert.That(tempFileModel.Files, Is.EqualTo(1));
        }

        [Test]
        public void Ctor_TenFiles_FileCountIsTen()
        {
            string folderPath = @"C:\Path\Folder";
            MockFileSystem fileSystem = new MockFileSystem();
            MockFileData emptyFileData = new MockFileData(string.Empty);
            fileSystem.AddFile(folderPath + @"\ChildFolder1\file.txt", emptyFileData);
            fileSystem.AddFile(folderPath + @"\ChildFolder2\file.txt", emptyFileData);
            fileSystem.AddFile(folderPath + @"\ChildFolder2\file2.txt", emptyFileData);
            fileSystem.AddFile(folderPath + @"\ChildFolder2\file3.txt", emptyFileData);
            fileSystem.AddFile(folderPath + @"\ChildFolder2\file4.txt", emptyFileData);
            fileSystem.AddFile(folderPath + @"\ChildFolder2\file5", emptyFileData);
            fileSystem.AddFile(folderPath + @"\ChildFolder3\file.abc", emptyFileData);
            fileSystem.AddFile(folderPath + @"\ChildFolder3\Child1\file.txt", emptyFileData);
            fileSystem.AddFile(folderPath + @"\ChildFolder3\Child2\file.txt", emptyFileData);
            fileSystem.AddFile(folderPath + @"\ChildFolder3\Child2\file.wtf", emptyFileData);
            fileSystem.AddDirectory(folderPath + @"\ChildFolder3\Child3");

            TempFileModel tempFileModel = new TempFileModel(folderPath, fileSystem);

            Assert.That(tempFileModel.Files, Is.EqualTo(10));
        }

        [Test]
        public void Ctor_MultipleFiles_FileSizeIsInBytes()
        {
            string folderPath = @"C:\Path\Folder";
            MockFileSystem fileSystem = new MockFileSystem();
            fileSystem.AddFile(folderPath + "\\file1", new MockFileData(new byte[123]));
            fileSystem.AddFile(folderPath + "\\file2", new MockFileData(new byte[456]));

            TempFileModel tempFileModel = new TempFileModel(folderPath, fileSystem);

            Assert.Multiple(() =>
            {
                Assert.That(tempFileModel.Files, Is.EqualTo(2));
                Assert.That(tempFileModel.Size, Is.EqualTo("579 bytes"));
            });
        }

        [Test]
        public void Ctor_MultipleFiles_FileSizeIsInKB()
        {
            string folderPath = @"C:\Path\Folder";
            MockFileSystem fileSystem = new MockFileSystem();
            fileSystem.AddFile(folderPath + "\\file1", new MockFileData(new byte[123456]));
            fileSystem.AddFile(folderPath + "\\file2", new MockFileData(new byte[456789]));

            TempFileModel tempFileModel = new TempFileModel(folderPath, fileSystem);

            Assert.Multiple(() =>
            {
                Assert.That(tempFileModel.Files, Is.EqualTo(2));
                Assert.That(tempFileModel.Size, Is.EqualTo("566.65 KB"));
            });
        }

        [Test]
        public void Ctor_MultipleFiles_FileSizeIsInMB()
        {
            string folderPath = @"C:\Path\Folder";
            MockFileSystem fileSystem = new MockFileSystem();
            fileSystem.AddFile(folderPath + "\\file1", new MockFileData(new byte[12345678]));
            fileSystem.AddFile(folderPath + "\\file2", new MockFileData(new byte[987654]));

            TempFileModel tempFileModel = new TempFileModel(folderPath, fileSystem);

            Assert.Multiple(() =>
            {
                Assert.That(tempFileModel.Files, Is.EqualTo(2));
                Assert.That(tempFileModel.Size, Is.EqualTo("12.72 MB"));
            });
        }

        [Test]
        public void Ctor_MultipleFiles_FileSizeIsInGB()
        {
            string folderPath = @"C:\Path\Folder";
            MockFileSystem fileSystem = new MockFileSystem();
            fileSystem.AddFile(folderPath + "\\file1", new MockFileData(new byte[1234567890]));
            fileSystem.AddFile(folderPath + "\\file2", new MockFileData(new byte[987654321]));

            TempFileModel tempFileModel = new TempFileModel(folderPath, fileSystem);

            Assert.Multiple(() =>
            {
                Assert.That(tempFileModel.Files, Is.EqualTo(2));
                Assert.That(tempFileModel.Size, Is.EqualTo("2.07 GB"));
            });
        }

        [Test]
        public void ClearFolder_SingleFolderMultipleFiles_IsCleared()
        {
            string folderPath = @"C:\Path\Folder";
            MockFileSystem fileSystem = new MockFileSystem();
            MockFileData emptyFileData = new MockFileData(string.Empty);
            fileSystem.AddFile(folderPath + @"\file1.txt", emptyFileData);
            fileSystem.AddFile(folderPath + @"\file2.txt", emptyFileData);
            TempFileModel tempFileModel = new TempFileModel(folderPath, fileSystem);
            tempFileModel.Clear = true;

            tempFileModel.ClearFolder();

            Assert.Multiple(() =>
            {
                Assert.That(fileSystem.Directory.Exists(folderPath), Is.True);
                Assert.That(fileSystem.Directory.EnumerateDirectories(folderPath, "*").Count(), Is.EqualTo(0));
                Assert.That(fileSystem.Directory.EnumerateFiles(folderPath, "*").Count(), Is.EqualTo(0));
            });
        }

        [Test]
        public void ClearFolder_MultipleFoldersAndFiles_IsCleared()
        {
            string folderPath = @"C:\Path\Folder";
            MockFileSystem fileSystem = new MockFileSystem();
            MockFileData emptyFileData = new MockFileData(string.Empty);
            fileSystem.AddFile(folderPath + @"\ChildFolder1\file.txt", emptyFileData);
            fileSystem.AddFile(folderPath + @"\ChildFolder2\file.txt", emptyFileData);
            fileSystem.AddFile(folderPath + @"\ChildFolder2\file2.txt", emptyFileData);
            fileSystem.AddFile(folderPath + @"\ChildFolder2\file3.txt", emptyFileData);
            fileSystem.AddFile(folderPath + @"\ChildFolder2\file4.txt", emptyFileData);
            fileSystem.AddFile(folderPath + @"\ChildFolder2\file5", emptyFileData);
            fileSystem.AddFile(folderPath + @"\ChildFolder3\file.abc", emptyFileData);
            fileSystem.AddFile(folderPath + @"\ChildFolder3\Child1\file.txt", emptyFileData);
            fileSystem.AddFile(folderPath + @"\ChildFolder3\Child2\file.txt", emptyFileData);
            fileSystem.AddFile(folderPath + @"\ChildFolder3\Child2\file.wtf", emptyFileData);
            fileSystem.AddDirectory(folderPath + @"\ChildFolder3\Child3");
            TempFileModel tempFileModel = new TempFileModel(folderPath, fileSystem);
            tempFileModel.Clear = true;

            tempFileModel.ClearFolder();

            Assert.Multiple(() =>
            {
                Assert.That(fileSystem.Directory.Exists(folderPath), Is.True);
                Assert.That(fileSystem.Directory.EnumerateDirectories(folderPath, "*").Count(), Is.EqualTo(0));
                Assert.That(fileSystem.Directory.EnumerateFiles(folderPath, "*").Count(), Is.EqualTo(0));
            });
        }
    }
}
