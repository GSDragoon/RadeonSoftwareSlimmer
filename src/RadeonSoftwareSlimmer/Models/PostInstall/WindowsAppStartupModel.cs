using System;
using System.ComponentModel;
using Microsoft.Win32;
using RadeonSoftwareSlimmer.Intefaces;

namespace RadeonSoftwareSlimmer.Models.PostInstall
{
    public class WindowsAppStartupModel : INotifyPropertyChanged
    {
        private static readonly DateTime UNIX_EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static readonly string RSX_LAUNCHER_REG_PATH = @"Software\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\AppModel\SystemAppData\AdvancedMicroDevicesInc-RSXCM_fhmx3h6dzfmvj\launcherrsxruntimeTask";
        private static readonly string RSX_LAUNCHER_REG_STATUS_NAME = "State";
        private static readonly string RSX_LAUNCHER_REG_LASTDISABLEDTIME_NAME = "LastDisabledTime";
        private static readonly object STATE_ENABLED_VALUE = 2;
        private static readonly object STATE_DISABLED_VALUE = 1;
        //if we set this, can we bypass the reboot requirement after a new install to disalbe this right away?
        //otherwise the first reboot after a new install enables this startup task
        private static readonly string RSX_LAUNCHER_REG_STARTUPONCE_NAME = "UserEnabledStartupOnce";
        private static readonly object STATE_STARTUPONCE_YES = 1;

        private readonly IRegistry _registry;
        private bool _exists;
        private bool _enabled;


        public WindowsAppStartupModel(IRegistry registry)
        {
            Exists = false;
            Enabled = false;
            _registry = registry;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        public bool Exists
        {
            get { return _exists; }
            private set
            {
                _exists = value;
                OnPropertyChanged(nameof(Exists));
            }
        }
        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                OnPropertyChanged(nameof(Enabled));
            }
        }


        public void LoadOrRefresh()
        {
            using (IRegistryKey launcherKey = _registry.CurrentUser.OpenSubKey(RSX_LAUNCHER_REG_PATH, false))
            {
                if (launcherKey == null)
                    return;

                object state = launcherKey.GetValue(RSX_LAUNCHER_REG_STATUS_NAME);

                if (state != null)
                {
                    Exists = true;

                    if (state == STATE_ENABLED_VALUE)
                        Enabled = true;
                    else if (state == STATE_DISABLED_VALUE)
                        Enabled = false;
                }
            }
        }

        public void Enable()
        {
            using (IRegistryKey launcherKey = _registry.CurrentUser.OpenSubKey(RSX_LAUNCHER_REG_PATH, true))
            {
                if (launcherKey == null)
                    return;

                launcherKey.SetValue(RSX_LAUNCHER_REG_STATUS_NAME, STATE_ENABLED_VALUE, RegistryValueKind.DWord);
                Enabled = true;
            }
        }

        //make this take in a time to make it testable
        public void Disable()
        {
            using (IRegistryKey launcherKey = _registry.CurrentUser.OpenSubKey(RSX_LAUNCHER_REG_PATH, true))
            {
                if (launcherKey == null)
                    return;

                launcherKey.SetValue(RSX_LAUNCHER_REG_STATUS_NAME, STATE_DISABLED_VALUE, RegistryValueKind.DWord);
                launcherKey.SetValue(RSX_LAUNCHER_REG_LASTDISABLEDTIME_NAME, SecondsFromEpochTime(DateTime.UtcNow), RegistryValueKind.QWord);
                launcherKey.SetValue(RSX_LAUNCHER_REG_STARTUPONCE_NAME, STATE_STARTUPONCE_YES, RegistryValueKind.DWord);
                Enabled = false;
            }
        }

        public void ApplyChanges()
        {
            if (_exists)
            {
                if (_enabled)
                    Enable();
                else
                    Disable();
            }
        }


        private static long SecondsFromEpochTime(DateTime fromDate)
        {
            return Convert.ToInt64((fromDate - UNIX_EPOCH).TotalSeconds);
        }
    }
}
