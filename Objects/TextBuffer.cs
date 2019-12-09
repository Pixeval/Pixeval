using System.IO;
using System.Text;

namespace Pixeval.Objects
{
    public class TextBuffer
    {
        public static FileInfo GetOrCreateFile(string path)
        {
            if (!File.Exists(path))
            {
                File.Create(path);
            }
            return new FileInfo(path);
        }

        public static void SaveText<T>(string path, T obj, IConverter<T, string> converter)
        {
            File.WriteAllText(path, converter.Convert(obj), Encoding.UTF8);
        }

        public static T LoadText<T>(string path, IConverter<string, T> converter)
        {
            return converter.Convert(File.ReadAllText(path, Encoding.UTF8));
        }
    }
}