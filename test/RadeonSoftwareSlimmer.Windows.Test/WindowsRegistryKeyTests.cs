using System;
using Microsoft.Win32;
using NUnit.Framework;
using RadeonSoftwareSlimmer.Core.Enums;
using RadeonSoftwareSlimmer.Core.Interfaces;

namespace RadeonSoftwareSlimmer.Windows.Test
{
    [NonParallelizable]
    public class WindowsRegistryKeyTests
    {
        private const string ScratchRootPath = @"Software\RadeonSoftwareSlimmerTest";

        [SetUp]
        public void SetUp()
        {
            Registry.CurrentUser.DeleteSubKeyTree(ScratchRootPath, throwOnMissingSubKey: false);
            Registry.CurrentUser.CreateSubKey(ScratchRootPath).Dispose();
        }

        [TearDown]
        public void TearDown()
        {
            Registry.CurrentUser.DeleteSubKeyTree(ScratchRootPath, throwOnMissingSubKey: false);
        }


        private static WindowsRegistryKey OpenScratch(bool writable)
        {
            RegistryKey inner = Registry.CurrentUser.OpenSubKey(ScratchRootPath, writable);
            return new WindowsRegistryKey(inner);
        }


        [Test]
        public void Ctor_NullRegistryKey_Throws()
        {
            Assert.That((System.Action)(() => new WindowsRegistryKey(null)), Throws.ArgumentNullException);
        }

        [Test]
        public void Name_ReflectsUnderlyingRegistryKey()
        {
            using (WindowsRegistryKey key = OpenScratch(writable: false))
            {
                Assert.That(key.Name, Does.EndWith(ScratchRootPath));
            }
        }


        [Test]
        public void OpenSubKey_ExistingSubKey_ReturnsWrapper()
        {
            using (RegistryKey root = Registry.CurrentUser.OpenSubKey(ScratchRootPath, writable: true))
            {
                root.CreateSubKey("Child").Dispose();
            }

            using (WindowsRegistryKey key = OpenScratch(writable: false))
            using (IRegistryKey subKey = key.OpenSubKey("Child", writable: false))
            {
                Assert.That(subKey, Is.Not.Null);
            }
        }

        [Test]
        public void OpenSubKey_MissingSubKey_ReturnsNull()
        {
            using (WindowsRegistryKey key = OpenScratch(writable: false))
            using (IRegistryKey subKey = key.OpenSubKey("DoesNotExist", writable: false))
            {
                Assert.That(subKey, Is.Null);
            }
        }

        [Test]
        public void OpenSubKey_ReadOnly_SetValueThrows()
        {
            using (WindowsRegistryKey key = OpenScratch(writable: false))
            {
                Assert.That((System.Action)(() => key.SetValue("Something", 1, CoreRegistryValueKind.DWord)),
                    Throws.TypeOf<UnauthorizedAccessException>());
            }
        }


        [Test]
        public void GetSubKeyNames_ReturnsAllChildKeys()
        {
            using (RegistryKey root = Registry.CurrentUser.OpenSubKey(ScratchRootPath, writable: true))
            {
                root.CreateSubKey("First").Dispose();
                root.CreateSubKey("Second").Dispose();
                root.CreateSubKey("Third").Dispose();
            }

            using (WindowsRegistryKey key = OpenScratch(writable: false))
            {
                Assert.That(key.GetSubKeyNames(), Is.EquivalentTo(new[] { "First", "Second", "Third" }));
            }
        }


        [Test]
        public void GetValue_ExistingValue_ReturnsValue()
        {
            using (RegistryKey root = Registry.CurrentUser.OpenSubKey(ScratchRootPath, writable: true))
            {
                root.SetValue("Greeting", "hello", RegistryValueKind.String);
            }

            using (WindowsRegistryKey key = OpenScratch(writable: false))
            {
                Assert.That(key.GetValue("Greeting"), Is.EqualTo("hello"));
            }
        }

        [Test]
        public void GetValue_MissingValue_ReturnsNull()
        {
            using (WindowsRegistryKey key = OpenScratch(writable: false))
            {
                Assert.That(key.GetValue("NotThere"), Is.Null);
            }
        }

        [Test]
        public void GetValue_MissingValueWithDefault_ReturnsDefault()
        {
            using (WindowsRegistryKey key = OpenScratch(writable: false))
            {
                Assert.That(key.GetValue("NotThere", "fallback"), Is.EqualTo("fallback"));
            }
        }


        [Test]
        public void SetValue_String_RoundTrips()
        {
            using (WindowsRegistryKey key = OpenScratch(writable: true))
            {
                key.SetValue("Str", "value", CoreRegistryValueKind.String);
            }

            using (WindowsRegistryKey key = OpenScratch(writable: false))
            {
                Assert.That(key.GetValue("Str"), Is.EqualTo("value"));
            }
        }

        [Test]
        public void SetValue_DWord_PersistsAsDWordKind()
        {
            using (WindowsRegistryKey key = OpenScratch(writable: true))
            {
                key.SetValue("Number", 42, CoreRegistryValueKind.DWord);
            }

            using (RegistryKey root = Registry.CurrentUser.OpenSubKey(ScratchRootPath, writable: false))
            {
                using (Assert.EnterMultipleScope())
                {
                    Assert.That(root.GetValue("Number"), Is.EqualTo(42));
                    Assert.That(root.GetValueKind("Number"), Is.EqualTo(RegistryValueKind.DWord));
                }
            }
        }

        [Test]
        public void SetValue_QWord_PersistsAsQWordKind()
        {
            using (WindowsRegistryKey key = OpenScratch(writable: true))
            {
                key.SetValue("Big", 4294967296L, CoreRegistryValueKind.QWord);
            }

            using (RegistryKey root = Registry.CurrentUser.OpenSubKey(ScratchRootPath, writable: false))
            {
                using (Assert.EnterMultipleScope())
                {
                    Assert.That(root.GetValue("Big"), Is.EqualTo(4294967296L));
                    Assert.That(root.GetValueKind("Big"), Is.EqualTo(RegistryValueKind.QWord));
                }
            }
        }


        [Test]
        public void Dispose_MultipleTimes_DoesNotThrow()
        {
            WindowsRegistryKey key = OpenScratch(writable: false);

            key.Dispose();

            Assert.That((System.Action)key.Dispose, Throws.Nothing);
        }
    }
}
