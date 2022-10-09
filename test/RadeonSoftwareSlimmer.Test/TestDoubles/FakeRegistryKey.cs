﻿using System.Collections.Generic;
using Microsoft.Win32;
using RadeonSoftwareSlimmer.Intefaces;

namespace RadeonSoftwareSlimmer.Test.TestDoubles
{
    public class FakeRegistryKey : IRegistryKey
    {
        private bool disposedValue;

        public IDictionary<string, RegistryValue> Values { get; }
        public IDictionary<string, FakeRegistryKey> SubKeys { get; }


        public FakeRegistryKey()
        {
            Values = new Dictionary<string, RegistryValue>();
            SubKeys = new Dictionary<string, FakeRegistryKey>();
        }


        #region Test Setup
        public FakeRegistryKey AddTestValue(string name, object value)
        {
            return AddTestValue(name, value, RegistryValueKind.None);
        }
        public FakeRegistryKey AddTestValue(string name, object value, RegistryValueKind valueKind)
        {
            Values.Add(new KeyValuePair<string, RegistryValue>(name, new RegistryValue(value, valueKind)));
            return this;
        }

        public FakeRegistryKey AddTestSubKey(string name)
        {
            return AddTestSubKey(name, new FakeRegistryKey());
        }
        public FakeRegistryKey AddTestSubKey(string name, FakeRegistryKey registryKey)
        {
            SubKeys.Add(name, registryKey);
            return registryKey;
        }
        #endregion


        #region IRegistryKey
        public void SetValue(string name, object value, RegistryValueKind valueKind)
        {
            if (Values.ContainsKey(name))
                Values[name] = new RegistryValue(value, valueKind);
            else
                Values.Add(new KeyValuePair<string, RegistryValue>(name, new RegistryValue(value, valueKind)));
        }

        public object GetValue(string name)
        {
            return GetValue(name, null);
        }

        public object GetValue(string name, object defaultValue)
        {
            if (string.IsNullOrWhiteSpace(name))
                return defaultValue;
            if (Values.ContainsKey(name))
                return Values[name];
            else
                return defaultValue;
        }

        public IRegistryKey OpenSubKey(string name, bool writable)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;
            else if (SubKeys.ContainsKey(name))
                return SubKeys[name];
            else
                return null;
        }
        #endregion


        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects)
                }

                // free unmanaged resources (unmanaged objects) and override finalizer
                // set large fields to null
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            System.GC.SuppressFinalize(this);
        }
    }
}
