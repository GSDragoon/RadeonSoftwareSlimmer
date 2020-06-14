using System.Diagnostics;
using RadeonSoftwareSlimmer.ViewModels;

namespace RadeonSoftwareSlimmer.Services
{
    public static class ProcessExecutor
    {
        public static int RunProcess(string fileName, string arguments)
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = fileName;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;

                StaticViewModel.AddDebugMessage($"Running {process.StartInfo.FileName} {process.StartInfo.Arguments}");
                process.Start();
                process.WaitForExit();
                StaticViewModel.AddDebugMessage($"Process finished with ExitCode: {process.ExitCode}");
                StaticViewModel.AddDebugMessage($"StandardOutput: {process.StandardOutput.ReadToEnd()}");
                StaticViewModel.AddDebugMessage($"StandardError: {process.StandardError.ReadToEnd()}");

                return process.ExitCode;
            }
        }
    }
}
