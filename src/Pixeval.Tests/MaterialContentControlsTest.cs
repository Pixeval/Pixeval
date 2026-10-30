using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CommunityToolkit.Avalonia.Controls;
using FluentIcons.Common;
using Material3Expressive.Components;
using Material3Expressive.Foundation.Rendering;
using MaterialSymbols;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixeval.Controls;
using Pixeval.I18N;
using Pixeval.Utilities;
using Pixeval.Views.Settings;

namespace Pixeval.Tests;

[TestClass]
[DoNotParallelize]
public sealed class MaterialContentControlsTest
{
    [ClassInitialize]
    public static void Initialize(TestContext _)
    {
        I18NManager.CandidatePaths.Add(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Pixeval")));
        I18NManager.Register(new JsonMarkdownLangPlugin(), LanguageHelper.DefaultLanguage);
    }

    [TestMethod]
    [DataRow(false, 420)]
    [DataRow(true, 420)]
    [DataRow(false, 840)]
    [DataRow(true, 840)]
    public async Task CardsTabsAndSettingsActionsKeepTheirContentVisible(bool dark, int width)
    {
        await using var session = HeadlessUnitTestSession.StartNew(typeof(MaterialThemeIntegrationTest.MaterialTestApplication));
        await session.Dispatch(() =>
        {
            Application.Current!.RequestedThemeVariant = dark ? ThemeVariant.Dark : ThemeVariant.Light;
            var window = new Window { Width = width, Height = 720 };
            using var bitmap = new Bitmap(AssetLoader.Open(new Uri("avares://Pixeval/Assets/Platforms/pixiv.png")));
            var image = new Image { Source = bitmap, Stretch = Stretch.UniformToFill };
            var title = new TextBlock { Text = "ウマ娘プリティーダービー / A long trending tag", TextWrapping = TextWrapping.Wrap, TextTrimming = TextTrimming.CharacterEllipsis, MaxLines = 2 };
            DockPanel.SetDock(title, Dock.Top);
            var content = new DockPanel { VerticalSpacing = 8, Children = { title, new Border { ClipToBounds = true, Child = image } } };
            var card = new Card
            {
                Width = 220,
                Height = 200,
                Theme = (ControlTheme) window.FindResource("ContentCardTheme")!,
                Content = content
            };
            var invoked = 0;
            card.Click += (_, _) => invoked++;
            var tabs = new TabStrip
            {
                ItemsSource = new[]
                {
                    new IconText { Text = "插画", Symbol = Symbol.Image },
                    new IconText { Text = "小说", Symbol = Symbol.Book }
                },
                SelectedIndex = 0
            };
            var open = new IconButton { Symbol = MaterialSymbolCodepoint.OpenInNew };
            var dependency = new SettingsExpander { Header = "dotnet", Description = "CommunityToolkit", Content = open };
            var blocked = new BlockedUsersSettingsExpander { Header = "全局屏蔽用户", Description = "屏蔽的内容显示为占位内容", IsExpanded = true };
            var constrained = new IconText { Width = 120, HorizontalAlignment = HorizontalAlignment.Left, Text = "A very long label that should be ellipsized", Symbol = Symbol.Person };
            window.Content = new StackPanel { Spacing = 12, Margin = new Thickness(16), Children = { blocked, dependency, tabs, card, constrained } };
            try
            {
                window.Show();
                Render(window);
                Assert.IsTrue(card.IsClickable);
                Assert.IsTrue(card.CornerRadius.TopLeft <= 16);
                Assert.IsTrue(content.Bounds.Width >= 190, $"Card content was squeezed to {content.Bounds.Width}.");
                Assert.IsTrue(image.Bounds.Width >= 190 && image.Bounds.Height >= 120);
                Assert.IsTrue(title.Bounds.Height <= 2 * title.LineHeight);
                var label = constrained.GetVisualDescendants().OfType<TextBlock>().Single();
                Assert.IsTrue(label.Bounds.Width <= constrained.Bounds.Width - constrained.IconSize);
                foreach (var tab in tabs.GetVisualDescendants().OfType<TabStripItem>())
                {
                    var text = tab.GetVisualDescendants().OfType<TextBlock>().Single();
                    Assert.AreEqual(14, text.FontSize);
                    Assert.AreEqual(20, text.LineHeight);
                    Assert.IsTrue(text.Bounds.Height >= text.LineHeight);
                }
                foreach (var button in blocked.GetVisualDescendants().OfType<IconButton>().Append(open))
                {
                    Assert.AreEqual(48, button.Bounds.Width);
                    Assert.AreEqual(48, button.Bounds.Height);
                    var symbol = button.GetVisualDescendants().OfType<MaterialSymbol>().Single();
                    Assert.IsTrue(symbol.Bounds.Width >= 24 && symbol.Bounds.Height >= 24);
                }
                Assert.IsFalse(blocked.Items.OfType<SettingsCard>().Last().IsVisible);
                Assert.IsTrue(card.Focus());
                window.KeyPress(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, "\r");
                window.KeyRelease(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, "\r");
                Assert.AreEqual(1, invoked);
                var imageCorner = image.TranslatePoint(new Point(image.Bounds.Width - 4, image.Bounds.Height - 4), window)
                    ?? throw new AssertFailedException("The card image is detached from its window.");
                window.MouseMove(imageCorner);
                window.MouseDown(imageCorner, MouseButton.Left);
                window.MouseUp(imageCorner, MouseButton.Left);
                Assert.AreEqual(2, invoked, "The rectangular image area must remain clickable.");
            }
            finally
            {
                window.Close();
            }
        }, CancellationToken.None);
    }

    [TestMethod]
    public async Task FallbackTextAndInlineLinksDoNotInheritFixedMaterialGeometry()
    {
        await using var session = HeadlessUnitTestSession.StartNew(typeof(MaterialThemeIntegrationTest.MaterialTestApplication));
        await session.Dispatch(() =>
        {
            var large = new TextBlock { Text = "标题 Ag\n第二行", FontSize = 32 };
            var title = new TextBlock { Text = "选择图片\n或者", Theme = (ControlTheme) Application.Current!.FindResource("TitleTextBlockStyle")! };
            var link = new HyperlinkButton { Content = "5.0.13.0", Theme = (ControlTheme) Application.Current!.FindResource("InlineHyperlinkTheme")! };
            var badge = new PixevalBadge { Mode = BadgeMode.R18 };
            var card = new Card { Theme = (ControlTheme) Application.Current!.FindResource("ContentCardTheme")!, Content = title };
            var window = new Window { Width = 420, Height = 500, Content = new StackPanel { Children = { large, card, link, badge } } };
            try
            {
                window.Show();
                Render(window);
                Assert.IsTrue(double.IsNaN(large.LineHeight));
                Assert.IsTrue(large.Bounds.Height >= 64);
                Assert.AreEqual(22, title.FontSize);
                Assert.AreEqual(28, title.LineHeight);
                Assert.IsTrue(title.Bounds.Height >= 56);
                Assert.IsTrue(link.Bounds.Height < 40, "An inline version link must not reserve the Material button height.");
                var badgeText = badge.GetVisualDescendants().OfType<TextBlock>().Single();
                Assert.IsTrue(double.IsNaN(badgeText.LineHeight));
                Assert.IsTrue(badgeText.Bounds.Height < 20);
            }
            finally
            {
                window.Close();
            }
        }, CancellationToken.None);
    }

    private static void Render(Window window)
    {
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        AvaloniaHeadlessPlatform.ForceRenderTimerTick(1);
        using var frame = window.CaptureRenderedFrame();
        Assert.IsNotNull(frame);
    }

    [TestMethod]
    public async Task MaterialBaselineKeepsFixedLineHeightWhenOnlyFontSizeChanges()
    {
        await using var session = HeadlessUnitTestSession.StartNew(typeof(MaterialBaselineApplication));
        await session.Dispatch(() =>
        {
            var text = new TextBlock { FontSize = 32, Text = "标题 Ag\n第二行" };
            var window = new Window { Width = 420, Height = 300, Content = new StackPanel { Children = { text } } };
            try
            {
                window.Show();
                Render(window);
                Assert.AreEqual(24, text.LineHeight);
                var inheritedHeight = text.Bounds.Height;
                text.LineHeight = double.NaN;
                Render(window);
                Assert.IsTrue(text.Bounds.Height > inheritedHeight, "The inherited baseline constrains larger fallback text.");
            }
            finally
            {
                window.Close();
            }
        }, CancellationToken.None);
    }

    public sealed class MaterialBaselineApplication : Application
    {
        public override void Initialize()
        {
            Styles.Add(new Avalonia.Themes.Fluent.FluentTheme());
            Styles.Add(new Material3Expressive.Styles.MaterialTheme());
        }

        public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<MaterialBaselineApplication>()
            .UseSkia()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false, ShouldRenderOnUIThread = true })
            .WithPixevalFonts();
    }
}
