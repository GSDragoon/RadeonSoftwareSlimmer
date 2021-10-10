using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using NUnit.Framework;
using RadeonSoftwareSlimmer.Models.PreInstall;

namespace RadeonSoftwareSlimmer.Test.Models.PreInstall
{
    public class ScheduledTaskXmlModelTest
    {
        [Test]
        public void GetFile_ReturnsFile()
        {
            string filePath = @"C:\Some\path\To\A\file.xml";
            MockFileSystem fileSystem = new MockFileSystem();
            fileSystem.AddFile(filePath, new MockFileData(string.Empty));
            IFileInfo originalFile = fileSystem.FileInfo.FromFileName(filePath);
            ScheduledTaskXmlModel scheduledTask = new ScheduledTaskXmlModel(originalFile);

            IFileInfo returnFile = scheduledTask.GetFile();

            Assert.That(returnFile, Is.Not.Null);
            Assert.That(returnFile, Is.EqualTo(originalFile));
        }
    }
}
