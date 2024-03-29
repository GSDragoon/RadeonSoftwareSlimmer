﻿using System.Diagnostics;
using NUnit.Framework;
using RadeonSoftwareSlimmer.Services;

namespace RadeonSoftwareSlimmer.Test.Services
{
    [NonParallelizable]
    public class ProcessHandlerTests
    {
        [Test]
        public void RunProcess_NoArguments_Returns1()
        {
            ProcessHandler processHandler = new ProcessHandler(@"C:\Windows\System32\ping.exe");
            Assert.That(processHandler.RunProcess(), Is.EqualTo(1));
        }
        [Test]
        public void RunProcess_WithArguments_Returns0()
        {
            ProcessHandler processHandler = new ProcessHandler(@"C:\Windows\System32\ping.exe");
            Assert.That(processHandler.RunProcess("localhost"), Is.EqualTo(0));
        }
        [Test]
        public void RunProcess_DoesNotExist_ReturnsNegative1()
        {
            ProcessHandler processHandler = new ProcessHandler(@"C:\Windows\System32\DOES_NOT_EXIST.exe");
            Assert.That(processHandler.RunProcess(), Is.EqualTo(-1));
        }
        [Test]
        public void RunProcess_FileNameOnly_ReturnsNegative1()
        {
            ProcessHandler processHandler = new ProcessHandler("sc.exe");
            Assert.That(processHandler.RunProcess(), Is.EqualTo(-1));
        }

        [Test]
        public void IsProcessRunning_ProcessRunning_ReturnsTrue()
        {
            ProcessHandler processHandler = new ProcessHandler(@"C:\Windows\System32\svchost.exe");
            Assert.That(processHandler.IsProcessRunning(), Is.True);
        }
        [Test]
        public void IsProcessRunning_ProcessNotRunning_ReturnsFalse()
        {
            ProcessHandler processHandler = new ProcessHandler(@"C:\Windows\System32\perfmon.exe");
            Assert.That(processHandler.IsProcessRunning(), Is.False);
        }
        [Test]
        public void IsProcessRunning_DoesNotExist_ReturnsFalse()
        {
            ProcessHandler processHandler = new ProcessHandler(@"C:\Windows\System32\DOES_NOT_EXIST.exe");
            Assert.That(processHandler.IsProcessRunning(), Is.False);
        }
        [Test]
        public void IsProcessRunning_FileNameOnly_ReturnsFalse()
        {
            ProcessHandler processHandler = new ProcessHandler("svchost.exe");
            Assert.That(processHandler.IsProcessRunning(), Is.False);
        }

        [Test]
        public void WaitForProcessToEnd_NotRunning()
        {
            ProcessHandler processHandler = new ProcessHandler(@"C:\Windows\System32\tracert.exe");
            processHandler.WaitForProcessToEnd(5);
            Assert.That(processHandler.IsProcessRunning(), Is.False);
        }
        [Test]
        public void WaitForProcessToEnd_EndsInTime()
        {
            ProcessHandler processHandler = new ProcessHandler(@"C:\Windows\System32\ping.exe");
            using (Process process = new Process())
            {
                process.StartInfo.FileName = @"C:\Windows\System32\ping.exe";
                process.StartInfo.Arguments = "localhost";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
            }

            processHandler.WaitForProcessToEnd(5);
            Assert.That(processHandler.IsProcessRunning(), Is.False);
        }
        [Test]
        public void WaitForProcessToEnd_ForcedKilled()
        {
            ProcessHandler processHandler = new ProcessHandler(@"C:\Windows\System32\ping.exe");
            using (Process process = new Process())
            {
                process.StartInfo.FileName = @"C:\Windows\System32\ping.exe";
                process.StartInfo.Arguments = "localhost -n 10";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
            }

            processHandler.WaitForProcessToEnd(5);
            Assert.That(processHandler.IsProcessRunning(), Is.False);
        }
    }
}
