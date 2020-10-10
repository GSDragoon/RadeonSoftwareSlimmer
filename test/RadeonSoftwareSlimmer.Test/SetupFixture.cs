using NUnit.Framework;
using RadeonSoftwareSlimmer.ViewModels;

namespace RadeonSoftwareSlimmer.Test
{
    [SetUpFixture]
    public class SetupFixture
    {
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            StaticViewModel.LogToConsole = true;
        }
    }
}
