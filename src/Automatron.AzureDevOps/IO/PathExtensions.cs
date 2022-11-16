using System;
using System.IO;

namespace Automatron.AzureDevOps.IO
{
    public static class PathExtensions
    {
        internal static string GetRelativePath(string relativeTo, string path)
        {
            var uri = new Uri(relativeTo);
            var rel = Uri.UnescapeDataString(uri.MakeRelativeUri(new Uri(path)).ToString()).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            if (rel.Contains(Path.DirectorySeparatorChar.ToString()) == false)
            {
                rel = $".{Path.DirectorySeparatorChar}{rel}";
            }
            return rel;
        }

        public static string GetUnixPath(string path)
        {
            return path.Replace("\\", "/");
        }

        public static string? GetGitRoot()
        {
            return GetGitRoot(Directory.GetCurrentDirectory());
        }

        public static string? GetGitRoot(string workingDirectory)
        {
            var dir = workingDirectory;

            while (dir != null)
            {
                if (IsGitRoot(dir))
                {
                    return dir;
                }

                dir = Directory.GetParent(dir)?.FullName;
            }

            return null;
        }

        internal static bool IsGitRoot(string dir)
        {
            return Directory.Exists(Path.Combine(dir, ".git"));
        }
    }
}
