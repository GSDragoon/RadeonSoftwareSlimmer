using System.Diagnostics;
using NUnit.Framework;
using RadeonSoftwareSlimmer.Core.Test.TestDoubles;

namespace RadeonSoftwareSlimmer.Windows.Test
{
    [NonParallelizable]
    public class WindowsProcessHandlerTests
    {
        private FakeAppLogger _logger;
        private WindowsProcessHandler _handler;

        [SetUp]
        public void Setup()
        {
            _logger = new FakeAppLogger();
            _handler = new WindowsProcessHandler(_logger);
        }


        [Test]
        public void IsProcessRunning_ProcessRunning_ReturnsTrue()
        {
            Assert.That(_handler.IsProcessRunning("svchost"), Is.True);
        }

        [Test]
        public void IsProcessRunning_ProcessNotRunning_ReturnsFalse()
        {
            Assert.That(_handler.IsProcessRunning("perfmon"), Is.False);
        }

        [Test]
        public void IsProcessRunning_DoesNotExist_ReturnsFalse()
        {
            Assert.That(_handler.IsProcessRunning("DOES_NOT_EXIST"), Is.False);
        }

        [Test]
        public void IsProcessRunning_FileNameWithExtension_ReturnsFalse()
        {
            // Process.GetProcessesByName expects the name without the extension.
            Assert.That(_handler.IsProcessRunning("svchost.exe"), Is.False);
        }


        [Test]
        public void WaitForProcessToEnd_NotRunning()
        {
            _handler.WaitForProcessToEnd("tracert", 5);
            Assert.That(_handler.IsProcessRunning("tracert"), Is.False);
        }

        [Test]
        public void WaitForProcessToEnd_EndsInTime()
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = @"C:\Windows\System32\ping.exe";
                process.StartInfo.Arguments = "localhost";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
            }

            _handler.WaitForProcessToEnd("ping", 5);
            Assert.That(_handler.IsProcessRunning("ping"), Is.False);
        }

        [Test]
        public void WaitForProcessToEnd_ForcedKilled()
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = @"C:\Windows\System32\ping.exe";
                process.StartInfo.Arguments = "localhost -n 10";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
            }

            _handler.WaitForProcessToEnd("ping", 5);
            Assert.That(_handler.IsProcessRunning("ping"), Is.False);
        }
    }
}
