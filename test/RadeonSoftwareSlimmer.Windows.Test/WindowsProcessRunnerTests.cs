using System.IO.Abstractions;
using NUnit.Framework;
using RadeonSoftwareSlimmer.Core.Test.TestDoubles;

namespace RadeonSoftwareSlimmer.Windows.Test
{
    [NonParallelizable]
    public class WindowsProcessRunnerTests
    {
        private FakeAppLogger _logger;
        private FileSystem _fileSystem;
        private WindowsProcessRunner _runner;

        [SetUp]
        public void Setup()
        {
            _logger = new FakeAppLogger();
            _fileSystem = new FileSystem();
            _runner = new WindowsProcessRunner(_fileSystem, _logger);
        }

        [Test]
        public void RunProcess_NoArguments_Returns1()
        {
            Assert.That(_runner.RunProcess(@"C:\Windows\System32\ping.exe", string.Empty), Is.EqualTo(1));
        }

        [Test]
        public void RunProcess_WithArguments_Returns0()
        {
            Assert.That(_runner.RunProcess(@"C:\Windows\System32\ping.exe", "localhost"), Is.EqualTo(0));
        }

        [Test]
        public void RunProcess_DoesNotExist_ReturnsNegative1()
        {
            Assert.That(_runner.RunProcess(@"C:\Windows\System32\DOES_NOT_EXIST.exe", string.Empty), Is.EqualTo(-1));
        }

        [Test]
        public void RunProcess_FileNameOnly_ReturnsNegative1()
        {
            Assert.That(_runner.RunProcess("sc.exe", string.Empty), Is.EqualTo(-1));
        }
    }
}
