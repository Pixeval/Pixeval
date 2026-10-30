using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CommunityToolkit.Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using ComposeAnimation;
using Material3Expressive.Components;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixeval.Controls;
using Pixeval.Utilities;
using Pixeval.Views.Work;
using SmoothScroll.Avalonia.Controls;

namespace Pixeval.Tests;

[TestClass]
[DoNotParallelize]
public sealed class MaterialThemeIntegrationTest
{
    [TestMethod]
    public async Task BasicControlsRenderAndUpdateAcrossThemeChanges()
    {
        await using var session = HeadlessUnitTestSession.StartNew(typeof(MaterialTestApplication));
        await session.Dispatch(() =>
        {
            var window = CreateWindow(out var animationFailures);
            var button = new Button { Content = "Save" };
            var textBox = new TextBox { Text = "Original", Width = 320 };
            var checkBox = new CheckBox { Content = "Enabled", IsChecked = true };
            var toggleSwitch = new ToggleSwitch { Content = "Enabled", IsChecked = true };
            var slider = new Slider { Minimum = 0, Maximum = 100, Value = 25, Width = 320 };
            var progress = new ProgressBar { Minimum = 0, Maximum = 100, Value = 25, Width = 320 };
            var comboBox = new ComboBox { ItemsSource = new[] { "First", "Second" }, SelectedIndex = 0, Width = 320 };
            TemplatedControl[] controls = [button, textBox, checkBox, toggleSwitch, slider, progress, comboBox];
            var panel = new StackPanel { Margin = new Thickness(24), Spacing = 8 };
            foreach (var control in controls)
                panel.Children.Add(control);
            window.Content = panel;
            try
            {
                Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
                window.Show();
                Render(window);
                foreach (var control in controls)
                {
                    Assert.IsNotNull(control.Template);
                    Assert.IsTrue(control.Bounds.Width > 0 && control.Bounds.Height > 0,
                        $"{control.GetType().Name} has no usable layout.");
                }
                Assert.IsTrue(button.GetVisualDescendants().OfType<MaterialSurface>().Any());
                Assert.IsTrue(progress.GetVisualDescendants().OfType<LinearProgressIndicator>().Any());
                Assert.IsTrue(textBox.GetVisualDescendants().OfType<Control>().Any(control => control.Name is "PART_Decoration"));

                var lightForeground = GetColor(button.Foreground);
                var lightSurface = GetColor(window.Background);
                var lightCard = GetResourceColor(window, "CardBackgroundFillColorDefaultBrush");
                Application.Current.RequestedThemeVariant = ThemeVariant.Dark;
                Render(window);
                Assert.AreNotEqual(lightForeground, GetColor(button.Foreground));
                Assert.AreNotEqual(lightSurface, GetColor(window.Background));
                Assert.AreNotEqual(lightCard, GetResourceColor(window, "CardBackgroundFillColorDefaultBrush"));

                Assert.IsTrue(textBox.Focus());
                textBox.SelectAll();
                window.KeyTextInput("中文 input");
                Assert.AreEqual("中文 input", textBox.Text);
                comboBox.SelectedIndex = 1;
                Assert.AreEqual("Second", comboBox.SelectedItem);
                progress.Value = slider.Value = 75;
                Render(window);
                Assert.AreEqual(0.75, progress.GetVisualDescendants().OfType<LinearProgressIndicator>().Single().Progress, 0.0001);
                Assert.IsEmpty(animationFailures);
            }
            finally
            {
                window.Close();
            }
        }, CancellationToken.None);
    }

