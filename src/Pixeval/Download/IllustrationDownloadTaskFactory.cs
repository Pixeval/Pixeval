using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pixeval.Download.MacroParser;
using Pixeval.Util;
using Pixeval.Utilities;
using Pixeval.ViewModel;

namespace Pixeval.Download
{
    public class IllustrationDownloadTaskFactory : IDownloadTaskFactory<IllustrationViewModel>
    {
        public IMetaPathParser<IllustrationViewModel> PathParser { get; }

        public IllustrationDownloadTaskFactory()
        {
            PathParser = new IllustrationMetaPathParser();
        }

        public async Task<IEnumerable<IDownloadTask>> Create(IllustrationViewModel context, string rawPath)
        {
            if (context.Illustration.IsUgoira())
            {
                var ugoiraMetadata = await App.AppViewModel.MakoClient.GetUgoiraMetadataAsync(context.Id);
                return ugoiraMetadata.UgoiraMetadataInfo?.ZipUrls?.Medium is { } url
                    ? Enumerates.EnumerableOf(new AnimatedIllustrationDownloadTask(url, ugoiraMetadata, PathParser.Reduce(rawPath, context)))
                    : Enumerable.Empty<IDownloadTask>();
            }

            return context.GetMangaIllustrationViewModels().Select(m => new IllustrationDownloadTask(
                m.Illustration.GetOriginalUrl()!,
                PathParser.Reduce(rawPath, m)));
        }
    }
}