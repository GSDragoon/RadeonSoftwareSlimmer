namespace RadeonSoftwareSlimmer.Core.Enums
{
    // https://learn.microsoft.com/en-us/dotnet/api/system.serviceprocess.servicestartmode
    public enum ServiceStartMode : int
    {
        Boot = 0,
        System = 1,
        Automatic = 2,
        Manual = 3,
        Disabled = 4
    }
}
