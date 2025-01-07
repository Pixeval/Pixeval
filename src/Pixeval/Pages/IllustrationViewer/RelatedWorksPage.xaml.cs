// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Microsoft.UI.Xaml.Navigation;
using Pixeval.Options;
using WinUI3Utilities;

namespace Pixeval.Pages.IllustrationViewer;

public sealed partial class RelatedWorksPage
{
    private long _illustrationId;

    public RelatedWorksPage() => InitializeComponent();

    public ThumbnailDirection ThumbnailDirection => App.AppViewModel.AppSettings.ThumbnailDirection;

    public override void OnPageActivated(NavigationEventArgs e)
    {
        _illustrationId = e.Parameter.To<long>();
        RelatedWorksIllustrationGrid.ResetEngine(App.AppViewModel.MakoClient.RelatedWorks(_illustrationId));
    }
}
