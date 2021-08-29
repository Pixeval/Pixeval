using System.Collections;
using System.Collections.Generic;
using Pixeval.ViewModel;

namespace Pixeval.Util.Generic
{
    public class IllustrationViewModelPublishDateComparer : IComparer<IllustrationViewModel>, IComparer
    {
        public static readonly IllustrationViewModelPublishDateComparer Instance = new();

        public int Compare(IllustrationViewModel? x, IllustrationViewModel? y)
        {
            if (x is null || y is null)
            {
                return 0;
            }

            return x.PublishDate.CompareTo(y.PublishDate);
        }

        public int Compare(object? x, object? y)
        {
            return Compare(x as IllustrationViewModel, y as IllustrationViewModel);
        }
    }

    public class IllustrationBookmarkComparer : IComparer<IllustrationViewModel>, IComparer
    {
        public static readonly IllustrationBookmarkComparer Instance = new();

        public int Compare(IllustrationViewModel? x, IllustrationViewModel? y)
        {
            if (x?.Illustration is { } xi && y?.Illustration is { } yi)
            {
                return xi.TotalBookmarks.CompareTo(yi.TotalBookmarks);
            }

            return 0;
        }

        public int Compare(object? x, object? y)
        {
            return Compare(x as IllustrationViewModel, y as IllustrationViewModel);
        }
    }
}
