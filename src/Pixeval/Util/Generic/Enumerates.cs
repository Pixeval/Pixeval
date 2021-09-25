using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.WinUI.UI;
using Pixeval.Utilities;

namespace Pixeval.Util.Generic
{
    public static class Enumerates
    {
        public static void ResetView(this AdvancedCollectionView view)
        {
            var sourceCopy = view.Source.Cast<object>().ToList();
            foreach (var o in sourceCopy)
            {
                view.Remove(o);
            }
            view.AddRange(sourceCopy);
        }
    }
}