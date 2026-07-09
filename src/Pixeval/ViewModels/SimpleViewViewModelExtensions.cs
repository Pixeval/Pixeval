// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

namespace Pixeval.ViewModels;

public static class SimpleViewViewModelExtensions
{
    extension(ISimpleViewViewModel? viewModel)
    {
        public IWorkViewViewModel? CloneAsWorkViewModel() =>
            viewModel switch
            {
                IllustrationViewViewModel illustrationViewModel => new IllustrationViewViewModel(illustrationViewModel),
                NovelViewViewModel novelViewModel => new NovelViewViewModel(novelViewModel),
                _ => null
            };

        public UserViewViewModel? CloneAsUserViewModel() =>
            viewModel is UserViewViewModel userViewModel ? new(userViewModel) : null;

        public SpotlightViewViewModel? CloneAsSpotlightViewModel() =>
            viewModel is SpotlightViewViewModel spotlightViewModel ? new(spotlightViewModel) : null;
    }
}
