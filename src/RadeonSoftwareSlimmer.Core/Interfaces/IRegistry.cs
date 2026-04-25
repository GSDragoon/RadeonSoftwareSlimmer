namespace RadeonSoftwareSlimmer.Core.Interfaces
{
    public interface IRegistry
    {
        IRegistryKey CurrentUser { get; }
        IRegistryKey LocalMachine { get; }
    }
}
