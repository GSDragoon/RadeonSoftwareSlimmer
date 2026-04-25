using System;
using RadeonSoftwareSlimmer.Core.Enums;

namespace RadeonSoftwareSlimmer.Core.Interfaces
{
    public interface IRegistryKey : IDisposable
    {
        string Name { get; }


        IRegistryKey OpenSubKey(string name, bool writable);

        string[] GetSubKeyNames();


        object GetValue(string name);

        object GetValue(string name, object defaultValue);

        void SetValue(string name, object value, RegistryValueKind valueKind);
    }
}
