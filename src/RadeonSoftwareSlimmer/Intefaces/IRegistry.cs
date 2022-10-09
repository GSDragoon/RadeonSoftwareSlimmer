namespace RadeonSoftwareSlimmer.Intefaces
{
    public interface IRegistry
    {
        IRegistryKey CurrentUser { get; }
        IRegistryKey LocalMachine { get; }
    }
}
