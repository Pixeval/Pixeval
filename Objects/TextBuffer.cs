using System.IO;

namespace Pixeval.Objects
{
    public class TextBuffer
    {
        public static FileInfo GetOrCreateFile(string path)
        {
            if (!File.Exists(path)) File.Create(path);
            return new FileInfo(path);
        }

        public static string GetOrCreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }

        public static string GetExtension(string file)
        {
            return file[file.LastIndexOf('.')..];
        }

        public static void WriteAllBytesTaskAsync(string path, byte[] bytes)
        {
            File.WriteAllBytesAsync(path, bytes);
        }
    }
}