    [TestMethod]
    public async Task ApplicationFontChangesReachMaterialTemplateContent()
    {
        await using var session = HeadlessUnitTestSession.StartNew(typeof(MaterialTestApplication));
        await session.Dispatch(() =>
        {
            var window = CreateWindow(out var animationFailures);
            var button = new Button { Content = "字体 Font" };
            var textBox = new TextBox { Text = "字体 Font", Width = 320 };
            window.Content = new StackPanel { Children = { button, textBox } };
            try
            {
                window.Show();
                Render(window);
                App.ApplyAppFontFamily(["fonts:PixevalCjk#Noto Sans SC"]);
                Render(window);
                var expected = (FontFamily) Application.Current!.Resources["ContentControlThemeFontFamily"]!;
                var label = button.GetVisualDescendants().OfType<ContentPresenter>()
                    .Single(presenter => presenter.Name is "PART_ContentPresenter");
                var editor = textBox.GetVisualDescendants().OfType<TextPresenter>().Single();
                Assert.AreEqual(expected, label.FontFamily);
                Assert.AreEqual(expected, editor.FontFamily);
                Assert.IsEmpty(animationFailures);
            }
            finally
            {
                window.Close();
            }
        }, CancellationToken.None);
    }

    [TestMethod]
    public async Task WindowInitializesMotionBeforeItsContentNeedsAnimation()
    {
        await using var session = HeadlessUnitTestSession.StartNew(typeof(MaterialTestApplication));
        await session.Dispatch(() =>
        {
            var button = new Button { Content = "Open" };
            var window = new Window
            {
                Width = 480,
                Height = 320,
                Content = new StackPanel { Children = { button, new TextBox { Text = "Input" } } }
            };
            try
            {
                window.Show();
                var scope = AnimationScope.GetRoot(window);
                var animationFailures = new List<Exception>();
                scope.AnimationFailed += (_, args) => animationFailures.Add(args.Exception);
                Render(window);
                Assert.IsTrue(button.Focus());
                window.KeyPress(Key.Space, RawInputModifiers.None, PhysicalKey.Space, " ");
                Render(window);
                window.KeyRelease(Key.Space, RawInputModifiers.None, PhysicalKey.Space, " ");
                Render(window);
                Assert.IsEmpty(animationFailures);
            }
            finally
            {
                window.Close();
            }
        }, CancellationToken.None);
    }

    [TestMethod]
    public async Task SettingsCardsRespectLocalDarkThemeWithinLightApplication()
    {
        await using var session = HeadlessUnitTestSession.StartNew(typeof(MaterialTestApplication));
        await session.Dispatch(() =>
        {
            var window = CreateWindow(out var animationFailures);
            Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
            var lightToggle = new ToggleSwitch();
            var darkToggle = new ToggleSwitch();
            var lightCard = new SettingsCard { Header = "Setting", Description = "Description", Content = lightToggle };
            var darkCard = new SettingsCard { Header = "Setting", Description = "Description", Content = darkToggle };
            window.Content = new StackPanel
            {
                Children =
                {
                    lightCard,
                    new ThemeVariantScope { RequestedThemeVariant = ThemeVariant.Dark, Child = darkCard }
                }
            };
            try
            {
                window.Show();
                Render(window);
                Assert.IsNotNull(lightCard.Template);
                Assert.IsNotNull(darkCard.Template);
                Assert.IsTrue(lightToggle.Bounds.Height > 0 && darkToggle.Bounds.Height > 0);
                Assert.IsTrue(darkToggle.GetVisualDescendants().OfType<MaterialSurface>().Any());
                var darkBackground = GetColor(darkCard.Background);
                var darkForeground = GetColor(darkCard.Foreground);
                Assert.AreNotEqual(GetColor(lightCard.Background), darkBackground);
                Assert.AreNotEqual(GetColor(lightCard.Foreground), darkForeground);
                Assert.AreNotEqual((byte) 0, darkBackground.A);

                Assert.IsTrue(darkToggle.Focus());
                window.KeyPress(Key.Space, RawInputModifiers.None, PhysicalKey.Space, " ");
                window.KeyRelease(Key.Space, RawInputModifiers.None, PhysicalKey.Space, " ");
                Assert.IsTrue(darkToggle.IsChecked);
                Application.Current.RequestedThemeVariant = ThemeVariant.Dark;
                Render(window);
                Assert.AreEqual(darkBackground, GetColor(lightCard.Background));
                Assert.AreEqual(darkForeground, GetColor(lightCard.Foreground));
                Assert.AreEqual(darkBackground, GetColor(darkCard.Background));
                Assert.IsEmpty(animationFailures);
            }
            finally
            {
                window.Close();
            }
        }, CancellationToken.None);
    }

