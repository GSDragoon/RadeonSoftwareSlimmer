using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RadeonSoftwareSlimmer.Core.PreInstall;
using RadeonSoftwareSlimmer.Core.Test.TestDoubles;

namespace RadeonSoftwareSlimmer.Core.Test.Models.PreInstall
{
    [SuppressMessage("System.IO.Abstractions", "IO0002:Replace File class with IFileSystem.File for improved testability", Justification = "Reading from test data file")]
    [SuppressMessage("System.IO.Abstractions", "IO0006:Replace Path class with IFileSystem.Path for improved testability", Justification = "Composing real-disk TestData paths and MockFileSystem input paths from raw strings")]
    public class ScheduledTaskXmlListModelTest
    {
        private static readonly string InstallRoot = TestPath.Rooted(@"Parent\Child\InstallerFolder");

        private MockFileSystem _fileSystem;
        private FakeAppLogger _logger;
        private string _testDataDirectory;

        [SetUp]
        [SuppressMessage("System.IO.Abstractions", "IO0006:Replace Path class with IFileSystem.Path for improved testability", Justification = "Used to set path to load files from VS and command line")]
        public void Setup()
        {
            _fileSystem = new MockFileSystem();
            _logger = new FakeAppLogger();
            _testDataDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TestData");
        }


        [Test]
        public void LoadOrRefresh_WithScheduledTaskFiles_LoadsScheduledTaskList()
        {
            //These files were created in Task Scheduler on Windows 11 then exported to file
            //Test does not cover all the odd configurations the files come in from AMD
            //Task Scheduler allows OS compatibility options, but not much is different and shouldn't have any impact on this software. All 3 options are tested.
            _fileSystem.AddFile(Path.Combine(InstallRoot, "Config", "TaskVista.xml"), new MockFileData(File.ReadAllText(Path.Combine(_testDataDirectory, "ScheduledTaskXmlListModel_TaskVista.xml"))));
            _fileSystem.AddFile(Path.Combine(InstallRoot, "Config", "Task7.xml"), new MockFileData(File.ReadAllText(Path.Combine(_testDataDirectory, "ScheduledTaskXmlListModel_Task7.xml"))));
            _fileSystem.AddFile(Path.Combine(InstallRoot, "Config", "Task10.xml"), new MockFileData(File.ReadAllText(Path.Combine(_testDataDirectory, "ScheduledTaskXmlListModel_Task10.xml"))));
            IDirectoryInfo installerDir = _fileSystem.DirectoryInfo.New(InstallRoot);
            ScheduledTaskXmlListModel scheduledTaskList = new ScheduledTaskXmlListModel(_fileSystem, _logger);

            scheduledTaskList.LoadOrRefresh(installerDir);

            Assert.That(scheduledTaskList.ScheduledTasks, Is.Not.Null);
            List<ScheduledTaskXmlModel> actualTasks = scheduledTaskList.ScheduledTasks.ToList();
            IList<ScheduledTaskXmlModel> exectedTasks = ExpectedLoadedScheduledTaskListModel(InstallRoot);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(actualTasks, Is.Not.Null);
                Assert.That(actualTasks, Has.Count.EqualTo(3));

                for (int i = 0; i < 3; i++)
                {
                    Assert.That(actualTasks[i], Is.Not.Null);
                    Assert.That(actualTasks[i].Enabled, Is.EqualTo(exectedTasks[i].Enabled));
                    Assert.That(actualTasks[i].Uri, Is.EqualTo(exectedTasks[i].Uri));
                    Assert.That(actualTasks[i].Command, Is.EqualTo(exectedTasks[i].Command));
                    Assert.That(actualTasks[i].Description, Is.EqualTo(exectedTasks[i].Description));
                }
            }
        }

