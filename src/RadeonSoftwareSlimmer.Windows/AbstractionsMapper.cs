using System;
using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;
using System.ServiceProcess;
using RadeonSoftwareSlimmer.Core.Enums;
using RadeonSoftwareSlimmer.Core.Interfaces;

namespace RadeonSoftwareSlimmer.Windows
{
    public static class AbstractionsMapper
    {
        public static CoreTaskState ToCoreTaskState(this TaskState state) => (CoreTaskState)(int)state;

        public static RegistryValueKind ToWindowsRegistryValueKind(this CoreRegistryValueKind kind) => (RegistryValueKind)(int)kind;

        public static WindowsScheduledTask ToWindowsScheduledTask(this Task task) => new WindowsScheduledTask(task);

        public static Predicate<Task> ToWindowsTaskPredicate(this Predicate<IScheduledTask> filter) => task => filter(task.ToWindowsScheduledTask());

        public static CoreServiceStartMode ToCoreServiceStartMode(this ServiceStartMode mode) => (CoreServiceStartMode)(int)mode;

        public static CoreServiceType ToCoreServiceType(this ServiceType type) => (CoreServiceType)(int)type;

        public static CoreServiceControllerStatus ToCoreServiceControllerStatus(this ServiceControllerStatus status) => (CoreServiceControllerStatus)(int)status;

        public static ServiceControllerStatus ToWindowsServiceControllerStatus(this CoreServiceControllerStatus status) => (ServiceControllerStatus)(int)status;
    }
}
