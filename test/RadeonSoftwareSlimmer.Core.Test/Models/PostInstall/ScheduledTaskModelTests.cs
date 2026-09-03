using System;
using NUnit.Framework;
using RadeonSoftwareSlimmer.Core.Enums;
using RadeonSoftwareSlimmer.Core.PostInstall;
using RadeonSoftwareSlimmer.Core.Test.TestDoubles;

namespace RadeonSoftwareSlimmer.Core.Test.Models.PostInstall
{
    public class ScheduledTaskModelTests
    {
        private const string TaskName = "TestTask";

        private FakeAppLogger _logger;
        private FakeScheduledTaskService _taskService;

        [SetUp]
        public void Setup()
        {
            _logger = new FakeAppLogger();
            _taskService = new FakeScheduledTaskService();
        }


        [Test]
        public void Ctor_NullTask_ThrowsArgumentNullException()
        {
            Assert.That((System.Action)(() => new ScheduledTaskModel(null, _logger, _taskService)), Throws.ArgumentNullException);
        }

        [Test]
        public void Ctor_ValidTask_LoadsAllProperties()
        {
            DateTime lastRun = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);
            FakeScheduledTask task = new FakeScheduledTask
            {
                Name = TaskName,
                Description = "Test description",
                Enabled = true,
                IsActive = true,
                State = CoreTaskState.Ready,
                Command = @"C:\test.exe",
                LastRunTime = lastRun,
            };

            ScheduledTaskModel model = new ScheduledTaskModel(task, _logger, _taskService);

            Assert.Multiple((System.Action)(() =>
            {
                Assert.That(model.Name, Is.EqualTo(TaskName));
                Assert.That(model.Description, Is.EqualTo("Test description"));
                Assert.That(model.Enabled, Is.True);
                Assert.That(model.Active, Is.True);
                Assert.That(model.State, Is.EqualTo(CoreTaskState.Ready));
                Assert.That(model.Command, Is.EqualTo(@"C:\test.exe"));
                Assert.That(model.LastRun, Is.EqualTo(lastRun));
            }));
        }


        [Test]
        public void Enable_TaskAlreadyEnabled_DoesNotCallEnable()
        {
            FakeScheduledTask task = new FakeScheduledTask { Name = TaskName, Enabled = true };
            _taskService.AddTestTask(task);
            ScheduledTaskModel model = new ScheduledTaskModel(task, _logger, _taskService);

            model.Enable();

            Assert.That(task.EnableCalls, Is.Zero);
        }

        [Test]
        public void Enable_TaskDisabled_CallsEnableAndUpdatesModelState()
        {
            FakeScheduledTask task = new FakeScheduledTask
            {
                Name = TaskName,
                Enabled = false,
                State = CoreTaskState.Disabled,
                IsActive = false,
            };
            _taskService.AddTestTask(task);
            ScheduledTaskModel model = new ScheduledTaskModel(task, _logger, _taskService);

            model.Enable();

            Assert.Multiple((System.Action)(() =>
            {
                Assert.That(task.EnableCalls, Is.EqualTo(1));
                Assert.That(model.Enabled, Is.True);
                Assert.That(model.State, Is.EqualTo(CoreTaskState.Ready));
            }));
        }


        [Test]
        public void Disable_TaskAlreadyDisabled_DoesNotCallDisable()
        {
            FakeScheduledTask task = new FakeScheduledTask { Name = TaskName, Enabled = false };
            _taskService.AddTestTask(task);
            ScheduledTaskModel model = new ScheduledTaskModel(task, _logger, _taskService);

            model.Disable();

            Assert.That(task.DisableCalls, Is.Zero);
        }

        [Test]
        public void Disable_TaskEnabled_CallsDisableAndUpdatesModelState()
        {
            FakeScheduledTask task = new FakeScheduledTask
            {
                Name = TaskName,
                Enabled = true,
                State = CoreTaskState.Ready,
                IsActive = true,
            };
            _taskService.AddTestTask(task);
            ScheduledTaskModel model = new ScheduledTaskModel(task, _logger, _taskService);

            model.Disable();

            Assert.Multiple((System.Action)(() =>
            {
                Assert.That(task.DisableCalls, Is.EqualTo(1));
                Assert.That(model.Enabled, Is.False);
                Assert.That(model.State, Is.EqualTo(CoreTaskState.Disabled));
                Assert.That(model.Active, Is.False);
            }));
        }
    }
}
