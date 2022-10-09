using Microsoft.Win32;

namespace RadeonSoftwareSlimmer.Test.TestDoubles
{
    public class RegistryValue
    {
        public RegistryValue(object value)
        {
            Value = value;
            Kind = RegistryValueKind.None;
        }

        public RegistryValue(object value, RegistryValueKind valueKind)
        {
            Value = value;
            Kind = valueKind;
        }

        public object Value { get; }
        public RegistryValueKind Kind { get; }
    }
}
