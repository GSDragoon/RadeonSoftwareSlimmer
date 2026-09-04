using System;
using System.Linq;
using System.ServiceProcess;
using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;
using NUnit.Framework;
using RadeonSoftwareSlimmer.Core.Enums;
using RadeonSoftwareSlimmer.Core.Interfaces;

namespace RadeonSoftwareSlimmer.Windows.Test
{
    public class AbstractionsMapperTests
    {
        [TestCase(TaskState.Unknown, CoreTaskState.Unknown)]
        [TestCase(TaskState.Disabled, CoreTaskState.Disabled)]
        [TestCase(TaskState.Queued, CoreTaskState.Queued)]
        [TestCase(TaskState.Ready, CoreTaskState.Ready)]
        [TestCase(TaskState.Running, CoreTaskState.Running)]
        public void ToCoreTaskState_MapsToMatchingCoreValue(TaskState input, CoreTaskState expected)
        {
            Assert.That(input.ToCoreTaskState(), Is.EqualTo(expected));
        }


        [TestCase(CoreRegistryValueKind.None, RegistryValueKind.None)]
        [TestCase(CoreRegistryValueKind.Unknown, RegistryValueKind.Unknown)]
        [TestCase(CoreRegistryValueKind.String, RegistryValueKind.String)]
        [TestCase(CoreRegistryValueKind.ExpandString, RegistryValueKind.ExpandString)]
        [TestCase(CoreRegistryValueKind.Binary, RegistryValueKind.Binary)]
        [TestCase(CoreRegistryValueKind.DWord, RegistryValueKind.DWord)]
        [TestCase(CoreRegistryValueKind.MultiString, RegistryValueKind.MultiString)]
        [TestCase(CoreRegistryValueKind.QWord, RegistryValueKind.QWord)]
        public void ToWindowsRegistryValueKind_MapsToMatchingWindowsValue(CoreRegistryValueKind input, RegistryValueKind expected)
        {
            Assert.That(input.ToWindowsRegistryValueKind(), Is.EqualTo(expected));
        }


        [TestCase(ServiceStartMode.Boot, CoreServiceStartMode.Boot)]
        [TestCase(ServiceStartMode.System, CoreServiceStartMode.System)]
        [TestCase(ServiceStartMode.Automatic, CoreServiceStartMode.Automatic)]
        [TestCase(ServiceStartMode.Manual, CoreServiceStartMode.Manual)]
        [TestCase(ServiceStartMode.Disabled, CoreServiceStartMode.Disabled)]
        public void ToCoreServiceStartMode_MapsToMatchingCoreValue(ServiceStartMode input, CoreServiceStartMode expected)
        {
            Assert.That(input.ToCoreServiceStartMode(), Is.EqualTo(expected));
        }


        [TestCase(ServiceType.KernelDriver, CoreServiceType.KernelDriver)]
        [TestCase(ServiceType.FileSystemDriver, CoreServiceType.FileSystemDriver)]
        [TestCase(ServiceType.Adapter, CoreServiceType.Adapter)]
        [TestCase(ServiceType.RecognizerDriver, CoreServiceType.RecognizerDriver)]
        [TestCase(ServiceType.Win32OwnProcess, CoreServiceType.Win32OwnProcess)]
        [TestCase(ServiceType.Win32ShareProcess, CoreServiceType.Win32ShareProcess)]
        [TestCase(ServiceType.InteractiveProcess, CoreServiceType.InteractiveProcess)]
        public void ToCoreServiceType_MapsToMatchingCoreValue(ServiceType input, CoreServiceType expected)
        {
            Assert.That(input.ToCoreServiceType(), Is.EqualTo(expected));
        }

        [Test]
        public void ToCoreServiceType_PreservesFlagCombinations()
        {
            ServiceType combined = ServiceType.Win32OwnProcess | ServiceType.InteractiveProcess;

            CoreServiceType result = combined.ToCoreServiceType();

            Assert.That(result, Is.EqualTo(CoreServiceType.Win32OwnProcess | CoreServiceType.InteractiveProcess));
        }


        [TestCase(ServiceControllerStatus.Stopped, CoreServiceControllerStatus.Stopped)]
        [TestCase(ServiceControllerStatus.StartPending, CoreServiceControllerStatus.StartPending)]
        [TestCase(ServiceControllerStatus.StopPending, CoreServiceControllerStatus.StopPending)]
        [TestCase(ServiceControllerStatus.Running, CoreServiceControllerStatus.Running)]
        [TestCase(ServiceControllerStatus.ContinuePending, CoreServiceControllerStatus.ContinuePending)]
        [TestCase(ServiceControllerStatus.PausePending, CoreServiceControllerStatus.PausePending)]
        [TestCase(ServiceControllerStatus.Paused, CoreServiceControllerStatus.Paused)]
        public void ToCoreServiceControllerStatus_MapsToMatchingCoreValue(ServiceControllerStatus input, CoreServiceControllerStatus expected)
        {
            Assert.That(input.ToCoreServiceControllerStatus(), Is.EqualTo(expected));
        }


        [TestCase(CoreServiceControllerStatus.Stopped, ServiceControllerStatus.Stopped)]
        [TestCase(CoreServiceControllerStatus.StartPending, ServiceControllerStatus.StartPending)]
        [TestCase(CoreServiceControllerStatus.StopPending, ServiceControllerStatus.StopPending)]
        [TestCase(CoreServiceControllerStatus.Running, ServiceControllerStatus.Running)]
        [TestCase(CoreServiceControllerStatus.ContinuePending, ServiceControllerStatus.ContinuePending)]
        [TestCase(CoreServiceControllerStatus.PausePending, ServiceControllerStatus.PausePending)]
        [TestCase(CoreServiceControllerStatus.Paused, ServiceControllerStatus.Paused)]
        public void ToWindowsServiceControllerStatus_MapsToMatchingWindowsValue(CoreServiceControllerStatus input, ServiceControllerStatus expected)
        {
            Assert.That(input.ToWindowsServiceControllerStatus(), Is.EqualTo(expected));
        }


        [Test]
        public void ToWindowsTaskPredicate_RoutesThroughWindowsScheduledTaskWrapper()
        {
            using (TaskService taskService = new TaskService())
            {
                Task existingTask = taskService.RootFolder.AllTasks.FirstOrDefault();
                if (existingTask == null)
                    Assert.Ignore("No scheduled tasks available on this machine to exercise the predicate wrapper.");

                bool sawInvocation = false;
                Predicate<IScheduledTask> corePredicate = _ => { sawInvocation = true; return true; };

                Predicate<Task> windowsPredicate = corePredicate.ToWindowsTaskPredicate();
                bool result = windowsPredicate(existingTask);

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(sawInvocation, Is.True);
                    Assert.That(result, Is.True);
                }
            }
        }
    }
}
