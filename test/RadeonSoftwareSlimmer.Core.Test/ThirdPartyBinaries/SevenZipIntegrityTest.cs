using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using NUnit.Framework;

// This test intentionally reads a physical on-disk file, so the System.IO.Abstractions wrappers do not apply.
#pragma warning disable IO0002 // Replace File class with IFileSystem.File for improved testability
#pragma warning disable IO0006 // Replace Path class with IFileSystem.Path for improved testability

namespace RadeonSoftwareSlimmer.Core.Test.ThirdPartyBinaries
{
    // Guards the checked-in 7-Zip blobs (src/Shared/7-Zip) against accidental corruption or unauthorized replacement.
    // When 7-Zip is intentionally upgraded, update the expected hashes below with the values printed on failure.
    // Source: 7-Zip 26.02 (x64), https://www.7-zip.org/
    public class SevenZipIntegrityTest
    {
        private const string SevenZipDirectory = "ThirdPartyBinaries/7-Zip";

        // SHA-256 of src/Shared/7-Zip/7z.exe
        private const string ExpectedSevenZipExeSha256 =
            "83967f1b02b43c4efeda302795722c809e0e81b8307de73558d10484d5676a7d";

        // SHA-256 of src/Shared/7-Zip/7z.dll
        private const string ExpectedSevenZipDllSha256 =
            "69fd4df057985c40e510e2fac182881c7f85e90aa13ec703f763a8fdb2ce61f8";

        // SHA-256 of src/Shared/7-Zip/License.txt
        private const string ExpectedSevenZipLicenseSha256 =
            "519ac0a4bded9c18ea02e0afb71f663d8c47373bd9facd3ac96a79f51d77765d";


        [TestCase("7z.exe", ExpectedSevenZipExeSha256)]
        [TestCase("7z.dll", ExpectedSevenZipDllSha256)]
        [TestCase("License.txt", ExpectedSevenZipLicenseSha256)]
        public void ThirdPartyFile_Sha256_MatchesExpected(string fileName, string expectedSha256)
        {
            string filePath = Path.Combine(AppContext.BaseDirectory, SevenZipDirectory, fileName);

            Assert.That(File.Exists(filePath), Is.True, $"Third-party file not found at '{filePath}'.");

            string actualSha256 = ComputeSha256(filePath);

            Assert.That(
                actualSha256,
                Is.EqualTo(expectedSha256).IgnoreCase,
                $"SHA-256 mismatch for '{fileName}'. If this change is intentional, update the expected constant to: {actualSha256}");
        }


        private static string ComputeSha256(string filePath)
        {
            using (SHA256 sha = SHA256.Create())
            using (FileStream stream = File.OpenRead(filePath))
            {
                byte[] hash = sha.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", string.Empty).ToLower(CultureInfo.InvariantCulture);
            }
        }
    }
}
