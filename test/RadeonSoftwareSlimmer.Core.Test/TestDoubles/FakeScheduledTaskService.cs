using System;
using System.Collections.Generic;
using System.Linq;
using RadeonSoftwareSlimmer.Core.Interfaces;

namespace RadeonSoftwareSlimmer.Core.Test.TestDoubles
{
    public class FakeScheduledTaskService : IScheduledTaskService
    {
        public IList<IScheduledTask> Tasks { get; } = new List<IScheduledTask>();


        public FakeScheduledTaskService AddTestTask(IScheduledTask task)
        {
            Tasks.Add(task);
            return this;
        }


        public IScheduledTask GetTask(string taskName)
        {
            return Tasks.FirstOrDefault(t => string.Equals(t.Name, taskName, StringComparison.OrdinalIgnoreCase));
        }

        public IScheduledTask[] FindAllTasks(Predicate<IScheduledTask> filter, bool searchAllFolders = true)
        {
            return Tasks.Where(t => filter(t)).ToArray();
        }
    }
}
