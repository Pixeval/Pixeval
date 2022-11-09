using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pixeval.Startup.WinUI.Navigation
{
    public interface INavigablePage
    {
        void OnNavigatedFrom(INavigationRoot root, INavigablePage? from, params object?[]? args);
    }
}