        [Test]
        public void LoadOrRefresh_NoXmlFiles_LoadsEmptyList()
        {
            _fileSystem.AddFile(Path.Combine(InstallRoot, "Config", "Task.foo"), new MockFileData(string.Empty));
            _fileSystem.AddFile(Path.Combine(InstallRoot, "Config", "Test.bar"), new MockFileData(string.Empty));
            IDirectoryInfo installerDir = _fileSystem.DirectoryInfo.New(InstallRoot);
            ScheduledTaskXmlListModel scheduledTaskList = new ScheduledTaskXmlListModel(_fileSystem, _logger);

            scheduledTaskList.LoadOrRefresh(installerDir);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(scheduledTaskList.ScheduledTasks, Is.Not.Null);
                Assert.That(scheduledTaskList.ScheduledTasks.ToList(), Is.Empty);
            }
        }

        [Test]
        public void LoadOrRefresh_XmlFilesNotScheduledTask_LoadsEmptyList()
        {
            _fileSystem.AddFile(Path.Combine(InstallRoot, "Config", "Task.xml"), new MockFileData(string.Empty));
            _fileSystem.AddFile(Path.Combine(InstallRoot, "Config", "MonetTST.xml"), new MockFileData(string.Empty));
            _fileSystem.AddFile(Path.Combine(InstallRoot, "Config", "Test.xml"), new MockFileData("<?xml version=\"1.0\" encoding=\"UTF - 16\"?><Test><data>asdf</data></Test>"));
            IDirectoryInfo installerDir = _fileSystem.DirectoryInfo.New(InstallRoot);
            ScheduledTaskXmlListModel scheduledTaskList = new ScheduledTaskXmlListModel(_fileSystem, _logger);

            scheduledTaskList.LoadOrRefresh(installerDir);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(scheduledTaskList.ScheduledTasks, Is.Not.Null);
                Assert.That(scheduledTaskList.ScheduledTasks.ToList(), Is.Empty);
            }
        }


        [Test]
        public void SetScheduledTaskStatusAndUnhide_Enable_EnablesAndUnhides()
        {
            _fileSystem.AddFile(Path.Combine(InstallRoot, "Config", "Task.xml"), new MockFileData(File.ReadAllText(Path.Combine(_testDataDirectory, "ScheduledTaskXmlListModel_TaskVista.xml"))));
            ScheduledTaskXmlListModel scheduledTaskList = new ScheduledTaskXmlListModel(_fileSystem, _logger);
            ScheduledTaskXmlModel scheduledTask = new ScheduledTaskXmlModel(_fileSystem.FileInfo.New(Path.Combine(InstallRoot, "Config", "Task.xml")))
            {
                Enabled = true,
                Uri = "\\Test Name Vista",
                Command = @"C:\SomePath\command.exe -arguments",
                Description = "Test Description"
            };

            scheduledTaskList.SetScheduledTaskStatusAndUnhide(scheduledTask);

            string modifiedXml = _fileSystem.GetFile(Path.Combine(InstallRoot, "Config", "Task.xml")).TextContents.Replace("\r\n", "\n");
            string expectedXml = File.ReadAllText(Path.Combine(_testDataDirectory, "ScheduledTaskXmlListModel_TaskEnabled.xml")).Replace("\r\n", "\n");
            Assert.That(modifiedXml, Is.EqualTo(expectedXml));
        }

        [Test]
        public void SetScheduledTaskStatusAndUnhide_Disable_EnablesAndUnhides()
        {
            _fileSystem.AddFile(Path.Combine(InstallRoot, "Config", "Task.xml"), new MockFileData(File.ReadAllText(Path.Combine(_testDataDirectory, "ScheduledTaskXmlListModel_TaskVista.xml"))));
            ScheduledTaskXmlListModel scheduledTaskList = new ScheduledTaskXmlListModel(_fileSystem, _logger);
            ScheduledTaskXmlModel scheduledTask = new ScheduledTaskXmlModel(_fileSystem.FileInfo.New(Path.Combine(InstallRoot, "Config", "Task.xml")))
            {
                Enabled = false,
                Uri = "\\Test Name Vista",
                Command = @"C:\SomePath\command.exe -arguments",
                Description = "Test Description"
            };

            scheduledTaskList.SetScheduledTaskStatusAndUnhide(scheduledTask);

            string modifiedXml = _fileSystem.GetFile(Path.Combine(InstallRoot, "Config", "Task.xml")).TextContents.Replace("\r\n", "\n");
            string expectedXml = File.ReadAllText(Path.Combine(_testDataDirectory, "ScheduledTaskXmlListModel_TaskDisabled.xml")).Replace("\r\n", "\n");
            Assert.That(modifiedXml, Is.EqualTo(expectedXml));
        }

        [Test]
        public void RestoreToDefault_SetsAllScheduledTasksToEnabled()
        {
            _fileSystem.AddFile(Path.Combine(InstallRoot, "Config", "TaskVista.xml"), new MockFileData(File.ReadAllText(Path.Combine(_testDataDirectory, "ScheduledTaskXmlListModel_TaskVista.xml"))));
            _fileSystem.AddFile(Path.Combine(InstallRoot, "Config", "Task7.xml"), new MockFileData(File.ReadAllText(Path.Combine(_testDataDirectory, "ScheduledTaskXmlListModel_Task7.xml"))));
            _fileSystem.AddFile(Path.Combine(InstallRoot, "Config", "Task10.xml"), new MockFileData(File.ReadAllText(Path.Combine(_testDataDirectory, "ScheduledTaskXmlListModel_Task10.xml"))));
            IDirectoryInfo installerDir = _fileSystem.DirectoryInfo.New(InstallRoot);
            ScheduledTaskXmlListModel scheduledTaskList = new ScheduledTaskXmlListModel(_fileSystem, _logger);
            scheduledTaskList.LoadOrRefresh(installerDir);

            scheduledTaskList.RestoreToDefault();

            scheduledTaskList.LoadOrRefresh(installerDir);
            foreach (ScheduledTaskXmlModel scheduledTask in scheduledTaskList.ScheduledTasks)
            {
                Assert.That(scheduledTask.Enabled, Is.True);
            }
        }


        private List<ScheduledTaskXmlModel> ExpectedLoadedScheduledTaskListModel(string installerRoot)
        {
            return new List<ScheduledTaskXmlModel>()
            {
                new ScheduledTaskXmlModel(_fileSystem.FileInfo.New(Path.Combine(installerRoot, "Config", "TaskVista.xml")))
                {
                    Enabled = true,
                    Uri = "\\Test Name Vista",
                    Command = @"C:\SomePath\command.exe -arguments",
                    Description = "Test Description"
                },
                new ScheduledTaskXmlModel(_fileSystem.FileInfo.New(Path.Combine(installerRoot, "Config", "Task7.xml")))
                {
                    Enabled = false,
                    Uri = "\\Test Name 7",
                    Command = @"C:\SomePath\command.exe -arguments",
                    Description = "Test Description"
                },
                new ScheduledTaskXmlModel(_fileSystem.FileInfo.New(Path.Combine(installerRoot, "Config", "Task10.xml")))
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
