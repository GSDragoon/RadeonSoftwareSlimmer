using Microsoft.Win32;
using RadeonSoftwareSlimmer.Intefaces;

namespace RadeonSoftwareSlimmer.Services
{
    public class WindowsRegistry : IRegistry
    {
        private static readonly WindowsRegistryKey _currentUser = new WindowsRegistryKey(Registry.CurrentUser);
        private static readonly WindowsRegistryKey _localMachine = new WindowsRegistryKey(Registry.LocalMachine);

        public IRegistryKey CurrentUser => _currentUser;
        public IRegistryKey LocalMachine => _localMachine;
    }
}
