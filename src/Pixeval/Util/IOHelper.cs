using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Pixeval.Util
{
    // ReSharper disable once InconsistentNaming
    public static class IOHelper
    {
        public static async Task<string> CalculateChecksum<T>(string fullnameOfFile) where T : HashAlgorithm, new()
        {
            return await (await File.ReadAllBytesAsync(fullnameOfFile)).HashAsync<T>();
        }

        /// <summary>
        /// Deletes directory and all the subdirectories before create a new one with the same name
        /// </summary>
        public static void ReinitializeDirectory(string dir)
        {
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }
            Directory.CreateDirectory(dir);
        }
    }
}