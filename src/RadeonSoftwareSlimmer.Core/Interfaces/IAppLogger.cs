using System;

namespace RadeonSoftwareSlimmer.Core.Interfaces
{
    public interface IAppLogger
    {
        void Info(string message);
        void Info(Exception ex);

        void Debug(string message);
        void Debug(Exception ex);

        void Error(Exception ex);
        void Error(Exception ex, string message);
    }
}
