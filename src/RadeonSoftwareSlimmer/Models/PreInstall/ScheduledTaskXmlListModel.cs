using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using RadeonSoftwareSlimmer.ViewModels;

namespace RadeonSoftwareSlimmer.Models.PreInstall
{
    //So many problems with these files... This is why we can't have nice things.
    public class ScheduledTaskXmlListModel : INotifyPropertyChanged
    {
        private IEnumerable<ScheduledTaskXmlModel> _scheduledTasks;


        public ScheduledTaskXmlListModel() { }


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


        public void LoadOrRefresh(DirectoryInfo installDirectory)
        {
            ScheduledTasks = new List<ScheduledTaskXmlModel>(GetInstallerScheduledTasks(installDirectory));
        }

        public static void UnhideAndDisableScheduledTask(ScheduledTaskXmlModel scheduledTaskToRemove)
        {
            if (scheduledTaskToRemove == null)
                throw new NullReferenceException();

            if (!TryGetScheduledTaskXDocument(scheduledTaskToRemove.File, out XDocument xDoc))
                throw new IOException();

            XNamespace xNs = xDoc.Root.GetDefaultNamespace();

            xDoc.Root.Element(xNs + "Settings").Element(xNs + "Enabled").Value = bool.FalseString;
            xDoc.Root.Element(xNs + "Settings").Element(xNs + "Hidden").Value = bool.FalseString;

            xDoc.Save(scheduledTaskToRemove.File.FullName, SaveOptions.None);
        }


        private static IEnumerable<ScheduledTaskXmlModel> GetInstallerScheduledTasks(DirectoryInfo installDirectory)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(installDirectory + "\\Config");
            foreach (FileInfo file in directoryInfo.EnumerateFiles("*.xml", SearchOption.TopDirectoryOnly).Where(f => !f.Name.StartsWith("Monet", StringComparison.CurrentCulture)))
            {
                if (TryGetScheduledTaskXDocument(file, out XDocument xDoc))
                {
                    ScheduledTaskXmlModel scheduledTask = new ScheduledTaskXmlModel();
                    XNamespace xNs = xDoc.Root.GetDefaultNamespace();

                    //Not every file has a URI specified :(
                    XElement uri = xDoc.Root.Element(xNs + "RegistrationInfo").Element(xNs + "URI");
                    if (uri != null)
                        scheduledTask.Uri = uri.Value;

                    scheduledTask.Description = xDoc.Root.Element(xNs + "RegistrationInfo").Element(xNs + "Description").Value;
                    scheduledTask.Enabled = bool.Parse(xDoc.Root.Element(xNs + "Settings").Element(xNs + "Enabled").Value);

                    string command = xDoc.Root.Element(xNs + "Actions").Element(xNs + "Exec").Element(xNs + "Command").Value;
                    string arguments = xDoc.Root.Element(xNs + "Actions").Element(xNs + "Exec").Element(xNs + "Arguments").Value;
                    scheduledTask.Command = command + arguments;

                    scheduledTask.File = file;

                    StaticViewModel.AddDebugMessage($"Found scheduled task {uri} in {file.FullName}");
                    yield return scheduledTask;
                }
            }
        }

        private static bool TryGetScheduledTaskXDocument(FileInfo file, out XDocument xDocument)
        {
            xDocument = null;

            if (file == null || !file.Exists)
                return false;

            try
            {
                xDocument = XDocument.Load(file.FullName);
            }
            catch (XmlException)
            {
                //Some files have incorrect encoding :(
                //https://stackoverflow.com/questions/29915467/there-is-no-unicode-byte-order-mark-cannot-switch-to-unicode
                StaticViewModel.AddDebugMessage($"Wrong encoding for {file.FullName}");
                xDocument = XDocument.Parse(File.ReadAllText(file.FullName));
            }

            if (!xDocument.Root.Name.LocalName.Equals("Task", StringComparison.CurrentCulture))
                return false;

            return true;
        }
    }
}
