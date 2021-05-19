using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using NUnit.Framework;
using RadeonSoftwareSlimmer.Models.PreInstall;

namespace RadeonSoftwareSlimmer.Test.Models.PreInstall
{
    [TestFixture]
    public class DisplayComponentModelTest
    {
        [Test]
        public void Ctor_Valid_Component_Is_Successful()
        {
            MockFileSystem fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {@"C:\driver\path1\path2\display\component1\driver.inf", new MockFileData(
                    string.Format("dummyline{0}dummyline2{0}[Strings]{0}desc\"test{0}", Environment.NewLine)) }
            });

            IDirectoryInfo rootDir = fileSystem.DirectoryInfo.FromDirectoryName(@"C:\driver");
            IDirectoryInfo componentDir = fileSystem.DirectoryInfo.FromDirectoryName(@"C:\driver\path1\path2\display\component1");

            DisplayComponentModel displayComponentModel = new DisplayComponentModel(rootDir, componentDir);

            Assert.That(displayComponentModel.Description, Is.EqualTo("test"));
            Assert.That(displayComponentModel.Directory, Is.EqualTo(@"\path1\path2\display\component1"));
            Assert.That(displayComponentModel.InfFile, Is.EqualTo("driver.inf"));
            Assert.That(displayComponentModel.Keep, Is.True);
        }
        
        [Test]
        public void Ctor_ExtendedGraphics_Description_Is_Valid()
        {
            MockFileSystem fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {@"C:\driver\path1\path2\display\component1\driver.inf", new MockFileData(
                    string.Format("dummyline{0}dummyline2{0}[Strings]{0}ExtendedGraphics\"test{0}", Environment.NewLine)) }
            });

            IDirectoryInfo rootDir = fileSystem.DirectoryInfo.FromDirectoryName(@"C:\driver");
            IDirectoryInfo componentDir = fileSystem.DirectoryInfo.FromDirectoryName(@"C:\driver\path1\path2\display\component1");

            DisplayComponentModel displayComponentModel = new DisplayComponentModel(rootDir, componentDir);

            Assert.That(displayComponentModel.Description, Is.EqualTo("test"));
            Assert.That(displayComponentModel.Directory, Is.EqualTo(@"\path1\path2\display\component1"));
            Assert.That(displayComponentModel.InfFile, Is.EqualTo("driver.inf"));
            Assert.That(displayComponentModel.Keep, Is.True);
        }

        [Test]
        public void Ctor_No_Inf_InfFile_Is_Null()
        {
            MockFileSystem fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {@"C:\driver\path1\path2\display\component1\driver.somethingelse", new MockFileData(
                    string.Format("dummyline{0}dummyline2{0}[Strings]{0}\"test{0}", Environment.NewLine)) }
            });

            IDirectoryInfo rootDir = fileSystem.DirectoryInfo.FromDirectoryName(@"C:\driver");
            IDirectoryInfo componentDir = fileSystem.DirectoryInfo.FromDirectoryName(@"C:\driver\path1\path2\display\component1");

            DisplayComponentModel displayComponentModel = new DisplayComponentModel(rootDir, componentDir);

            Assert.That(displayComponentModel.Description, Is.Null);
            Assert.That(displayComponentModel.Directory, Is.EqualTo(@"\path1\path2\display\component1"));
            Assert.That(displayComponentModel.InfFile, Is.Null);
            Assert.That(displayComponentModel.Keep, Is.True);
        }

        [Test]
        public void Ctor_EmptyInf_Discription_Is_Null()
        {
            MockFileSystem fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {@"C:\driver\path1\path2\display\component1\driver.inf", new MockFileData(string.Empty) }
            });

            IDirectoryInfo rootDir = fileSystem.DirectoryInfo.FromDirectoryName(@"C:\driver");
            IDirectoryInfo componentDir = fileSystem.DirectoryInfo.FromDirectoryName(@"C:\driver\path1\path2\display\component1");

            DisplayComponentModel displayComponentModel = new DisplayComponentModel(rootDir, componentDir);

            Assert.That(displayComponentModel.Description, Is.Null);
            Assert.That(displayComponentModel.Directory, Is.EqualTo(@"\path1\path2\display\component1"));
            Assert.That(displayComponentModel.InfFile, Is.EqualTo("driver.inf"));
            Assert.That(displayComponentModel.Keep, Is.True);
        }

        [Test]
        public void Ctor_Missing_Strings_Discription_Is_Null()
        {
            MockFileSystem fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {@"C:\driver\path1\path2\display\component1\driver.inf", new MockFileData(
                    string.Format("dummyline{0}dummyline2{0}desc\"test{0}", Environment.NewLine)) }
            });

            IDirectoryInfo rootDir = fileSystem.DirectoryInfo.FromDirectoryName(@"C:\driver");
            IDirectoryInfo componentDir = fileSystem.DirectoryInfo.FromDirectoryName(@"C:\driver\path1\path2\display\component1");

            DisplayComponentModel displayComponentModel = new DisplayComponentModel(rootDir, componentDir);

            Assert.That(displayComponentModel.Description, Is.Null);
            Assert.That(displayComponentModel.Directory, Is.EqualTo(@"\path1\path2\display\component1"));
            Assert.That(displayComponentModel.InfFile, Is.EqualTo("driver.inf"));
            Assert.That(displayComponentModel.Keep, Is.True);
        }

        [Test]
        public void Ctor_Missing_Description_Discription_Is_Null()
        {
            MockFileSystem fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {@"C:\driver\path1\path2\display\component1\driver.inf", new MockFileData(
                    string.Format("dummyline{0}dummyline2{0}[Strings]{0}\"test{0}", Environment.NewLine)) }
            });

            IDirectoryInfo rootDir = fileSystem.DirectoryInfo.FromDirectoryName(@"C:\driver");
            IDirectoryInfo componentDir = fileSystem.DirectoryInfo.FromDirectoryName(@"C:\driver\path1\path2\display\component1");

            DisplayComponentModel displayComponentModel = new DisplayComponentModel(rootDir, componentDir);

            Assert.That(displayComponentModel.Description, Is.Null);
            Assert.That(displayComponentModel.Directory, Is.EqualTo(@"\path1\path2\display\component1"));
            Assert.That(displayComponentModel.InfFile, Is.EqualTo("driver.inf"));
            Assert.That(displayComponentModel.Keep, Is.True);
        }

        [Test]
        public void Ctor_EOF_After_Strings_Discription_Is_Null()
        {
            MockFileSystem fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {@"C:\driver\path1\path2\display\component1\driver.inf", new MockFileData(
                    string.Format("dummyline{0}dummyline2{0}[Strings]", Environment.NewLine)) }
            });

            IDirectoryInfo rootDir = fileSystem.DirectoryInfo.FromDirectoryName(@"C:\driver");
            IDirectoryInfo componentDir = fileSystem.DirectoryInfo.FromDirectoryName(@"C:\driver\path1\path2\display\component1");

            DisplayComponentModel displayComponentModel = new DisplayComponentModel(rootDir, componentDir);

            Assert.That(displayComponentModel.Description, Is.Null);
            Assert.That(displayComponentModel.Directory, Is.EqualTo(@"\path1\path2\display\component1"));
            Assert.That(displayComponentModel.InfFile, Is.EqualTo("driver.inf"));
            Assert.That(displayComponentModel.Keep, Is.True);
        }


        [Test]
        public void Remove_Directory_Does_Not_Exist_Does_Nothing()
        {
            MockFileSystem fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {@"C:\driver\path1\path2\display\component1\driver.inf", new MockFileData(string.Empty) }
            });

            IDirectoryInfo rootDir = fileSystem.DirectoryInfo.FromDirectoryName(@"C:\driver");
            IDirectoryInfo componentDir = fileSystem.DirectoryInfo.FromDirectoryName(@"C:\driver\path1\path2\display\component1");
            DisplayComponentModel displayComponentModel = new DisplayComponentModel(rootDir, componentDir);
            componentDir.Delete(true);
            displayComponentModel.Keep = false;

            displayComponentModel.Remove();

            Assert.That(rootDir.Exists, Is.True);
        }

        [Test]
        public void Remove_Keep_True_Does_Not_Delete()
        {
            MockFileSystem fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {@"C:\driver\path1\path2\display\component1\driver.inf", new MockFileData(string.Empty) }
            });

            IDirectoryInfo rootDir = fileSystem.DirectoryInfo.FromDirectoryName(@"C:\driver");
            IDirectoryInfo componentDir = fileSystem.DirectoryInfo.FromDirectoryName(@"C:\driver\path1\path2\display\component1");
            DisplayComponentModel displayComponentModel = new DisplayComponentModel(rootDir, componentDir);
            displayComponentModel.Keep = true;

            displayComponentModel.Remove();

            Assert.That(componentDir.Exists, Is.True);
            Assert.That(componentDir.GetFiles().Length, Is.EqualTo(1));
            Assert.That(rootDir.Exists, Is.True);
        }

        [Test]
        public void Remove_Deletes_Readonly_Files()
        {
            MockFileSystem fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {@"C:\driver\path1\path2\display\component1\driver.inf", new MockFileData(string.Empty) }
            });

            IDirectoryInfo rootDir = fileSystem.DirectoryInfo.FromDirectoryName(@"C:\driver");
            IDirectoryInfo componentDir = fileSystem.DirectoryInfo.FromDirectoryName(@"C:\driver\path1\path2\display\component1");
            DisplayComponentModel displayComponentModel = new DisplayComponentModel(rootDir, componentDir);
            displayComponentModel.Keep = false;
            componentDir.GetFiles()[0].IsReadOnly = true;

            displayComponentModel.Remove();

            Assert.That(componentDir.Exists, Is.False);
            Assert.That(rootDir.Exists, Is.True);
        }

        [Test]
        public void Remove_Keep_False_Does_Delete()
        {
            MockFileSystem fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {@"C:\driver\path1\path2\display\component1\driver.inf", new MockFileData(string.Empty) }
            });

            IDirectoryInfo rootDir = fileSystem.DirectoryInfo.FromDirectoryName(@"C:\driver");
            IDirectoryInfo componentDir = fileSystem.DirectoryInfo.FromDirectoryName(@"C:\driver\path1\path2\display\component1");
            DisplayComponentModel displayComponentModel = new DisplayComponentModel(rootDir, componentDir);
            displayComponentModel.Keep = false;

            displayComponentModel.Remove();

            Assert.That(componentDir.Exists, Is.False);
            Assert.That(rootDir.Exists, Is.True);
        }
    }
}
