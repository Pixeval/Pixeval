using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Pixeval.UserControls.JustifiedLayout;

public sealed partial class JustifiedListView
{
    public ObservableCollection<ICollection<JustifiedListViewRowItemWrapper>> Items { get; set; } = new();

    public JustifiedListView()
    {
        InitializeComponent();
    }
}