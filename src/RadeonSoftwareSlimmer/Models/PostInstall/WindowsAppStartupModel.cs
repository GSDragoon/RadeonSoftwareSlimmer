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
            Exists = false;

            using (IRegistryKey launcherKey = _registry.CurrentUser.OpenSubKey(RSX_LAUNCHER_REG_PATH, false))
            {
                if (launcherKey == null)
                    return;

                Exists = true;
                object state = launcherKey.GetValue(RSX_LAUNCHER_REG_STATUS_NAME);


                if (state != null && state.Equals(STATE_ENABLED_VALUE))
                    Enabled = true;
                if (state == null || state.Equals(STATE_DISABLED_VALUE))
                {
                    // Any other value besides enabled (2) is treated as disabled (1)
                    // If the flag that the user has enabled statup once isn't set (0 or missing), then it will be re-enabled next logon/reboot
                    object startupOnce = launcherKey.GetValue(RSX_LAUNCHER_REG_STARTUPONCE_NAME);

                    if (startupOnce != null && startupOnce.Equals(STATE_STARTUPONCE_YES))
                        Enabled = false;
                    else
                        Enabled = true;
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
