using System.Collections.ObjectModel;
using System.Linq;

namespace Pixeval.Data.Model.ViewModel
{
    internal class DownloadList
    {
        public static ObservableCollection<Illustration> ToDownloadList { get; } = new ObservableCollection<Illustration>();

        public static void Add(Illustration illustration)
        {
            if (ToDownloadList.All(x => x.Id != illustration.Id))
            {
                ToDownloadList.Add(illustration);
            }
        }

        public static void Remove(Illustration illustration)
        {
            ToDownloadList.Remove(illustration);
        }
    }
}