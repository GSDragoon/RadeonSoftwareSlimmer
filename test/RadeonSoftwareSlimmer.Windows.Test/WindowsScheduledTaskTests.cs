using Microsoft.Win32.TaskScheduler;
using NUnit.Framework;
using RadeonSoftwareSlimmer.Core.Enums;

namespace RadeonSoftwareSlimmer.Windows.Test
{
    [NonParallelizable]
    public class WindowsScheduledTaskTests
    {
        private const string TaskName = "RadeonSoftwareSlimmerTest_ModelTask";
        private const string TaskAuthor = "RadeonSoftwareSlimmerTest";
        private const string TaskDescription = "Scratch task for RadeonSoftwareSlimmer.Windows.Test";
        private const string TaskCommand = "cmd.exe";
        private const string TaskArguments = "/c echo hello";

        [SetUp]
        public void SetUp()
        {
            DeleteScratchTask();
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
                // Swallow — test cleanup only.
            }
        }

        private static void RegisterScratchTask(bool enabled)
        {
            TaskDefinition td = TaskService.Instance.NewTask();
            td.RegistrationInfo.Description = TaskDescription;
            td.RegistrationInfo.Author = TaskAuthor;
            td.Settings.Enabled = enabled;
            td.Actions.Add(new ExecAction(TaskCommand, TaskArguments, null));
            TaskService.Instance.RootFolder.RegisterTaskDefinition(TaskName, td);
        }

        private static Task GetScratchTask()
        {
            return TaskService.Instance.GetTask(TaskName);
        }


        [Test]
        public void Ctor_NullTask_Throws()
        {
            Assert.That((System.Action)(() => new WindowsScheduledTask(null)), Throws.ArgumentNullException);
        }

        [Test]
        public void Ctor_LoadsAllPropertiesFromUnderlyingTask()
        {
            RegisterScratchTask(enabled: true);

            using (Task task = GetScratchTask())
            {
                WindowsScheduledTask windowsTask = new WindowsScheduledTask(task);

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(windowsTask.Name, Is.EqualTo(TaskName));
                    Assert.That(windowsTask.Description, Is.EqualTo(TaskDescription));
                    Assert.That(windowsTask.Author, Is.EqualTo(TaskAuthor));
                    Assert.That(windowsTask.Enabled, Is.True);
                    Assert.That(windowsTask.Command, Does.Contain(TaskCommand));
                    Assert.That(windowsTask.Command, Does.Contain(TaskArguments));
                }
            }
        }


        [Test]
        public void Enable_DisabledTask_EnablesUnderlyingTask()
        {
            RegisterScratchTask(enabled: false);
            WindowsScheduledTask windowsTask;
            using (Task task = GetScratchTask())
            {
                windowsTask = new WindowsScheduledTask(task);
            }

            windowsTask.Enable();

            using (Task refreshed = GetScratchTask())
            {
                using (Assert.EnterMultipleScope())
                {
                    Assert.That(refreshed.Enabled, Is.True);
                    Assert.That(windowsTask.Enabled, Is.True);
                }
            }
        }

        [Test]
        public void Enable_AlreadyEnabledTask_LeavesEnabled()
        {
            RegisterScratchTask(enabled: true);
            WindowsScheduledTask windowsTask;
            using (Task task = GetScratchTask())
            {
                windowsTask = new WindowsScheduledTask(task);
            }

            windowsTask.Enable();

            using (Task refreshed = GetScratchTask())
            {
                Assert.That(refreshed.Enabled, Is.True);
            }
        }


        [Test]
        public void Disable_EnabledTask_DisablesUnderlyingTask()
        {
            RegisterScratchTask(enabled: true);
            WindowsScheduledTask windowsTask;
            using (Task task = GetScratchTask())
            {
                windowsTask = new WindowsScheduledTask(task);
            }

            windowsTask.Disable();

            using (Task refreshed = GetScratchTask())
            {
                using (Assert.EnterMultipleScope())
                {
                    Assert.That(refreshed.Enabled, Is.False);
                    Assert.That(windowsTask.Enabled, Is.False);
                    Assert.That(windowsTask.State, Is.EqualTo(CoreTaskState.Disabled));
                }
            }
        }

        [Test]
        public void Disable_AlreadyDisabledTask_LeavesDisabled()
        {
            RegisterScratchTask(enabled: false);
            WindowsScheduledTask windowsTask;
            using (Task task = GetScratchTask())
            {
                windowsTask = new WindowsScheduledTask(task);
            }

            windowsTask.Disable();

            using (Task refreshed = GetScratchTask())
            {
                Assert.That(refreshed.Enabled, Is.False);
            }
        }
    }
}
