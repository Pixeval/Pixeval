using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Pixeval.LoginProxy
{
    public static class Interop
    {
        private static readonly HttpClient HttpClient = new();

        public static Task<HttpResponseMessage> PostJsonToPixevalClient<T>(string path, T obj)
        {
            var url = $"http://localhost:{App.Port}{path}";
            return HttpClient.PostAsJsonAsync(url, obj);
        }
    }
}