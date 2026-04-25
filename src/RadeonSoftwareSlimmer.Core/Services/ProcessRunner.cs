using System.Diagnostics;
using System.IO.Abstractions;
using RadeonSoftwareSlimmer.Core.Interfaces;

namespace RadeonSoftwareSlimmer.Core.Services
{
    public class ProcessRunner : IProcessRunner
    {
        private readonly IAppLogger _logger;

        private readonly IFileSystem _fileSystem;


        public ProcessRunner(IFileSystem fileSystem, IAppLogger logger)
        {
            _logger = logger;
            _fileSystem = fileSystem;
        }


        public int RunProcess(string fileName, string arguments)
        {
            IFileInfo file = _fileSystem.FileInfo.New(fileName);
            if (!file.Exists)
            {
                _logger.Debug($"{file.FullName} does not exist or user does not have access");
                return -1;
            }

            using (Process process = new Process())
            {
                process.StartInfo.FileName = file.FullName;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;

                _logger.Debug($"Running {process.StartInfo.FileName} {process.StartInfo.Arguments}");
                process.Start();
                process.WaitForExit();
                _logger.Debug($"Process finished with ExitCode: {process.ExitCode}");
                _logger.Debug($"StandardOutput: {process.StandardOutput.ReadToEnd()}");
                _logger.Debug($"StandardError: {process.StandardError.ReadToEnd()}");

                return process.ExitCode;
            }
        }
    }
}
