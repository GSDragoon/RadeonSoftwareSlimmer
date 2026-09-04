using System;
using RadeonSoftwareSlimmer.Core.Interfaces;

namespace RadeonSoftwareSlimmer.Core.Test.TestDoubles
{
    public class FakeAppLogger : IAppLogger
    {
        public bool IsLoading { get; set; }

        public void Info(string message) { }
        public void Info(Exception ex) { }
        public void Info(Exception ex, string message) { }

        public void Debug(string message) { }
        public void Debug(Exception ex) { }
        public void Debug(Exception ex, string message) { }

        public void Error(Exception ex) { }
        public void Error(Exception ex, string message) { }
    }
}
