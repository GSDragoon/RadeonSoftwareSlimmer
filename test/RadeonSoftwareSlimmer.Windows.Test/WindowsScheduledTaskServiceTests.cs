using Microsoft.Win32.TaskScheduler;
using NUnit.Framework;
using RadeonSoftwareSlimmer.Core.Interfaces;

namespace RadeonSoftwareSlimmer.Windows.Test
{
    [NonParallelizable]
    public class WindowsScheduledTaskServiceTests
    {
        private const string TaskName = "RadeonSoftwareSlimmerTest_ServiceTask";
        private const string TaskDescription = "Scratch task for RadeonSoftwareSlimmer.Windows.Test";

        [SetUp]
        public void SetUp()
        {
            DeleteScratchTask();
            TaskDefinition td = TaskService.Instance.NewTask();
            td.RegistrationInfo.Description = TaskDescription;
            td.RegistrationInfo.Author = "RadeonSoftwareSlimmerTest";
            td.Settings.Enabled = true;
            td.Actions.Add(new ExecAction("cmd.exe", "/c echo hello", null));
            TaskService.Instance.RootFolder.RegisterTaskDefinition(TaskName, td);
        }

        [TearDown]
        public void TearDown()
        {
            DeleteScratchTask();
        }

        private static void DeleteScratchTask()
        {
            try
            {
                TaskService.Instance.RootFolder.DeleteTask(TaskName, exceptionOnNotExists: false);
            }
            catch
            {
                // Task Scheduler occasionally reports errors on missing tasks even when told not to; swallow for test cleanup.
            }
        }


        [Test]
        public void GetTask_ExistingTask_ReturnsScheduledTask()
        {
            WindowsScheduledTaskService service = new WindowsScheduledTaskService();

            IScheduledTask task = service.GetTask(TaskName);

            Assert.Multiple((System.Action)(() =>
            {
                Assert.That(task, Is.Not.Null);
                Assert.That(task.Name, Is.EqualTo(TaskName));
            }));
        }

        [Test]
        public void GetTask_NonExistentTask_Throws()
        {
            WindowsScheduledTaskService service = new WindowsScheduledTaskService();

            Assert.That((System.Action)(() => service.GetTask("RadeonSoftwareSlimmerDoesNotExistTask")),
                Throws.ArgumentNullException);
        }


        [Test]
        public void FindAllTasks_FilterMatchesScratchTask_ReturnsIt()
        {
            WindowsScheduledTaskService service = new WindowsScheduledTaskService();

            IScheduledTask[] tasks = service.FindAllTasks(t => t.Name == TaskName, searchAllFolders: false);

            Assert.That(tasks, Has.Length.EqualTo(1));
        }

        [Test]
        public void FindAllTasks_FilterMatchesNothing_ReturnsEmpty()
        {
            WindowsScheduledTaskService service = new WindowsScheduledTaskService();

            IScheduledTask[] tasks = service.FindAllTasks(_ => false, searchAllFolders: false);

            Assert.That(tasks, Is.Empty);
        }
    }
}
