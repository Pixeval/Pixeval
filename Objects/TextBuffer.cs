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
    }
}