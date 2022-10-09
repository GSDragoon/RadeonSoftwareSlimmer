using System;
using Microsoft.Win32;

namespace RadeonSoftwareSlimmer.Intefaces
{
    public interface IRegistryKey : IDisposable
    {
        IRegistryKey OpenSubKey(string name, bool writable);


        object GetValue(string name);

        object GetValue(string name, object defaultValue);


        void SetValue(string name, object value, RegistryValueKind valueKind);
    }
}
