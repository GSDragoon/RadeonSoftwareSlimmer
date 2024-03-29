﻿using System.ComponentModel;
using System.IO.Abstractions;

namespace RadeonSoftwareSlimmer.Models.PreInstall
{
    public class ScheduledTaskXmlModel : INotifyPropertyChanged
    {
        private bool _enabled;
        private readonly IFileInfo _file;


        public ScheduledTaskXmlModel(IFileInfo file)
        {
            _file = file;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                OnPropertyChanged(nameof(Enabled));
            }
        }
        public string Uri { get; internal set; }
        public string Command { get; internal set; }
        public string Description { get; internal set; }

        public IFileInfo GetFile()
        {
            return _file;
        }
    }
}
