using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pixeval.Navigation
{
    public class NavigationEventArgs : EventArgs
    {
        public NavigationEventArgs(INavigablePage? source, INavigablePage? destination)
        {
            Source = source;
            Destination = destination;
        }

        public INavigablePage? Source { get; set; }
        public INavigablePage? Destination { get; set; }
    }
}
