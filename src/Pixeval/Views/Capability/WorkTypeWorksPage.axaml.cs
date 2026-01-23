using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Mako;
using Mako.Engine;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.Controls;
using Frame = FluentAvalonia.UI.Controls.Frame;

namespace Pixeval.Views.Capability;

public abstract partial class WorkTypeWorksPage : UserControl
{
    public WorkTypeWorksPage()
    {
        InitializeComponent();
        AddHandler(Frame.NavigatedToEvent, (sender, e) => ChangeSource());
    }

    private void WorkTypeComboBox_OnSelectionChanged(SymbolComboBox sender, EventArgs e)
    {
        ChangeSource();
    }

    private void WorkContainer_OnRefreshRequested(object? sender, RoutedEventArgs e)
    {
        ChangeSource();
    }

    private void ChangeSource()
    {
        WorkContainer.ResetEngine(GetFetchEngine(App.AppViewModel.MakoClient, WorkTypeComboBox.GetSelectedValue<WorkType>()));
    }

    protected abstract IFetchEngine<IWorkEntry> GetFetchEngine(MakoClient makoClient, WorkType workType);
}

public class RecommendWorksPage : WorkTypeWorksPage
{
    protected override IFetchEngine<IWorkEntry> GetFetchEngine(MakoClient makoClient, WorkType workType)
    {
        return makoClient.RecommendedWorks(workType, PixevalSettings.TargetFilter);
    }
}

public class NewWorksPage : WorkTypeWorksPage
{
    protected override IFetchEngine<IWorkEntry> GetFetchEngine(MakoClient makoClient, WorkType workType)
    {
        return makoClient.NewWorks(workType, PixevalSettings.TargetFilter);
    }
}

public class UserWorkPostsPage : WorkTypeWorksPage
{
    private long _userId;

    public UserWorkPostsPage()
    {
        AddHandler(Frame.NavigatedToEvent, (sender, e) =>
        {
            if (e.Parameter is not long uid)
                uid = App.AppViewModel.PixivUid;

            _userId = uid;
        });
    }

    protected override IFetchEngine<IWorkEntry> GetFetchEngine(MakoClient makoClient, WorkType workType)
    {
        return makoClient.WorkPosts(_userId, workType, PixevalSettings.TargetFilter);
    }
}
