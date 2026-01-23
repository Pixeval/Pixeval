using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;


namespace Tabalonia.Demo.ViewModels;


public class MainViewModel : ObservableObject
{
    private int _i;
        
        
    public Func<object> NewItemFactory => AddItem;


    public ObservableCollection<TabItemViewModel> TabItems { get; } = new();


    public MainViewModel()
    {
        TabItems.Add(new TabItemViewModel()
        {
            Header = "Fixed Tab",
            SimpleContent = "Fixed Tab content"
        });
            
        const int count = 10;

        for (int i = 0; i < count; i++)
        {
            TabItems.Add((TabItemViewModel) AddItem());
        }
    }
        
        
    private object AddItem()
    {
        return new TabItemViewModel
        {
            Header = $"Tab {++_i}",
            SimpleContent = $"Tab {_i} content"
        };
    }
}