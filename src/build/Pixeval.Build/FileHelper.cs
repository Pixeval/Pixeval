using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Pixeval.Build
{
    public static class FileHelper
    {
        public static string FindSrc()
        {
            var dir = Directory.GetParent(Assembly.GetExecutingAssembly().Location);
            while (dir!.Name != "src")
            {
                dir = dir.Parent;
            }

            return dir.FullName;
        }

        public static IEnumerable<FileInfo> ListFiles(DirectoryInfo directory)
        {
            var stack = new Stack<DirectoryInfo>();
            stack.Push(directory);
            while (stack.Any())
            {
                var dir = stack.Pop();
                foreach (var directoryInfo in dir.EnumerateDirectories())
                {
                    stack.Push(directoryInfo);
                }

                foreach (var fileInfo in dir.GetFiles())
                {
                    yield return fileInfo;
                }
            }
        }
    }
}