#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/RelatedWorksPage.xaml.cs
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

using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Messages;
using Pixeval.UserControls;
using Pixeval.Util;

namespace Pixeval.Pages.IllustrationViewer;

public sealed partial class RelatedWorksPage
{
    private IllustrationViewerPageViewModel? _illustrationViewerPageViewModel;

    public RelatedWorksPage()
    {
        InitializeComponent();
    }

    public override async void OnPageActivated(NavigationEventArgs e)
    {
        // Dispose current page contents if the parent page (IllustrationViewerPage) is navigating
        WeakReferenceMessenger.Default.TryRegister<RelatedWorksPage, NavigatingFromIllustrationViewerMessage>(this, (recipient, _) =>
        {
            recipient.RelatedWorksIllustrationGrid.ViewModel.Dispose();
            WeakReferenceMessenger.Default.UnregisterAll(this);
        });
        if (_illustrationViewerPageViewModel is null)
        {
            _illustrationViewerPageViewModel = e.Parameter as IllustrationViewerPageViewModel;
            await RelatedWorksIllustrationGrid.ViewModel.ResetEngineAndFillAsync(App.AppViewModel.MakoClient.RelatedWorks(_illustrationViewerPageViewModel!.IllustrationId));
        }
    }

    private void RelatedWorksIllustrationGrid_OnItemTapped(object? sender, IllustrationViewModel e)
    {
        IllustrationViewerPage.NavigatingStackEntriesFromRelatedWorksStack.Push((_illustrationViewerPageViewModel!.IllustrationId, _illustrationViewerPageViewModel.IsManga ? _illustrationViewerPageViewModel.CurrentIndex : null));
    }
}