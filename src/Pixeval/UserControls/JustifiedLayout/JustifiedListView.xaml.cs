using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Util.Threading;
using Pixeval.Utilities;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;
using WinUIEx.Messaging;
using UIHelper = Pixeval.Util.UI.UIHelper;

namespace Pixeval.UserControls.JustifiedLayout;

public class JustifiedListViewLoadMoreRequestEventArgs
{
    public JustifiedListViewLoadMoreRequestEventArgs()
    {
        Deferral = new TaskCompletionSource<int>();
    }

    public TaskCompletionSource<int> Deferral { get; }
}

/// <summary>
/// This class exists because in WinUI, there is no way to bind outside of a <see cref="DataTemplate"/>, so we store <see cref="DataTemplate"/>
/// in this class and bind it to the <see cref="JustifiedListView"/>
/// </summary>
public record JustifiedListViewRowBinding(DataTemplate Template, ICollection<JustifiedListViewRowItemWrapper> Wrappers);

[DependencyProperty<DataTemplate>("ItemTemplate")]
// The computed layout may be wider than the actual container's width, use this property to tune the width, the actual computed layout width will be the [ContainerWidth - this property]
[DependencyProperty<int>("ContainerWidthDeviationOffset", DefaultValue = "55")]
[DependencyProperty<int>("DesireRows", DefaultValue = "4")] 
[DependencyProperty<int>("Spacing", DefaultValue = "2")]
[DependencyProperty<object>("Header")]
public sealed partial class JustifiedListView
{
    // Help finding the corresponding item in outer grid, so that we can perform the connect animation
    public IEnumerable<JustifiedListViewRow> Children => ItemsListView.FindDescendants().OfType<JustifiedListViewRow>();

    private readonly ObservableCollection<IllustrationView.IllustrationViewModel> _itemsSource;

    private int _loadingMore; // use int instead of bool for CAS operation

    private bool _resizedByMaximizationOrRestoration;
    private bool _resizing;

    
    private const uint WmExitSizeMove = 0x0232;
    private const uint WmSysCommand = 0x0112;
    private const uint WmSizing = 0x0214;
    private const nuint ScMaximized = 0xF030;
    private const nuint ScRestore = 0xF120;

    private static readonly WindowMessageMonitor Monitor = new(CurrentContext.Window);

    private EventHandler<JustifiedListViewLoadMoreRequestEventArgs>? _loadMoreRequested;

    public event EventHandler<JustifiedListViewLoadMoreRequestEventArgs> LoadMoreRequested
    {
        add => _loadMoreRequested += value;
        remove => _loadMoreRequested -= value;
    }

    private EventHandler<EffectiveViewportChangedEventArgs>? _rowBringingIntoView;

    public event EventHandler<EffectiveViewportChangedEventArgs> RowBringingIntoView
    {
        add => _rowBringingIntoView += value;
        remove => _rowBringingIntoView -= value;
    }

    public Predicate<object>? Filter { get; set; }

    public JustifiedListView()
    {
        InitializeComponent();
        _itemsSource = new ObservableCollection<IllustrationView.IllustrationViewModel>();
    }

    private void JustifiedListView_OnLoaded(object sender, RoutedEventArgs e)
    {
        Monitor.WindowMessageReceived += MonitorOnWindowMessageReceived;
    }

    private void JustifiedListView_OnUnloaded(object sender, RoutedEventArgs e)
    {
        Monitor.WindowMessageReceived -= MonitorOnWindowMessageReceived;
    }

