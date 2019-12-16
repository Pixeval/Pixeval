using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Pixeval.Objects;

namespace Pixeval.Data.ViewModel
{
    internal class DownloadList
    {
        public static ObservableCollection<Illustration> ToDownloadList { get; } = new ObservableCollection<Illustration>();

        public static void Add(Illustration illustration)
        {
            if (ToDownloadList.All(x => x.Id != illustration.Id)) ToDownloadList.Add(illustration);
        }

        public static void AddRange(IEnumerable<Illustration> illustrations)
        {
            ToDownloadList.AddRange(illustrations.Where(x => ToDownloadList.All(i => i.Id != x.Id)));
        }

        public static void Remove(Illustration illustration)
        {
            ToDownloadList.Remove(illustration);
        }
    }
}