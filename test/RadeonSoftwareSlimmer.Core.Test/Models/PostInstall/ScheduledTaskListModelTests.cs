using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RadeonSoftwareSlimmer.Core.PostInstall;
using RadeonSoftwareSlimmer.Core.Test.TestDoubles;

namespace RadeonSoftwareSlimmer.Core.Test.Models.PostInstall
{
    public class ScheduledTaskListModelTests
    {
        private const string AmdAuthor = "Advanced Micro Devices";

        private FakeAppLogger _logger;
        private FakeScheduledTaskService _taskService;

        [SetUp]
        public void Setup()
        {
            _logger = new FakeAppLogger();
            _taskService = new FakeScheduledTaskService();
        }


        [Test]
        public void LoadOrRefresh_NoTasks_EmptyList()
        {
            ScheduledTaskListModel model = new ScheduledTaskListModel(_logger, _taskService);

            model.LoadOrRefresh();

            Assert.That(model.RadeonScheduledTasks, Is.Empty);
        }

        [Test]
        public void LoadOrRefresh_TaskAuthoredByAmd_IsIncluded()
        {
            _taskService.AddTestTask(new FakeScheduledTask { Name = "SomeAmdTask", Author = AmdAuthor });
            ScheduledTaskListModel model = new ScheduledTaskListModel(_logger, _taskService);

            model.LoadOrRefresh();

            Assert.That(model.RadeonScheduledTasks.Count(), Is.EqualTo(1));
        }

        [Test]
        public void LoadOrRefresh_AmdAuthorMatchIsCaseInsensitive()
        {
            _taskService.AddTestTask(new FakeScheduledTask { Name = "SomeAmdTask", Author = "advanced micro devices" });
            ScheduledTaskListModel model = new ScheduledTaskListModel(_logger, _taskService);

            model.LoadOrRefresh();

            Assert.That(model.RadeonScheduledTasks.Count(), Is.EqualTo(1));
        }

        [TestCase("DVRAnalytics")]
        [TestCase("StartAUEP")]
        [TestCase("StartCN")]
        [TestCase("StartCNBM")]
        [TestCase("StartDVR")]
        public void LoadOrRefresh_KnownRadeonTaskNameWithoutAmdAuthor_IsIncluded(string taskName)
        {
            _taskService.AddTestTask(new FakeScheduledTask { Name = taskName, Author = "Some Third Party" });
            ScheduledTaskListModel model = new ScheduledTaskListModel(_logger, _taskService);

            model.LoadOrRefresh();

            Assert.That(model.RadeonScheduledTasks.Count(), Is.EqualTo(1));
        }

        [Test]
        public void LoadOrRefresh_KnownRadeonTaskNameMatchIsCaseInsensitive()
        {
            _taskService.AddTestTask(new FakeScheduledTask { Name = "startcn", Author = "Some Third Party" });
            ScheduledTaskListModel model = new ScheduledTaskListModel(_logger, _taskService);

            model.LoadOrRefresh();

            Assert.That(model.RadeonScheduledTasks.Count(), Is.EqualTo(1));
        }

        [Test]
        public void LoadOrRefresh_UnrelatedAuthorAndName_IsExcluded()
        {
            _taskService.AddTestTask(new FakeScheduledTask { Name = "MicrosoftUpdate", Author = "Microsoft" });
            ScheduledTaskListModel model = new ScheduledTaskListModel(_logger, _taskService);

            model.LoadOrRefresh();

            Assert.That(model.RadeonScheduledTasks, Is.Empty);
        }

        [Test]
        public void LoadOrRefresh_EmptyAuthorAndName_IsExcluded()
        {
            _taskService.AddTestTask(new FakeScheduledTask { Name = string.Empty, Author = string.Empty });
            ScheduledTaskListModel model = new ScheduledTaskListModel(_logger, _taskService);

            model.LoadOrRefresh();

            Assert.That(model.RadeonScheduledTasks, Is.Empty);
        }

        [Test]
        public void LoadOrRefresh_MixOfTasks_ReturnsOnlyRadeonMatches()
        {
            _taskService.AddTestTask(new FakeScheduledTask { Name = "MicrosoftUpdate", Author = "Microsoft" });
            _taskService.AddTestTask(new FakeScheduledTask { Name = "AmdCoolStuff", Author = AmdAuthor });
            _taskService.AddTestTask(new FakeScheduledTask { Name = "StartDVR", Author = "Some Vendor" });
            _taskService.AddTestTask(new FakeScheduledTask { Name = "UnrelatedTask", Author = "Other" });
            ScheduledTaskListModel model = new ScheduledTaskListModel(_logger, _taskService);

            model.LoadOrRefresh();

            List<ScheduledTaskModel> results = model.RadeonScheduledTasks.ToList();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(results, Has.Count.EqualTo(2));
                Assert.That(results.Any(t => t.Name == "AmdCoolStuff"), Is.True);
                Assert.That(results.Any(t => t.Name == "StartDVR"), Is.True);
            }
        }


        [Test]
        public void ApplyChanges_EnabledFlagTrue_CallsTaskEnable()
        {
            FakeScheduledTask task = new FakeScheduledTask { Name = "AmdTask", Author = AmdAuthor, Enabled = false };
            _taskService.AddTestTask(task);
            ScheduledTaskListModel model = new ScheduledTaskListModel(_logger, _taskService);
            model.LoadOrRefresh();
            model.RadeonScheduledTasks.Single().Enabled = true;

            model.ApplyChanges();

            Assert.That(task.EnableCalls, Is.EqualTo(1));
        }

        [Test]
        public void ApplyChanges_EnabledFlagFalse_CallsTaskDisable()
        {
            FakeScheduledTask task = new FakeScheduledTask { Name = "AmdTask", Author = AmdAuthor, Enabled = true };
            _taskService.AddTestTask(task);
            ScheduledTaskListModel model = new ScheduledTaskListModel(_logger, _taskService);
            model.LoadOrRefresh();
            model.RadeonScheduledTasks.Single().Enabled = false;

            model.ApplyChanges();

            Assert.That(task.DisableCalls, Is.EqualTo(1));
        }
    }
}