    [TestMethod]
    public async Task CommandBarCommandsAndOverflowRemainInteractive()
    {
        await using var session = HeadlessUnitTestSession.StartNew(typeof(MaterialTestApplication));
        await session.Dispatch(() =>
        {
            var window = CreateWindow(out var animationFailures);
            var executions = 0;
            var primary = new CommandBarButton
            {
                Label = "Save",
                Icon = new Border { Width = 16, Height = 16, Background = Brushes.Blue },
                Command = new RelayCommand(() => executions++)
            };
            var toggle = new CommandBarToggleButton { Label = "Pin", IsChecked = false };
            var secondary = new CommandBarButton
            {
                Label = "Export",
                Icon = new Border { Width = 16, Height = 16, Background = Brushes.Green },
                Command = new RelayCommand(() => executions++)
            };
            var bar = new CommandBar
            {
                DefaultLabelPosition = CommandBarDefaultLabelPosition.Right,
                PrimaryCommands = [primary, toggle],
                SecondaryCommands = [secondary]
            };
            window.Content = new StackPanel { Children = { bar } };
            try
            {
                window.Show();
                Render(window);
                Assert.IsTrue(primary.GetVisualDescendants().OfType<MaterialSurface>().Any());
                Assert.IsTrue(primary.Focus());
                window.KeyPress(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, "\r");
                window.KeyRelease(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, "\r");
                Assert.AreEqual(1, executions);
                Assert.IsTrue(toggle.Focus());
                window.KeyPress(Key.Space, RawInputModifiers.None, PhysicalKey.Space, " ");
                window.KeyRelease(Key.Space, RawInputModifiers.None, PhysicalKey.Space, " ");
                Assert.IsTrue(toggle.IsChecked);

                bar.IsOpen = true;
                Render(window);
                Assert.IsTrue(secondary.IsInOverflow);
                var label = secondary.GetVisualDescendants().OfType<ContentPresenter>()
                    .Single(presenter => presenter.Name is "PART_Label");
                Assert.IsTrue(label.IsVisible && label.Bounds.Width > 0 && label.Bounds.Height > 0);
                var point = secondary.TranslatePoint(new Rect(secondary.Bounds.Size).Center, window)
                    ?? throw new AssertFailedException("The overflow command is detached from the window.");
                window.MouseMove(point);
                window.MouseDown(point, MouseButton.Left);
                window.MouseUp(point, MouseButton.Left);
                Assert.AreEqual(2, executions);
                bar.IsOpen = false;
                Render(window);
                Assert.IsEmpty(animationFailures);
            }
            finally
            {
                window.Close();
            }
        }, CancellationToken.None);
    }

