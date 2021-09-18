using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media;
using Pixeval.CoreApi;
using Pixeval.CoreApi.Net;
using Pixeval.CoreApi.Util;

namespace Pixeval.Util.IO
{
    public static partial class IOHelper
    {
        public static async Task<Result<ImageSource>> DownloadSoftwareBitmapSourceResultAsync(this MakoClient client, string url)
        {
            return await (await client.GetMakoHttpClient(MakoApiKind.ImageApi).DownloadAsIRandomAccessStreamAsync(url))
                .BindAsync(async m => (ImageSource) await m.GetSoftwareBitmapSourceAsync(true));
        }

        public static async Task<Result<ImageSource>> DownloadBitmapImageResultAsync(this MakoClient client, string url)
        {
            return await (await client.GetMakoHttpClient(MakoApiKind.ImageApi).DownloadAsIRandomAccessStreamAsync(url))
                .BindAsync(async m => (ImageSource) await m.GetBitmapImageAsync(true));
        }
    }
}
