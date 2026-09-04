using RadeonSoftwareSlimmer.Core.Interfaces;

namespace RadeonSoftwareSlimmer.Core.Test.TestDoubles
{
    public class FakeProcessRunner : IProcessRunner
    {
        public int ExitCode { get; set; }
        public string LastFileName { get; private set; }
        public string LastArguments { get; private set; }

        public int RunProcess(string fileName, string arguments)
        {
            LastFileName = fileName;
            LastArguments = arguments;
            return ExitCode;
        }
    }
}
