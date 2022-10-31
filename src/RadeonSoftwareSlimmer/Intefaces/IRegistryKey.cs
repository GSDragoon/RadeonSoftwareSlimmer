using System;
using Microsoft.Win32;

namespace RadeonSoftwareSlimmer.Intefaces
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
