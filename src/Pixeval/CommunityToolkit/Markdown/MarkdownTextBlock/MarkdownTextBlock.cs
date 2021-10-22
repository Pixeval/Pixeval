#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/MarkdownTextBlock.cs
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

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.CommunityToolkit.Markdown.Render;

namespace Pixeval.CommunityToolkit.Markdown.MarkdownTextBlock
{
    /// <summary>
    ///     An efficient and extensible control that can parse and render markdown.
    /// </summary>
    public partial class MarkdownTextBlock : Control, ILinkRegister, IImageResolver, ICodeBlockResolver
    {
        private long _backgroundPropertyToken;
        private long _borderBrushPropertyToken;
        private long _borderThicknessPropertyToken;
        private long _characterSpacingPropertyToken;
        private long _flowDirectionPropertyToken;
        private long _fontFamilyPropertyToken;
        private long _fontSizePropertyToken;
        private long _fontStretchPropertyToken;
        private long _fontStylePropertyToken;
        private long _fontWeightPropertyToken;
        private long _foregroundPropertyToken;
        private long _paddingPropertyToken;
        private long _requestedThemePropertyToken;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MarkdownTextBlock" /> class.
        /// </summary>
        public MarkdownTextBlock()
        {
            // Set our style.
            DefaultStyleKey = typeof(MarkdownTextBlock);

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void ThemeListener_ThemeChanged(ThemeListener sender)
        {
            RenderMarkdown();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            RegisterThemeChangedHandler();
            HookListeners();

            // Register for property callbacks that are owned by our parent class.
            _fontSizePropertyToken = RegisterPropertyChangedCallback(FontSizeProperty, OnPropertyChanged);
            _flowDirectionPropertyToken = RegisterPropertyChangedCallback(FlowDirectionProperty, OnPropertyChanged);
            _backgroundPropertyToken = RegisterPropertyChangedCallback(BackgroundProperty, OnPropertyChanged);
            _borderBrushPropertyToken = RegisterPropertyChangedCallback(BorderBrushProperty, OnPropertyChanged);
            _borderThicknessPropertyToken = RegisterPropertyChangedCallback(BorderThicknessProperty, OnPropertyChanged);
            _characterSpacingPropertyToken = RegisterPropertyChangedCallback(CharacterSpacingProperty, OnPropertyChanged);
            _fontFamilyPropertyToken = RegisterPropertyChangedCallback(FontFamilyProperty, OnPropertyChanged);
            _fontStretchPropertyToken = RegisterPropertyChangedCallback(FontStretchProperty, OnPropertyChanged);
            _fontStylePropertyToken = RegisterPropertyChangedCallback(FontStyleProperty, OnPropertyChanged);
            _fontWeightPropertyToken = RegisterPropertyChangedCallback(FontWeightProperty, OnPropertyChanged);
            _foregroundPropertyToken = RegisterPropertyChangedCallback(ForegroundProperty, OnPropertyChanged);
            _paddingPropertyToken = RegisterPropertyChangedCallback(PaddingProperty, OnPropertyChanged);
            _requestedThemePropertyToken = RegisterPropertyChangedCallback(RequestedThemeProperty, OnPropertyChanged);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (themeListener != null)
            {
                UnhookListeners();
                themeListener.ThemeChanged -= ThemeListener_ThemeChanged;
                themeListener.Dispose();
                themeListener = null;
            }

            // UnRegister property callbacks
            UnregisterPropertyChangedCallback(FontSizeProperty, _fontSizePropertyToken);
            UnregisterPropertyChangedCallback(FlowDirectionProperty, _flowDirectionPropertyToken);
            UnregisterPropertyChangedCallback(BackgroundProperty, _backgroundPropertyToken);
            UnregisterPropertyChangedCallback(BorderBrushProperty, _borderBrushPropertyToken);
            UnregisterPropertyChangedCallback(BorderThicknessProperty, _borderThicknessPropertyToken);
            UnregisterPropertyChangedCallback(CharacterSpacingProperty, _characterSpacingPropertyToken);
            UnregisterPropertyChangedCallback(FontFamilyProperty, _fontFamilyPropertyToken);
            UnregisterPropertyChangedCallback(FontStretchProperty, _fontStretchPropertyToken);
            UnregisterPropertyChangedCallback(FontStyleProperty, _fontStylePropertyToken);
            UnregisterPropertyChangedCallback(FontWeightProperty, _fontWeightPropertyToken);
            UnregisterPropertyChangedCallback(ForegroundProperty, _foregroundPropertyToken);
            UnregisterPropertyChangedCallback(PaddingProperty, _paddingPropertyToken);
            UnregisterPropertyChangedCallback(RequestedThemeProperty, _requestedThemePropertyToken);
        }

        /// <inheritdoc />
        protected override void OnApplyTemplate()
        {
            RegisterThemeChangedHandler();

            // Grab our root
            _rootElement = GetTemplateChild("RootElement") as Border;

            // And make sure to render any markdown we have.
            RenderMarkdown();
        }

        private void RegisterThemeChangedHandler()
        {
            themeListener ??= new ThemeListener();
            themeListener.ThemeChanged -= ThemeListener_ThemeChanged;
            themeListener.ThemeChanged += ThemeListener_ThemeChanged;
        }
    }
}