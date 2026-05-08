using System;
using System.Runtime.CompilerServices;

namespace RadeonSoftwareSlimmer.Core.Interfaces
{
    public interface IAppLogger
    {
        bool IsLoading { get; set; }

        void Info(string message);
        void Info(Exception ex);
        void Info(Exception ex, string message);

        void Debug(string message);
        void Debug(Exception ex);
        void Debug(Exception ex, string message);

        void Error(Exception ex);
        void Error(Exception ex, string message);
    }
}
