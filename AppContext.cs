using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Pixeval.Core;
using Pixeval.Data.ViewModel;

namespace Pixeval
{
    public static class AppContext
    {
        public static readonly string ProjectFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "pixeval");

        public static readonly string ConfFolder = ProjectFolder;

        public static readonly string SettingsFolder = ProjectFolder;

        public static readonly string ExceptionReportFolder = Path.Combine(ProjectFolder, "crash-reports");

        public static readonly string ConfigurationFileName = "pixeval_conf.json";

        internal static bool LogoutExit = false;

        static AppContext()
        {
            Directory.CreateDirectory(ProjectFolder);
            Directory.CreateDirectory(SettingsFolder);
            Directory.CreateDirectory(ExceptionReportFolder);
        }

        public static IDownloadPathProvider DownloadPathProvider = new DefaultDownloadPathProvider();

        public static IIllustrationFileNameFormatter FileNameFormatter = new DefaultIllustrationFileNameFormatter();

        public static ObservableCollection<DownloadableIllustrationViewModel> Downloading = new ObservableCollection<DownloadableIllustrationViewModel>();

        public static void EnqueueDownloadItem(Illustration illustration)
        {
            static void RemoveAction(DownloadableIllustrationViewModel d) => Downloading.Remove(d);
            if (illustration.IsManga)
            {
                for (var j = 0; j < illustration.MangaMetadata.Length; j++)
                {
                    var model = new DownloadableIllustrationViewModel(illustration.MangaMetadata[j], true, j) {DownloadFinished = RemoveAction};
                    Task.Run(() => model.Download());
                    Downloading.Add(model);
                }
            }
            else
            {
                var model = new DownloadableIllustrationViewModel(illustration, false) {DownloadFinished = RemoveAction};
                Task.Run(() => model.Download());
                Downloading.Add(model);
            }
        }
    }
}