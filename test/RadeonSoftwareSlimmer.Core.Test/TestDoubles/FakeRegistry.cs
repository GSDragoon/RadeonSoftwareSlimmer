using RadeonSoftwareSlimmer.Core.Interfaces;

namespace RadeonSoftwareSlimmer.Core.Test.TestDoubles
{
    public class FakeRegistry : IRegistry
    {
        public FakeRegistry()
        {
            MockCurrentUser = new FakeRegistryKey();
            MockLocalMachine = new FakeRegistryKey();
        }

        public FakeRegistryKey MockCurrentUser { get; }
        public FakeRegistryKey MockLocalMachine { get; }


        public IRegistryKey CurrentUser => MockCurrentUser;

        public IRegistryKey LocalMachine => MockLocalMachine;
    }
}
