#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/IllustrationResultFilterPopupContent.xaml.cs
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
using System.Reflection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Pixeval.Misc;

namespace Pixeval.Popups.IllustrationResultFilter
{
    public sealed partial class IllustrationResultFilterPopupContent : ICompletableAppPopupContent
    {
        private readonly IllustrationResultFilterPopupViewModel _viewModel;

        private EventHandler<TappedRoutedEventArgs>? _closeButtonTapped;

        private EventHandler<TappedRoutedEventArgs>? _resetButtonTapped;

        public IllustrationResultFilterPopupContent(IllustrationResultFilterPopupViewModel viewModel)
        {
            _viewModel = viewModel;
            UniqueId = Guid.NewGuid();
            InitializeComponent();
        }

        public bool IsReset { get; private set; }

        public Guid UniqueId { get; }

        public FrameworkElement UIContent => this;

        public object GetCompletionResult()
        {
            return new FilterSettings(
                _viewModel.IncludeTags,
                _viewModel.ExcludeTags,
                _viewModel.LeastBookmark,
                _viewModel.MaximumBookmark,
                _viewModel.UserGroupName,
                _viewModel.IllustratorName,
                _viewModel.IllustratorId,
                _viewModel.IllustrationName,
                _viewModel.IllustrationId,
                _viewModel.PublishDateStart,
                _viewModel.PublishDateEnd);
        }

        public event EventHandler<TappedRoutedEventArgs> ResetButtonTapped
        {
            add => _resetButtonTapped += value;
            remove => _resetButtonTapped -= value;
        }

        public event EventHandler<TappedRoutedEventArgs> CloseButtonTapped
        {
            add => _closeButtonTapped += value;
            remove => _closeButtonTapped -= value;
        }

        private void CloseButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            _closeButtonTapped?.Invoke(this, e);
        }

        /// <summary>
        ///     Flush the bindings
        /// </summary>
        public void Cleanup()
        {
            Focus(FocusState.Programmatic);
        }

        private void ResetButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            IsReset = true;
            foreach (var propertyInfo in typeof(IllustrationResultFilterPopupViewModel).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                propertyInfo.SetValue(_viewModel, propertyInfo.GetDefaultValue());
            }

            _resetButtonTapped?.Invoke(this, e);
        }

        private void IllustrationResultFilterPopupContent_OnLoaded(object sender, RoutedEventArgs e)
        {
            // Focus the popup content so that the hot key for closing can work properly
            CloseButton.Focus(FocusState.Programmatic);
        }
    }
}