    private void ItemsListView_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (_resizedByMaximizationOrRestoration)
        {
            RecomputeLayout();
            _resizedByMaximizationOrRestoration = false;
        }
    }

    private void MonitorOnWindowMessageReceived(object? sender, WindowMessageEventArgs e)
    {
        switch (e.Message.MessageId)
        {
            case WmSysCommand:
                if ((e.Message.WParam & 0xFFF0) == ScMaximized || (e.Message.WParam & 0xFFF0) == ScRestore) 
                    _resizedByMaximizationOrRestoration = true;
                break;
            case WmSizing:
                _resizing = true;
                break;
            case WmExitSizeMove:
                if (_resizing)
                {
                    RecomputeLayout();
                    _resizing = false;
                }
                break;
        }
    }

    public void RecomputeLayout()
    {
        var boxes = JustifiedListViewHelper.ComputeJustifiedListViewLayout(
            _itemsSource.Where(i => Filter?.Invoke(i) ?? true).Select(vm => (vm, vm.Illustration.Width / (double) vm.Illustration.Height)),
            (int) ItemsListView.ActualWidth - ContainerWidthDeviationOffset, Spacing, (int) (ActualHeight / DesireRows));
        var newSource = new ObservableCollection<JustifiedListViewRowBinding>();
        ItemsListView.ItemsSource = newSource;
        foreach (var justifiedListViewRowItemWrappers in boxes)
        {
            newSource.Add(new JustifiedListViewRowBinding(ItemTemplate, justifiedListViewRowItemWrappers));
        }
    }

    public void Append(IEnumerable<IllustrationView.IllustrationViewModel> more)
    {
        var illustrationViewModels = more as IllustrationView.IllustrationViewModel[] ?? more.ToArray();
        var boxes = JustifiedListViewHelper.ComputeJustifiedListViewLayout(
            illustrationViewModels.Where(i => Filter?.Invoke(i) ?? true).Select(vm => (vm, vm.Illustration.Width / (double) vm.Illustration.Height)),
            (int) ItemsListView.ActualWidth - ContainerWidthDeviationOffset, Spacing, (int) (ActualHeight / DesireRows));
        var source = (ObservableCollection<JustifiedListViewRowBinding>) ItemsListView.ItemsSource;
        _itemsSource.AddRange(illustrationViewModels);
        foreach (var justifiedListViewRowItemWrappers in boxes)
        {
            source.Add(new JustifiedListViewRowBinding(ItemTemplate, justifiedListViewRowItemWrappers));
        }
    }

    public void Clear()
    {
        _itemsSource.Clear();
        RecomputeLayout();
    }

    public void Remove(IEnumerable<IllustrationView.IllustrationViewModel> items)
    {
        foreach (var illustrationViewModel in items)
        {
            _itemsSource.Remove(illustrationViewModel);
        }

        RecomputeLayout();
    }

    private void JustifiedListViewRow_OnEffectiveViewportChanged(FrameworkElement sender, EffectiveViewportChangedEventArgs args)
    {
        var preLoadRows = Math.Clamp(App.AppViewModel.AppSetting.PreLoadRows, 1, 15);

        if (args.BringIntoViewDistanceY <= sender.ActualHeight * preLoadRows)
        {
            _rowBringingIntoView?.Invoke(sender, args);
            if (sender.GetDataContext<JustifiedListViewRowBinding?>()?.Wrappers.Any(c => c.Item.Id == _itemsSource.LastOrDefault(i => Filter?.Invoke(i) ?? true)?.Id) == true)
            {
                RequestLoadMoreAsync().Discard();;
            } 
        }
    }

    /// <summary>
    /// Calls to <see cref="RequestLoadMoreAsync"/> continuously until the elements fill the <see cref="ListView"/>,
    /// i.e., more elements than those in the viewport of the <see cref="ListView"/>
    /// </summary>
    public async Task FillListViewAsync()
    {
        while (Children.All(c => UIHelper.IsFullyOrPartiallyVisible(c, this)) && await RequestLoadMoreAsync() > 0) { }
    }

    /// <summary>
    /// Requests to load more elements into <see cref="ListView"/>
    /// </summary>
    /// <returns>The number of newly loaded items. If the method is called when another loading operation is ongoing, it returns <code>-1</code></returns>
    public async Task<int> RequestLoadMoreAsync()
    {
        if (Interlocked.CompareExchange(ref _loadingMore, 1, 0) != 1)
        {
            var e = new JustifiedListViewLoadMoreRequestEventArgs();
            _loadMoreRequested?.Invoke(this, e);
            var result = await e.Deferral.Task;
            do { } while (Interlocked.CompareExchange(ref _loadingMore, 0, 1) != 1);

            return result;
        }

        return -1;
    }
}