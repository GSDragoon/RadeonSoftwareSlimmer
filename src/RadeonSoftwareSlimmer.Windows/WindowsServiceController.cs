using System;
using System.ServiceProcess;
using RadeonSoftwareSlimmer.Core.Enums;
using RadeonSoftwareSlimmer.Core.Interfaces;

namespace RadeonSoftwareSlimmer.Windows
{
    public class WindowsServiceController : IServiceController
    {
        private readonly ServiceController _serviceController;
        private bool _disposed;


        public WindowsServiceController(string serviceName)
        {
            if (string.IsNullOrWhiteSpace(serviceName))
                throw new ArgumentException("Service name cannot be null or whitespace.", nameof(serviceName));

            _serviceController = new ServiceController(serviceName);

            try
            {
                // ServiceController is lazily initialized; reading DisplayName forces a lookup.
                DisplayName = _serviceController.DisplayName;
                Exists = true;
            }
            catch (InvalidOperationException)
            {
                Exists = false;
            }
        }


        public string ServiceName => _serviceController.ServiceName;
        public bool Exists { get; }
        public string DisplayName { get; }
        public CoreServiceStartMode StartType => _serviceController.StartType.ToCoreServiceStartMode();
        public CoreServiceType ServiceType => _serviceController.ServiceType.ToCoreServiceType();
        public CoreServiceControllerStatus Status => _serviceController.Status.ToCoreServiceControllerStatus();


        public void Start() => _serviceController.Start();

        public void Stop() => _serviceController.Stop();

        public void Refresh() => _serviceController.Refresh();

        public void WaitForStatus(CoreServiceControllerStatus desiredStatus, TimeSpan timeout)
        {
            _serviceController.WaitForStatus(desiredStatus.ToWindowsServiceControllerStatus(), timeout);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
                _serviceController?.Dispose();

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
