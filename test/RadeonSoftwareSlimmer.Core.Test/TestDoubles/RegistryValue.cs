using RadeonSoftwareSlimmer.Core.Enums;

namespace RadeonSoftwareSlimmer.Core.Test.TestDoubles
{
    public class RegistryValue
    {
        public RegistryValue(object value)
        {
            Value = value;
            Kind = CoreRegistryValueKind.None;
        }

        public RegistryValue(object value, CoreRegistryValueKind valueKind)
        {
            Value = value;
            Kind = valueKind;
        }

        public object Value { get; }
        public CoreRegistryValueKind Kind { get; }
    }
}
