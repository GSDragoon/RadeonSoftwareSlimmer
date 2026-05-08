namespace RadeonSoftwareSlimmer.Core.Enums
{
    // https://learn.microsoft.com/en-us/dotnet/api/system.serviceprocess.servicetype
    [System.Flags]
    public enum ServiceType : int
    {
        KernelDriver = 1,
        FileSystemDriver = 2,
        Adapter = 4,
        RecognizerDriver = 8,
        Win32OwnProcess = 16,
        Win32ShareProcess = 32,
        InteractiveProcess = 256
    }
}
