using System;

namespace RadeonSoftwareSlimmer.Core.Interfaces
{
    public interface IProcessRunner
    {
        int RunProcess(string fileName, string arguments);
    }
}
