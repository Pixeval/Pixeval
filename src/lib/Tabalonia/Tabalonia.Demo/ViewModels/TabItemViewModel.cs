using CommunityToolkit.Mvvm.ComponentModel;

namespace Tabalonia.Demo.ViewModels
{
    public class TabItemViewModel : ObservableObject
    {
        public string Header { get; set; }
        
        public string SimpleContent { get; set; }


        public override string ToString() => Header;
    }
}