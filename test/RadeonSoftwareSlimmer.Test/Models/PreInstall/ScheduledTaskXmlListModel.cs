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
    public class ScheduledTaskXmlListModelTest
    {
        private MockFileSystem _fileSystem;

        [SetUp]
        public void Setup()
        {
            _fileSystem = new MockFileSystem();
        }


        [Test]
        public void LoadOrRefresh_WithScheduledTaskFiles_LoadsScheduledTaskList()
        {
            string installRoot = @"C:\Parent\Child\InstallerFolder";
            //These files were created in Task Scheduler on Windows 11 then exported to file
            //Test does not cover all the odd configurations the files come in from AMD
            //Task Scheduler allows OS compatibility options, but not much is different and shouldn't have any impact on this software. All 3 options are tested.
            _fileSystem.AddFile(installRoot + @"\Config\TaskVista.xml", new MockFileData(File.ReadAllText(@"TestData\ScheduledTaskXmlListModel_TaskVista.xml")));
            _fileSystem.AddFile(installRoot + @"\Config\Task7.xml", new MockFileData(File.ReadAllText(@"TestData\ScheduledTaskXmlListModel_Task7.xml")));
            _fileSystem.AddFile(installRoot + @"\Config\Task10.xml", new MockFileData(File.ReadAllText(@"TestData\ScheduledTaskXmlListModel_Task10.xml")));
            IDirectoryInfo installerDir = _fileSystem.DirectoryInfo.FromDirectoryName(installRoot);
            ScheduledTaskXmlListModel scheduledTaskList = new ScheduledTaskXmlListModel(_fileSystem);

            scheduledTaskList.LoadOrRefresh(installerDir);

            Assert.That(scheduledTaskList.ScheduledTasks, Is.Not.Null);
            List<ScheduledTaskXmlModel> actualTasks = scheduledTaskList.ScheduledTasks.ToList();
            IList<ScheduledTaskXmlModel> exectedTasks = ExpectedLoadedScheduledTaskListModel(installRoot);
            Assert.That(actualTasks, Is.Not.Null);
            Assert.That(actualTasks.Count, Is.EqualTo(3));
            Assert.Multiple(() =>
            {
                for (int i = 0; i < 3; i++)
                {
                    Assert.That(actualTasks[i], Is.Not.Null);
                    Assert.That(actualTasks[i].Enabled, Is.EqualTo(exectedTasks[i].Enabled));
                    Assert.That(actualTasks[i].Uri, Is.EqualTo(exectedTasks[i].Uri));
                    Assert.That(actualTasks[i].Command, Is.EqualTo(exectedTasks[i].Command));
                    Assert.That(actualTasks[i].Description, Is.EqualTo(exectedTasks[i].Description));
                }
            });
        }

        [Test]
        public void LoadOrRefresh_NoXmlFiles_LoadsEmptyList()
        {
            string installRoot = @"C:\Parent\Child\InstallerFolder";
            _fileSystem.AddFile(installRoot + @"\Config\Task.foo", new MockFileData(string.Empty));
            _fileSystem.AddFile(installRoot + @"\Config\Test.bar", new MockFileData(string.Empty));
            IDirectoryInfo installerDir = _fileSystem.DirectoryInfo.FromDirectoryName(installRoot);
            ScheduledTaskXmlListModel scheduledTaskList = new ScheduledTaskXmlListModel(_fileSystem);

            scheduledTaskList.LoadOrRefresh(installerDir);

            Assert.That(scheduledTaskList.ScheduledTasks, Is.Not.Null);
            Assert.That(scheduledTaskList.ScheduledTasks.ToList().Count, Is.EqualTo(0));
        }

        [Test]
        public void LoadOrRefresh_XmlFilesNotScheduledTask_LoadsEmptyList()
        {
            string installRoot = @"C:\Parent\Child\InstallerFolder";
            _fileSystem.AddFile(installRoot + @"\Config\Task.xml", new MockFileData(string.Empty));
            _fileSystem.AddFile(installRoot + @"\Config\MonetTST.xml", new MockFileData(string.Empty));
            _fileSystem.AddFile(installRoot + @"\Config\Test.xml", new MockFileData("<?xml version=\"1.0\" encoding=\"UTF - 16\"?><Test><data>asdf</data></Test>"));
            IDirectoryInfo installerDir = _fileSystem.DirectoryInfo.FromDirectoryName(installRoot);
            ScheduledTaskXmlListModel scheduledTaskList = new ScheduledTaskXmlListModel(_fileSystem);

            scheduledTaskList.LoadOrRefresh(installerDir);

            Assert.That(scheduledTaskList.ScheduledTasks, Is.Not.Null);
            Assert.That(scheduledTaskList.ScheduledTasks.ToList().Count, Is.EqualTo(0));
        }


        [Test]
        public void SetScheduledTaskStatusAndUnhide_Enable_EnablesAndUnhides()
        {
            string installRoot = @"C:\Parent\Child\InstallerFolder";
            _fileSystem.AddFile(installRoot + @"\Config\Task.xml", new MockFileData(File.ReadAllText(@"TestData\ScheduledTaskXmlListModel_TaskVista.xml")));
            ScheduledTaskXmlListModel scheduledTaskList = new ScheduledTaskXmlListModel(_fileSystem);
            ScheduledTaskXmlModel scheduledTask = new ScheduledTaskXmlModel(_fileSystem.FileInfo.FromFileName(installRoot + @"\Config\Task.xml"))
            {
                Enabled = true,
                Uri = "\\Test Name Vista",
                Command = @"C:\SomePath\command.exe -arguments",
                Description = "Test Description"
            };

            scheduledTaskList.SetScheduledTaskStatusAndUnhide(scheduledTask);

            string modifiedXml = _fileSystem.GetFile(installRoot + @"\Config\Task.xml").TextContents;
            string expectedXml = File.ReadAllText(@"TestData\ScheduledTaskXmlListModel_TaskEnabled.xml");
            // Writing the file changes the casing of some things, so do a case-insensitive comparison
            Assert.That(modifiedXml.Equals(expectedXml, System.StringComparison.OrdinalIgnoreCase));
        }

        [Test]
        public void SetScheduledTaskStatusAndUnhide_Disable_EnablesAndUnhides()
        {
            string installRoot = @"C:\Parent\Child\InstallerFolder";
            _fileSystem.AddFile(installRoot + @"\Config\Task.xml", new MockFileData(File.ReadAllText(@"TestData\ScheduledTaskXmlListModel_TaskVista.xml")));
            ScheduledTaskXmlListModel scheduledTaskList = new ScheduledTaskXmlListModel(_fileSystem);
            ScheduledTaskXmlModel scheduledTask = new ScheduledTaskXmlModel(_fileSystem.FileInfo.FromFileName(installRoot + @"\Config\Task.xml"))
            {
                Enabled = false,
                Uri = "\\Test Name Vista",
                Command = @"C:\SomePath\command.exe -arguments",
                Description = "Test Description"
            };

            scheduledTaskList.SetScheduledTaskStatusAndUnhide(scheduledTask);

            string modifiedXml = _fileSystem.GetFile(installRoot + @"\Config\Task.xml").TextContents;
            string expectedXml = File.ReadAllText(@"TestData\ScheduledTaskXmlListModel_TaskDisabled.xml");
            // Writing the file changes the casing of some things, so do a case-insensitive comparison
            Assert.That(modifiedXml.Equals(expectedXml, System.StringComparison.OrdinalIgnoreCase));
        }


        private IList<ScheduledTaskXmlModel> ExpectedLoadedScheduledTaskListModel(string installerRoot)
        {
            return new List<ScheduledTaskXmlModel>()
            {
                new ScheduledTaskXmlModel(_fileSystem.FileInfo.FromFileName(installerRoot + @"\Config\TaskVista.xml"))
                {
                    Enabled = true,
                    Uri = "\\Test Name Vista",
                    Command = @"C:\SomePath\command.exe -arguments",
                    Description = "Test Description"
                },
                new ScheduledTaskXmlModel(_fileSystem.FileInfo.FromFileName(installerRoot + @"\Config\Task7.xml"))
                {
                    Enabled = false,
                    Uri = "\\Test Name 7",
                    Command = @"C:\SomePath\command.exe -arguments",
                    Description = "Test Description"
                },
                new ScheduledTaskXmlModel(_fileSystem.FileInfo.FromFileName(installerRoot + @"\Config\Task10.xml"))
                {
                    Enabled = true,
                    Uri = "\\Test Name 10",
                    Command = @"C:\SomePath\command.exe -arguments",
                    Description = "Test Description"
                },
            };
        }
    }
}
