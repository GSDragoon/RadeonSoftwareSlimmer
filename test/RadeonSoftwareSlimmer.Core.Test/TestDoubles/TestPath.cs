using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace RadeonSoftwareSlimmer.Core.Test.TestDoubles
{
    [SuppressMessage("System.IO.Abstractions", "IO0006:Replace Path class with IFileSystem.Path for improved testability", Justification = "This helper composes test paths from raw strings before a MockFileSystem exists.")]
    public static class TestPath
    {
        // MockFileSystem uses real OS path semantics, so tests need a root the current OS accepts.
        public static string Root { get; } =
            Path.DirectorySeparatorChar == '\\' ? @"C:\rss-test" : "/rss-test";


        // Builds an absolute test path under Root from a relative fragment written with either separator.
        public static string Rooted(string relative)
        {
            string normalized = Normalize(relative).TrimStart(Path.DirectorySeparatorChar);
            return Path.Combine(Root, normalized);
        }

        // Builds a leading-separator relative path (matches how DisplayComponentModel.Directory is formatted).
        public static string Relative(string relative)
        {
            return Path.DirectorySeparatorChar + Normalize(relative).TrimStart(Path.DirectorySeparatorChar);
        }

        private static string Normalize(string relative)
        {
            return relative.Replace('\\', Path.DirectorySeparatorChar)
                           .Replace('/', Path.DirectorySeparatorChar);
        }
    }
}
