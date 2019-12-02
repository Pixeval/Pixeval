using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Pzxlane.Objects
{
    // ReSharper disable once InconsistentNaming
    public class AsyncIO
    {
        public static void DownloadFile(string url, string path, Dictionary<string, string> header)
        {
            var httpClient = new HttpClient();

            foreach (var (key, value) in header)
            {
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation(key, value);
            }

            Task.Run(async () => File.WriteAllBytes(path, await httpClient.GetByteArrayAsync(url)));
        }
    }
}