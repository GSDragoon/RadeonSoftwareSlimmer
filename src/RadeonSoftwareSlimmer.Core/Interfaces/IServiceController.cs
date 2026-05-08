using RadeonSoftwareSlimmer.Core.Enums;
using System;


namespace RadeonSoftwareSlimmer.Core.Interfaces
{
    // https://dahall.github.io/TaskScheduler/html/T_Microsoft_Win32_TaskScheduler_TaskService.htm
    public interface IServiceController
    {
        string ServiceName { get; }
        bool Exists { get; }  // InvalidOperationException = does not exist
        string DisplayName { get; }
        ServiceStartMode StartType { get; }
        ServiceType ServiceType { get; }
        ServiceControllerStatus Status { get; }

        void Load(string serviceName);

        void Start();
        void Stop();
        void Refresh();
        void WaitForStatus(ServiceControllerStatus desiredStatus, TimeSpan timeout);
    }
}
