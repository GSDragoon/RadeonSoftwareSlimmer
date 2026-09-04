using Microsoft.Win32;
using NUnit.Framework;
using RadeonSoftwareSlimmer.Core.Interfaces;

namespace RadeonSoftwareSlimmer.Windows.Test
{
    public class WindowsRegistryTests
    {
        [Test]
        public void CurrentUser_IsNotNull()
        {
            WindowsRegistry registry = new WindowsRegistry();

            Assert.That(registry.CurrentUser, Is.Not.Null);
        }

        [Test]
        public void LocalMachine_IsNotNull()
        {
            WindowsRegistry registry = new WindowsRegistry();

            Assert.That(registry.LocalMachine, Is.Not.Null);
        }

        [Test]
        public void CurrentUser_NameMatchesHkeyCurrentUser()
        {
            WindowsRegistry registry = new WindowsRegistry();

            Assert.That(registry.CurrentUser.Name, Is.EqualTo(Registry.CurrentUser.Name));
        }

        [Test]
        public void LocalMachine_NameMatchesHkeyLocalMachine()
        {
            WindowsRegistry registry = new WindowsRegistry();

            Assert.That(registry.LocalMachine.Name, Is.EqualTo(Registry.LocalMachine.Name));
        }

        [Test]
        public void CurrentUser_ReturnsSameInstanceAcrossInvocations()
        {
            WindowsRegistry registry = new WindowsRegistry();

            IRegistryKey first = registry.CurrentUser;
            IRegistryKey second = registry.CurrentUser;

            Assert.That(second, Is.SameAs(first));
        }
    }
}
