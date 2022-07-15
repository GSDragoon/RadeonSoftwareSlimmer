using NUnit.Framework;
using RadeonSoftwareSlimmer.ViewModels;

namespace RadeonSoftwareSlimmer.Test
{
    [SetUpFixture]
    public static class SetupFixture
    {
        [OneTimeSetUp]
        public static void OneTimeSetup()
        {
            StaticViewModel.LogToConsole = true;
        }
    }
}