    [TestMethod]
    public async Task ExistingCompactControlsKeepVisibleEditableContent()
    {
        await using var session = HeadlessUnitTestSession.StartNew(typeof(MaterialTestApplication));
        await session.Dispatch(() =>
        {
            var window = CreateWindow(out var animationFailures);
            var button = new Button
            {
                Content = "Artwork action",
                Theme = (ControlTheme) window.FindResource("TransparentButtonStyle")!
            };
            var tokens = new TokenizingBox { Width = 320 };
            window.Content = new StackPanel { Children = { button, tokens } };
            try
            {
                window.Show();
                Render(window);
                var buttonContent = button.GetVisualDescendants().OfType<ContentPresenter>()
                    .Single(presenter => presenter.Name is "PART_ContentPresenter");
                Assert.IsTrue(buttonContent.Bounds.Width > 0 && buttonContent.Bounds.Height > 0);
                var editor = tokens.GetVisualDescendants().OfType<TextBox>().Single();
                Assert.IsTrue(editor.Bounds.Height <= tokens.Bounds.Height);
                Assert.IsTrue(editor.Focus());
                window.KeyTextInput("tag");
                Assert.AreEqual("tag", tokens.Text);
                Assert.IsEmpty(animationFailures);
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
    public async Task ExistingDialogCompletesItsPrimaryAction(bool dark)
    {
        await using var session = HeadlessUnitTestSession.StartNew(typeof(MaterialTestApplication));
        await session.Dispatch(async () =>
        {
            var window = CreateWindow(out var animationFailures);
            Application.Current!.RequestedThemeVariant = dark ? ThemeVariant.Dark : ThemeVariant.Light;
            window.Content = new Border();
            try
            {
                window.Show();
                Render(window);
                var host = new ContentDialogHost(window);
                var dialog = new ContentDialog
                {
                    Title = "Existing dialog",
                    Content = "Keep Pixeval's dialog lifecycle.",
                    PrimaryButtonText = "Confirm",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Primary
                };
                var completion = dialog.ShowAsync(host);
                Render(window);
                Assert.IsTrue(host.IsOpen);
                Assert.IsTrue(dialog.Bounds.Width > 0 && dialog.Bounds.Height > 0);
                var primary = dialog.GetVisualDescendants().OfType<Button>()
                    .Single(candidate => candidate.Name is "PART_PrimaryButton");
                primary.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Assert.AreEqual(ContentDialogResult.Primary, await completion.WaitAsync(TimeSpan.FromSeconds(5)));
                Assert.IsFalse(host.IsOpen);
                Assert.IsEmpty(host.Dialogs);
                Assert.IsEmpty(animationFailures);
            }
            finally
            {
                window.Close();
            }
        }, CancellationToken.None);
    }

    [TestMethod]
    public async Task MaterialScrollViewerAndImageScrollViewRetainProgrammaticScrolling()
    {
        await using var session = HeadlessUnitTestSession.StartNew(typeof(MaterialTestApplication));
        await session.Dispatch(() =>
        {
            var window = CreateWindow(out var animationFailures);
            var nativeViewer = new ScrollViewer
            {
                Width = 280,
                Height = 240,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Content = new Border { Height = 1600, Background = Brushes.Blue }
            };
            var imageViewer = new ScrollView
            {
                Width = 280,
                Height = 240,
                IsZoomEnabled = true,
                Content = new Border { Width = 240, Height = 1600, Background = Brushes.Green }
            };
            window.Content = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children = { nativeViewer, imageViewer }
            };
            try
            {
                window.Show();
                Render(window);
                Assert.IsTrue(nativeViewer.GetVisualDescendants().OfType<ScrollViewerPresenter>().Any());
                Assert.IsNotNull(imageViewer.ScrollPresenter);
                Assert.IsTrue(nativeViewer.Extent.Height > nativeViewer.Viewport.Height);
                Assert.IsTrue(imageViewer.Extent.Height > imageViewer.Viewport.Height);

                nativeViewer.Offset = new Vector(0, 160);
                _ = imageViewer.ScrollTo(new Vector(0, 160), false);
                Render(window);
                Assert.AreEqual(160, nativeViewer.Offset.Y, 0.01);
                Assert.AreEqual(160, imageViewer.Offset.Y, 0.01);

                nativeViewer.Offset = default;
                _ = imageViewer.ScrollTo(default, false);
                Render(window);
                Assert.AreEqual(0, nativeViewer.Offset.Y, 0.01);
                Assert.AreEqual(0, imageViewer.Offset.Y, 0.01);
                Assert.IsEmpty(animationFailures);
            }
            finally
            {
                window.Close();
            }
        }, CancellationToken.None);
    }

    [TestMethod]
    public async Task MaterialWorkListScrollsAndRecyclesVisibleCards()
    {
        await using var session = HeadlessUnitTestSession.StartNew(typeof(MaterialTestApplication));
        await session.Dispatch(async () =>
        {
            var window = CreateWindow(out var animationFailures);
            window.Width = 400;
            window.Height = 240;
            var builds = 0;
            var items = Enumerable.Range(0, 100).Select(index => index.ToString()).ToArray();
            var list = new WorkListBox
            {
                ItemsSource = items,
                ItemsPanel = new FuncTemplate<Panel?>(() => new Pixeval.Controls.VirtualizingWrapPanel
                {
                    ItemWidth = 100,
                    ItemHeight = 60,
                    CacheLength = 0
                })
            };
            list.DataTemplates.Add(new FuncDataTemplate<string>((_, _) =>
            {
                builds++;
                return new WorkItem { Content = new TextBlock { [!TextBlock.TextProperty] = new Binding(".") } };
            }));
            window.Content = list;
            ScrollViewerPresenter? presenter = null;
            ScrollAnimationStartingEventArgs? scrollAnimation = null;
            try
            {
                window.Show();
                Render(window);
                presenter = list.GetVisualDescendants().OfType<ScrollViewerPresenter>().Single();
                presenter.ScrollAnimationStarting += (_, args) => scrollAnimation = args;
                await ScrollToAsync(80);
                await ScrollToAsync(0);
                var warmBuilds = builds;
                for (var i = 0; i < 3; i++)
                {
                    await ScrollToAsync(80);
                    await ScrollToAsync(0);
                }
                Assert.IsTrue(warmBuilds > 0);
                Assert.AreEqual(warmBuilds, builds);
                Assert.IsEmpty(animationFailures);
            }
            finally
            {
                list.ItemsSource = null;
                window.Close();
            }

            async Task ScrollToAsync(int index)
            {
                scrollAnimation = null;
                list.ScrollIntoView(index);
                Assert.IsNotNull(scrollAnimation);
                // SmoothScroll deliberately animates BringIntoView; the target may be recycled
                // while the viewport is still crossing the intervening rows.
                for (var frame = 0; frame < 150 && (presenter!.Offset - scrollAnimation.EndPosition).Length > 0.01; frame++)
                {
                    await Task.Delay(10);
                    Render(window);
                }
                Render(window);
                Assert.AreEqual(scrollAnimation.EndPosition.Y, presenter!.Offset.Y, 0.01);
                var container = Assert.IsInstanceOfType<ListBoxItem>(list.ContainerFromIndex(index));
                var card = Assert.IsInstanceOfType<WorkItem>(container.Content);
                Assert.AreEqual(items[index], card.DataContext);
                Assert.AreEqual(items[index], Assert.IsInstanceOfType<TextBlock>(card.Content).Text);
                Assert.IsTrue(container.Bounds.Width > 0 && container.Bounds.Height > 0);
            }
        }, CancellationToken.None);
    }

    private static Window CreateWindow(out List<Exception> animationFailures)
    {
        var window = new Window { Width = 720, Height = 720 };
        var failures = new List<Exception>();
        // Exercise the application's TopLevel style, including scope creation before content attaches.
        window.ApplyStyling();
        AnimationScope.GetRoot(window).AnimationFailed += (_, args) => failures.Add(args.Exception);
        animationFailures = failures;
        return window;
    }

    private static Color GetColor(IBrush? brush) => Assert.IsInstanceOfType<ISolidColorBrush>(brush).Color;

    private static Color GetResourceColor(Control control, string key)
    {
        Assert.IsTrue(control.TryFindResource(key, control.ActualThemeVariant, out var resource));
        return GetColor(Assert.IsInstanceOfType<IBrush>(resource));
    }

    private static void Render(Window window)
    {
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        AvaloniaHeadlessPlatform.ForceRenderTimerTick(1);
        using var frame = window.CaptureRenderedFrame();
        Assert.IsNotNull(frame);
    }

    public sealed class MaterialTestApplication : Application
    {
        public override void Initialize() => Styles.Add((Styles) AvaloniaXamlLoader.Load(
            new Uri("avares://Pixeval/Themes/PixevalTheme.axaml")));

        public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<MaterialTestApplication>()
            .UseSkia()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false, ShouldRenderOnUIThread = true })
            .WithPixevalFonts();
    }
}
