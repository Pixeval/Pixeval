using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CommunityToolkit.Avalonia.Controls;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using Material3Expressive.Components;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixeval.Controls;
using Pixeval.AppManagement.Settings;
using Pixeval.I18N;
using Pixeval.Models.Settings.Entries;
using Pixeval.Utilities;
using Pixeval.Views.Settings;

namespace Pixeval.Tests;

[TestClass]
[DoNotParallelize]
public sealed class MaterialWrapperControlsTest
{
    [ClassInitialize]
    public static void Initialize(TestContext _)
    {
        var projectPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Pixeval"));
        I18NManager.CandidatePaths.Add(projectPath);
        I18NManager.Register(new JsonMarkdownLangPlugin(), LanguageHelper.DefaultLanguage);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task SymbolSelectorKeepsCompactSelectionAndDescriptiveMenu(bool dark)
    {
        await using var session = HeadlessUnitTestSession.StartNew(typeof(MaterialThemeIntegrationTest.MaterialTestApplication));
        await session.Dispatch(() =>
        {
            Application.Current!.RequestedThemeVariant = dark ? ThemeVariant.Dark : ThemeVariant.Light;
            var selector = new SymbolComboBox
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                ItemsSource = new[]
                {
                    new SymbolComboBoxItem(1, "First option", Symbol.Image),
                    new SymbolComboBoxItem(2, "Second option", Symbol.Book)
                },
                SelectedValue = 2
            };
            var window = new Window { Width = 500, Height = 400, Content = new StackPanel { Children = { selector } } };
            try
            {
                window.Show();
                Render(window);
                var inner = selector.GetVisualDescendants().OfType<ComboBox>().Single();
                Assert.AreEqual(1, inner.SelectedIndex);
                Assert.AreEqual(1, selector.SelectedIndex);
                Assert.AreSame(selector.IconSelectionBoxTemplate, inner.SelectionBoxItemTemplate);
                Assert.AreSame(selector.IconItemTemplate, inner.ItemTemplate);
                Assert.AreEqual(Symbol.Book, inner.GetVisualDescendants().OfType<SymbolIcon>().Single().Symbol);
                Assert.AreEqual("Second option", AutomationProperties.GetName(inner));
                Assert.IsTrue(selector.Bounds.Width is >= 80 and <= 120, $"Unexpected toolbar width: {selector.Bounds.Width}");

                inner.IsDropDownOpen = true;
                Render(window);
                var item = Assert.IsInstanceOfType<ComboBoxItem>(inner.ContainerFromIndex(0));
                Assert.AreEqual("First option", item.GetVisualDescendants().OfType<IconText>().Single().Text);
                inner.SelectedIndex = 0;
                inner.IsDropDownOpen = false;
                Render(window);
                Assert.AreEqual(1, selector.SelectedValue);
                Assert.AreEqual(Symbol.Image, inner.GetVisualDescendants().OfType<SymbolIcon>().Single().Symbol);

                selector.IconMode = false;
                Render(window);
                Assert.AreSame(selector.TextItemTemplate, inner.ItemTemplate);
                Assert.AreSame(selector.TextItemTemplate, inner.SelectionBoxItemTemplate);
                Assert.IsTrue(inner.GetVisualDescendants().OfType<TextBlock>().Any(text => text.Text is "First option"));
                Assert.IsFalse(inner.GetVisualDescendants().OfType<SymbolIcon>().Any());
                Assert.IsTrue(selector.Bounds.Width >= 160);

                selector.SelectedValue = 2;
                Render(window);
                selector.SelectedIndex = 0;
                Render(window);
                Assert.AreEqual(1, selector.SelectedValue);
                Assert.AreEqual(0, inner.SelectedIndex);
                selector.IconMode = true;
                Render(window);
                Assert.AreEqual(Symbol.Image, inner.GetVisualDescendants().OfType<SymbolIcon>().Single().Symbol);
                Assert.IsTrue(selector.Bounds.Width <= 120);
            }
            finally
            {
                window.Close();
            }
        }, CancellationToken.None);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task IpEditorUsesMaterialValidationAndDisabledColors(bool dark)
    {
        await using var session = HeadlessUnitTestSession.StartNew(typeof(MaterialThemeIntegrationTest.MaterialTestApplication));
        await session.Dispatch(() =>
        {
            Application.Current!.RequestedThemeVariant = dark ? ThemeVariant.Dark : ThemeVariant.Light;
            var address = new IPBox { Text = "192.168.1.1", Width = 320 };
            var reference = new TextBox { Text = "Reference", Width = 320 };
            var window = new Window { Width = 500, Height = 400, Content = new StackPanel { Children = { address, reference } } };
            try
            {
                window.Show();
                Render(window);
                var input = address.GetVisualDescendants().OfType<TextBox>().Single();
                Assert.AreEqual(ColorOf(reference.Background), ColorOf(input.Background));
                Assert.AreEqual(reference.Padding, input.Padding);
                Assert.AreEqual(reference.CornerRadius, input.CornerRadius);
                Assert.AreEqual(IPAddress.Parse("192.168.1.1"), address.IPAddress);
                Assert.IsFalse(MaterialTextField.GetIsError(input));
                var normalBorder = ColorOf(input.BorderBrush);

                input.Text = "invalid";
                Render(window);
                Assert.IsFalse(address.IsValid);
                Assert.IsNull(address.IPAddress);
                Assert.IsTrue(MaterialTextField.GetIsError(input));
                Assert.AreNotEqual(normalBorder, ColorOf(input.BorderBrush));

                address.IsEnabled = reference.IsEnabled = false;
                Render(window);
                Assert.AreEqual(ColorOf(reference.Background), ColorOf(input.Background));
                Assert.AreEqual(ColorOf(reference.Foreground), ColorOf(input.Foreground));
                Assert.AreEqual(ColorOf(reference.BorderBrush), ColorOf(input.BorderBrush));

                address.IsEnabled = true;
                input.Text = "2001:db8::1";
                Render(window);
                Assert.IsTrue(address.IsValid);
                Assert.IsFalse(MaterialTextField.GetIsError(input));
                Assert.AreEqual(IPAddress.Parse("2001:db8::1"), address.IPAddress);
            }
            finally
            {
                window.Close();
            }
        }, CancellationToken.None);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task CompactWrappersKeepContentAndThemeColors(bool dark)
    {
        await using var session = HeadlessUnitTestSession.StartNew(typeof(MaterialThemeIntegrationTest.MaterialTestApplication));
        await session.Dispatch(() =>
        {
            Application.Current!.RequestedThemeVariant = dark ? ThemeVariant.Dark : ThemeVariant.Light;
            var items = new ObservableCollection<string> { "First", "Second" };
            var tokens = new TokenizingBox { ItemsSource = items, Width = 320, PlaceholderText = "Add another item" };
            var heart = new HeartButton { Width = 30, Height = 30 };
            var slider = new PagePreviewSlider { ItemsSource = new object[] { "One", "Two", "Three" }, Width = 320 };
            var page = new PageButton { ToolTipText = "Next", Height = 100 };
            var window = new Window { Width = 500, Height = 400, Content = new StackPanel { Children = { tokens, heart, slider, page } } };
            try
            {
                window.Show();
                Render(window);
                Assert.IsTrue(tokens.Bounds.Height <= 56, $"Token input grew to {tokens.Bounds.Height}.");
                var input = tokens.GetVisualDescendants().OfType<TextBox>().Single();
                Assert.IsTrue(input.Bounds.Height < 40);
                var remove = tokens.GetVisualDescendants().OfType<Button>().First(button => button.Classes.Contains("tokenizing-token-remove"));
                Assert.AreEqual(24, remove.Bounds.Width);
                var editorScroll = input.GetVisualDescendants().OfType<ScrollViewer>().Single();
                Assert.IsTrue(editorScroll.Extent.Height <= editorScroll.Viewport.Height);
                Assert.IsFalse(input.GetVisualDescendants().OfType<ScrollBar>().Any(bar => bar.IsVisible && bar.Orientation is Orientation.Vertical));
                Assert.IsTrue(input.Focus());
                window.KeyTextInput("A long token that scrolls horizontally while editing");
                Render(window);
                Assert.AreEqual(input.Text, tokens.Text);
                Assert.IsTrue(editorScroll.Extent.Height <= editorScroll.Viewport.Height);
                remove.Command!.Execute(remove.CommandParameter);
                Render(window);
                Assert.AreEqual(1, items.Count);
                Assert.AreEqual(1, tokens.TokenCount);
                input.Focus();
                Render(window);
                var root = tokens.GetVisualDescendants().OfType<Border>().Single(border => border.Name is "PART_Root");
                Assert.AreEqual(ResourceColor(tokens, "AccentFillColorDefaultBrush"), ColorOf(root.BorderBrush));

                Assert.AreEqual(30, heart.Bounds.Width);
                Assert.AreEqual(30, heart.Bounds.Height);
                Assert.AreEqual(0, ColorOf(heart.Background).A);
                Assert.AreEqual(2, heart.GetVisualDescendants().OfType<WaveProgressHeart>().Count());
                Assert.AreEqual(ResourceColor(page, "TextFillColorPrimaryBrush"), ColorOf(page.Foreground));
                Assert.AreEqual(100, page.Bounds.Width);

                var selected = slider.GetVisualDescendants().OfType<Border>().Single(border => border.Name is "PART_SelectedTrack");
                var previousWidth = selected.Bounds.Width;
                slider.SelectedIndex = 2;
                Render(window);
                Assert.IsTrue(selected.Bounds.Width > previousWidth);
                Assert.AreEqual(ResourceColor(slider, "AccentFillColorDefaultBrush"), ColorOf(selected.Background));

                Application.Current.RequestedThemeVariant = dark ? ThemeVariant.Light : ThemeVariant.Dark;
                Render(window);
                Assert.AreEqual(ResourceColor(tokens, "ControlFillColorSecondaryBrush"), ColorOf(root.Background));
                Assert.AreEqual(ResourceColor(slider, "AccentFillColorDefaultBrush"), ColorOf(selected.Background));
            }
            finally
            {
                window.Close();
            }
        }, CancellationToken.None);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task ProfileHeaderAndTypographyUseMaterialSurfacesAndLineHeights(bool dark)
    {
        await using var session = HeadlessUnitTestSession.StartNew(typeof(MaterialThemeIntegrationTest.MaterialTestApplication));
        await session.Dispatch(() =>
        {
            Application.Current!.RequestedThemeVariant = dark ? ThemeVariant.Dark : ThemeVariant.Light;
            var window = new Window { Width = 500, Height = 500 };
            var title = new TextBlock { Text = "用户名 / Navigation", Theme = (ControlTheme) window.FindResource("TitleTextBlockStyle")! };
            var subtitle = new TextBlock { Text = "用户名", Theme = (ControlTheme) window.FindResource("SubtitleTextBlockStyle")! };
            var expander = new Expander
            {
                Header = title,
                Content = subtitle,
                ExpandDirection = ExpandDirection.Up,
                Background = Brushes.Magenta,
                Theme = (ControlTheme) window.FindResource("UserInfoExpanderTheme")!
            };
            var navigation = new NavigationPage { Height = 100 };
            window.Content = new StackPanel { Children = { expander, navigation } };
            try
            {
                window.Show();
                Render(window);
                var header = expander.GetVisualDescendants().OfType<ToggleButton>().Single(button => button.Name is "ExpanderHeader");
                Assert.AreEqual(ResourceColor(expander, "CardBackgroundFillColorDefaultBrush"), ColorOf(header.Background));
                Assert.AreEqual(255, ColorOf(header.Background).A);
                Assert.AreEqual(22, title.FontSize);
                Assert.AreEqual(28, title.LineHeight);
                Assert.IsTrue(title.Bounds.Height >= title.LineHeight);
                var navigationHeader = navigation.GetVisualDescendants().OfType<ContentPresenter>().Single(presenter => presenter.Name is "PART_Header");
                Assert.AreEqual(22, navigationHeader.FontSize);
                Assert.AreEqual(28, navigationHeader.GetValue(TextBlock.LineHeightProperty));
                expander.IsExpanded = true;
                Render(window);
                Assert.IsTrue(header.IsChecked);
                Assert.AreEqual(16, subtitle.FontSize);
                Assert.AreEqual(24, subtitle.LineHeight);
                Application.Current.RequestedThemeVariant = dark ? ThemeVariant.Light : ThemeVariant.Dark;
                Render(window);
                Assert.AreEqual(ResourceColor(expander, "CardBackgroundFillColorDefaultBrush"), ColorOf(header.Background));
                Assert.AreEqual(ResourceColor(expander, "AccentFillColorDefaultBrush"), ResourceColor(expander, "SystemFillColorAttentionBrush"));
            }
            finally
            {
                window.Close();
            }
        }, CancellationToken.None);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task IpListHasReadableRowsAndSingleClickRemoval(bool dark)
    {
        await using var session = HeadlessUnitTestSession.StartNew(typeof(MaterialThemeIntegrationTest.MaterialTestApplication));
        await session.Dispatch(() =>
        {
            Application.Current!.RequestedThemeVariant = dark ? ThemeVariant.Dark : ThemeVariant.Light;
            var settings = new PixivDomainFrontingSettings { PixivAppApiNameResolver = ["104.18.42.239", "172.64.145.17"] };
            var addresses = new IPListInput { Entry = new IPSetSettingsEntry<PixivDomainFrontingSettings>(settings, value => value.PixivAppApiNameResolver), Width = 360 };
            var window = new Window { Width = 500, Height = 320, Content = addresses };
            try
            {
                window.Show();
                Render(window);
                var remove = addresses.GetVisualDescendants().OfType<Button>().Single(button => button.DataContext is "104.18.42.239");
                Assert.IsTrue(remove.IsEnabled);
                remove.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Render(window);
                CollectionAssert.AreEqual(new[] { "172.64.145.17" }, settings.PixivAppApiNameResolver);
                var remaining = addresses.GetVisualDescendants().OfType<TextBlock>().Single(text => text.Text is "172.64.145.17");
                Assert.AreEqual(ResourceColor(addresses, "TextFillColorPrimaryBrush"), ColorOf(remaining.Foreground));
            }
            finally
            {
                window.Close();
            }
        }, CancellationToken.None);
    }

    private static Color ResourceColor(Control control, string key)
    {
        Assert.IsTrue(control.TryFindResource(key, control.ActualThemeVariant, out var resource));
        return ColorOf(Assert.IsInstanceOfType<IBrush>(resource));
    }

    private static Color ColorOf(IBrush? brush) => Assert.IsInstanceOfType<ISolidColorBrush>(brush).Color;

    private static void Render(Window window)
    {
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        AvaloniaHeadlessPlatform.ForceRenderTimerTick(1);
        using var frame = window.CaptureRenderedFrame();
        Assert.IsNotNull(frame);
    }
}
