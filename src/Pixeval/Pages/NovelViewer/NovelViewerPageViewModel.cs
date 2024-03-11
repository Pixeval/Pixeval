#region Copyright

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/NovelViewerPageViewModel.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Windows.System;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Pixeval.Controls;
using Pixeval.Controls.MarkupExtensions;
using Pixeval.Controls.Windowing;
using Pixeval.CoreApi.Model;
using Pixeval.Options;
using Pixeval.Util.ComponentModels;
using Pixeval.Util.UI;

namespace Pixeval.Pages.NovelViewer;

public partial class NovelViewerPageViewModel : DetailedUiObservableObject, IDisposable
{
    [ObservableProperty]
    private bool _isFullScreen;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="novelViewModels"></param>
    /// <param name="currentNovelIndex"></param>
    /// <param name="content"></param>
    public NovelViewerPageViewModel(IEnumerable<NovelItemViewModel> novelViewModels, int currentNovelIndex, FrameworkElement content) : base(content)
    {
        NovelsSource = novelViewModels.ToArray();
        CurrentNovelIndex = currentNovelIndex;

        InitializeCommands();
        FullScreenCommand.GetFullScreenCommand(false);
    }

    /// <summary>
    /// 当拥有DataProvider的时候调用这个构造函数，dispose的时候会自动dispose掉DataProvider
    /// </summary>
    /// <param name="viewModel"></param>
    /// <param name="currentNovelIndex"></param>
    /// <param name="content"></param>
    /// <remarks>
    /// novels should contain only one item if the novel is a single
    /// otherwise it contains the entire manga data
    /// </remarks>
    public NovelViewerPageViewModel(NovelViewViewModel viewModel, int currentNovelIndex, FrameworkElement content) : base(content)
    {
        ViewModelSource = new NovelViewViewModel(viewModel);
        ViewModelSource.DataProvider.View.FilterChanged += (_, _) => CurrentNovelIndex = Novels.IndexOf(CurrentNovel);
        CurrentNovelIndex = currentNovelIndex;

        InitializeCommands();
    }

    private NovelViewViewModel? ViewModelSource { get; }

    public NovelItemViewModel[]? NovelsSource { get; }

    public long NovelId => CurrentNovel.Entry.Id;

    public UserInfo Illustrator => CurrentNovel.Entry.User;

    public void Dispose()
    {
        foreach (var novelViewModel in Novels)
            novelViewModel.UnloadThumbnail(this);
        ViewModelSource?.Dispose();
    }

    public NavigationViewTag[] Tags =>
    [
        NovelInfoTag,
        CommentsTag
    ];

    public NavigationViewTag<WorkInfoPage, Novel> NovelInfoTag { get; } =
        new(null!) { Content = EntryViewerPageResources.InfoTabContent };

    public NavigationViewTag<CommentsPage, (CommentType, long Id)> CommentsTag { get; } =
        new(default) { Content = EntryViewerPageResources.CommentsTabContent };

    #region Current相关

    /// <summary>
    /// 插画列表
    /// </summary>
    public IList<NovelItemViewModel> Novels => ViewModelSource?.DataProvider.View ?? (IList<NovelItemViewModel>)NovelsSource!;

    /// <summary>
    /// 当前插画
    /// </summary>
    public NovelItemViewModel CurrentNovel => Novels[CurrentNovelIndex];

    /// <summary>
    /// 当前插画的索引
    /// </summary>
    public int CurrentNovelIndex
    {
        get => _currentNovelIndex;
        set
        {
            if (value is -1)
                return;
            if (value == _currentNovelIndex)
                return;

            var oldValue = _currentNovelIndex;
            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract

            _currentNovelIndex = value;

            NovelInfoTag.Parameter = CurrentNovel.Entry;
            CommentsTag.Parameter = (CommentType.Novel, NovelId);

            OnDetailedPropertyChanged(oldValue, value);
            OnPropertyChanged(nameof(CurrentNovel));
        }
    }

    /// <inheritdoc cref="CurrentNovelIndex"/>
    private int _currentNovelIndex = -1;

    #endregion

    #region Commands

    private void InitializeCommands()
    {
        FullScreenCommand.ExecuteRequested += FullScreenCommandOnExecuteRequested;
    }

    private void FullScreenCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        IsFullScreen = !IsFullScreen;
        FullScreenCommand.GetFullScreenCommand(IsFullScreen);
    }

    public XamlUICommand InfoAndCommentsCommand { get; } =
        EntryViewerPageResources.InfoAndComments.GetCommand(FontIconSymbol.InfoE946, VirtualKey.F12);

    public XamlUICommand FullScreenCommand { get; } = "".GetCommand(FontIconSymbol.FullScreenE740);

    #endregion
}
