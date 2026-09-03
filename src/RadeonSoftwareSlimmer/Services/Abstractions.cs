using System.IO.Abstractions;
using RadeonSoftwareSlimmer.Core.Interfaces;
using RadeonSoftwareSlimmer.Windows;

namespace RadeonSoftwareSlimmer.Services
{
    // Composition root for the WPF app: wires Core interfaces to their Windows implementations.
    public static class Abstractions
    {
        public static IAppLogger Logger { get; } = new AppLogger();
        public static IFileSystem FileSystem { get; } = new FileSystem();
        public static IRegistry Registry { get; } = new WindowsRegistry();
        public static IProcessHandler ProcessHandler { get; } = new WindowsProcessHandler(Logger);
        public static IProcessRunner ProcessRunner { get; } = new WindowsProcessRunner(FileSystem, Logger);
        public static IScheduledTaskService ScheduledTaskService { get; } = new WindowsScheduledTaskService();
        public static IServiceControllerFactory ServiceControllerFactory { get; } = new WindowsServiceControllerFactory();
    }
}
