#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/RelatedWorksPage.xaml.cs
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

using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Messages;
using Pixeval.Options;
using Pixeval.Util;
using WinUI3Utilities;

namespace Pixeval.Pages.IllustrationViewer;

public sealed partial class RelatedWorksPage
{
    public ThumbnailDirection ThumbnailDirection => App.AppViewModel.AppSetting.ThumbnailDirection;

    private string? _illustrationId;

    public RelatedWorksPage()
    {
        InitializeComponent();
    }

    public override void OnPageActivated(NavigationEventArgs e)
    {
        // Dispose current page contents if the parent page (IllustrationViewerPage) is navigating
        _ = WeakReferenceMessenger.Default.TryRegister<RelatedWorksPage, NavigatingFromIllustrationViewerMessage>(this, (recipient, _) =>
        {
            recipient.RelatedWorksIllustrationGrid.ViewModel.Dispose();
            WeakReferenceMessenger.Default.UnregisterAll(this);
        });
        _illustrationId = e.Parameter.To<string>();
        RelatedWorksIllustrationGrid.ViewModel.ResetEngine(App.AppViewModel.MakoClient.RelatedWorks(_illustrationId));
    }
}
