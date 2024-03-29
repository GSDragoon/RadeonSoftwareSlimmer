﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using RadeonSoftwareSlimmer.ViewModels;

namespace RadeonSoftwareSlimmer.Models.PreInstall
{
    //So many problems with these files... This is why we can't have nice things.
    public class ScheduledTaskXmlListModel : INotifyPropertyChanged
    {
        private readonly IFileSystem _fileSystem;
        private IEnumerable<ScheduledTaskXmlModel> _scheduledTasks;


        public ScheduledTaskXmlListModel(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        public IEnumerable<ScheduledTaskXmlModel> ScheduledTasks
        {
            get { return _scheduledTasks; }
            set
            {
                _scheduledTasks = value;
                OnPropertyChanged(nameof(ScheduledTasks));
            }
        }


        public void LoadOrRefresh(IDirectoryInfo installDirectory)
        {
            ScheduledTasks = new List<ScheduledTaskXmlModel>(GetInstallerScheduledTasks(installDirectory));
        }

        public void SetScheduledTaskStatusAndUnhide(ScheduledTaskXmlModel scheduledTaskToUpdate)
        {
            if (scheduledTaskToUpdate == null)
                throw new ArgumentNullException(nameof(scheduledTaskToUpdate));

            if (!TryGetScheduledTaskXDocument(scheduledTaskToUpdate.GetFile(), FileAccess.ReadWrite, out XDocument xDoc))
                throw new IOException();

            XNamespace xNs = xDoc.Root.GetDefaultNamespace();
            xDoc.Root.Element(xNs + "Settings").Element(xNs + "Enabled").Value = XmlConvert.ToString(scheduledTaskToUpdate.Enabled);
            xDoc.Root.Element(xNs + "Settings").Element(xNs + "Hidden").Value = XmlConvert.ToString(false);

            using (Stream stream = scheduledTaskToUpdate.GetFile().Open(FileMode.Create, FileAccess.Write))
            {
                xDoc.Save(stream, SaveOptions.None);
            }
        }

        public void RestoreToDefault()
        {
            if (_scheduledTasks != null)
            {
                StaticViewModel.AddDebugMessage("Restoring scheduled tasks to enabled");

                foreach (ScheduledTaskXmlModel scheduledTask in _scheduledTasks)
                {
                    // Assume they are all enabled by default
                    scheduledTask.Enabled = true;
                    SetScheduledTaskStatusAndUnhide(scheduledTask);
                }
            }
        }


        private IEnumerable<ScheduledTaskXmlModel> GetInstallerScheduledTasks(IDirectoryInfo installDirectory)
        {
            IDirectoryInfo directoryInfo = _fileSystem.DirectoryInfo.New(_fileSystem.Path.Combine(installDirectory.FullName, "Config"));
            foreach (IFileInfo file in directoryInfo.EnumerateFiles("*.xml", SearchOption.TopDirectoryOnly).Where(f => !f.Name.StartsWith("Monet", StringComparison.CurrentCulture)))
            {
                if (TryGetScheduledTaskXDocument(file, FileAccess.Read, out XDocument xDoc))
                {
                    ScheduledTaskXmlModel scheduledTask = new ScheduledTaskXmlModel(file);
                    XNamespace xNs = xDoc.Root.GetDefaultNamespace();

                    //Not every file has a URI specified :(
                    XElement uri = xDoc.Root.Element(xNs + "RegistrationInfo").Element(xNs + "URI");
                    if (uri != null)
                        scheduledTask.Uri = uri.Value;

                    scheduledTask.Description = xDoc.Root.Element(xNs + "RegistrationInfo").Element(xNs + "Description").Value;
                    scheduledTask.Enabled = bool.Parse(xDoc.Root.Element(xNs + "Settings").Element(xNs + "Enabled").Value);

                    string command = xDoc.Root.Element(xNs + "Actions").Element(xNs + "Exec").Element(xNs + "Command").Value;
                    string arguments = xDoc.Root.Element(xNs + "Actions").Element(xNs + "Exec").Element(xNs + "Arguments").Value;
                    scheduledTask.Command = $"{command} {arguments}";

                    StaticViewModel.AddDebugMessage($"Found scheduled task {scheduledTask.Description} in {file.FullName}");
                    yield return scheduledTask;
                }
            }
        }

        private bool TryGetScheduledTaskXDocument(IFileInfo file, FileAccess fileAccess, out XDocument xDocument)
        {
            xDocument = null;

            if (file == null || !file.Exists)
                return false;

            try
            {
                using (Stream stream = file.Open(FileMode.Open, fileAccess))
                {
                    xDocument = XDocument.Load(stream);
                }
            }
            catch (XmlException)
            {
                //Some files have incorrect encoding :(
                //This happens even with legit exported files from Windows itself
                //https://stackoverflow.com/questions/29915467/there-is-no-unicode-byte-order-mark-cannot-switch-to-unicode
                StaticViewModel.AddDebugMessage($"Wrong encoding for {file.FullName}");

                try
                {
                    xDocument = XDocument.Parse(_fileSystem.File.ReadAllText(file.FullName));
                }
                catch (XmlException ex)
                {
                    StaticViewModel.AddDebugMessage(ex);
                    return false;
                }
            }

            if (xDocument.Root.Name.LocalName.Equals("Task", StringComparison.CurrentCulture))
                return true;

            return false;
        }
    }
}
