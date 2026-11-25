using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Headless;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Themes.Fluent2;
using Avalonia.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixeval.Controls;
using Pixeval.I18N;
using Pixeval.Utilities;
using Pixeval.Views.Converters;
using Pixeval.Views.Work;
using VirtualizingWrapPanel = Pixeval.Controls.VirtualizingWrapPanel;

namespace Pixeval.Tests;

[TestClass]
[DoNotParallelize]
public sealed class IllustrationItemLayoutTest
{
    [TestMethod]
    [DataRow(0.5)]
    [DataRow(0.05)]
    public async Task ReusedLandscapePlaceholderDoesNotWidenPortraitThumbnail(double ratio)
    {
        await using var session = HeadlessUnitTestSession.StartNew(typeof(IllustrationTestApplication));
        await session.Dispatch(() =>
        {
            var card = new IllustrationItem();
            var thumbnail = card.FindControl<ConstrainedBox>("ThumbnailBounds")!;
            thumbnail.AspectRatio = 4;
            var skeleton = card.GetLogicalDescendants().OfType<Skeleton>().Single();
            var window = new Window { Width = 800, Height = 200, Content = card };
            try
            {
                window.Show();
                window.UpdateLayout();
                Dispatcher.UIThread.RunJobs();
                Assert.IsTrue(skeleton.IsLoaded);
                Assert.AreEqual(800, skeleton.Bounds.Width, 0.01);

                // The flow panel measures a recycled card with an unconstrained width.
                thumbnail.AspectRatio = ratio;
                card.Measure(new Size(double.PositiveInfinity, 200));

                Assert.AreEqual(200 * ratio, card.DesiredSize.Width, 0.01);
                Assert.AreEqual(200, card.DesiredSize.Height, 0.01);
            }
            finally
            {
                window.Close();
            }
        }, CancellationToken.None);
    }

    [TestMethod]
    [DataRow(0.5)]
    [DataRow(2d)]
    public async Task SquareSlotStretchesThumbnailAndOverlays(double ratio)
    {
        await using var session = HeadlessUnitTestSession.StartNew(typeof(IllustrationTestApplication));
        await session.Dispatch(() =>
        {
            var card = new IllustrationItem();
            card.FindControl<ConstrainedBox>("ThumbnailBounds")!.AspectRatio = ratio;
            using var bitmap = new WriteableBitmap(new PixelSize((int) (200 * ratio), 200), new Vector(96, 96));
            var image = card.FindControl<Image>("ThumbnailImage")!;
            image.Source = bitmap;
            var window = new Window { Width = 300, Height = 300, Content = card };
            try
            {
                window.Show();
                window.UpdateLayout();
                Dispatcher.UIThread.RunJobs();

                Assert.AreEqual(new Size(300, 300), image.Bounds.Size);
                Assert.AreEqual(new Size(300, 300), card.GetLogicalDescendants().OfType<Skeleton>().Single().Bounds.Size);
            }
            finally
            {
                window.Close();
            }
        }, CancellationToken.None);
    }

    [TestMethod]
    public async Task AppendedPortraitsKeepTheirWidthAfterRecyclingLandscapeCards()
    {
        await using var session = HeadlessUnitTestSession.StartNew(typeof(IllustrationTestApplication));
        await session.Dispatch(() =>
        {
            var items = new ObservableCollection<double>(Enumerable.Repeat(4d, 16));
            var list = new WorkListBox
            {
                ItemsSource = items,
                ItemsPanel = new FuncTemplate<Panel?>(() => new VirtualizingWrapPanel
                {
                    ItemHeight = 200,
                    ItemSpacing = 5,
                    LineSpacing = 5,
                    ItemsAlignment = WrapPanelItemsAlignment.Stretch,
                    CacheLength = 0
                })
            };
            list.DataTemplates.Add(new FuncDataTemplate<double>((_, _) =>
            {
                var card = new IllustrationItem();
                _ = card.FindControl<ConstrainedBox>("ThumbnailBounds")!.Bind(ConstrainedBox.AspectRatioProperty, new Binding(".")
                {
                    Converter = PixevalConverters.ToAspectRatio
                });
                return card;
            }));
            var window = new Window { Width = 1000, Height = 1200, Content = list };
            try
            {
                window.Show();
                UpdateLayout();
                var firstCard = (WorkItem) ((ListBoxItem) list.ContainerFromIndex(0)!).Content!;
                var placeholder = firstCard.GetLogicalDescendants().OfType<Skeleton>().Single();
                Assert.IsTrue(placeholder.IsLoaded);
                Assert.IsTrue(placeholder.Bounds.Width > 600);
                // Leave arranged landscape cards in the pool before measuring the next batch.
                window.Height = 400;
                UpdateLayout();
                ScrollTo(12);

                for (var batch = 0; batch < 3; batch++)
                {
                    var start = items.Count;
                    for (var i = 0; i < 16; i++)
                        items.Add(0.5);
                    UpdateLayout();
                    ScrollTo(start + 8);
                    var container = list.ContainerFromIndex(start + 8)!;

                    Assert.IsTrue(container.Bounds.Width < 200,
                        $"Portrait in batch {batch} was widened to {container.Bounds.Width}.");
                    ScrollTo(0);
                    ScrollTo(items.Count - 1);
                }
            }
            finally
            {
                list.ItemsSource = null;
                window.Close();
            }

            void ScrollTo(int index)
            {
                list.ScrollIntoView(index);
                UpdateLayout();
                Assert.IsNotNull(list.ContainerFromIndex(index));
            }

            void UpdateLayout()
            {
                window.UpdateLayout();
                Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();
            }
        }, CancellationToken.None);
    }

    public sealed class IllustrationTestApplication : Application
    {
        public override void Initialize()
        {
            I18NManager.Register(new JsonMarkdownLangPlugin(), LanguageHelper.DefaultLanguage);
            Styles.Add(new Fluent2Theme());
            Resources["RoundedButtonStyle"] = new ControlTheme(typeof(Button))
            {
                BasedOn = (ControlTheme) this.FindResource(typeof(Button))!,
                Setters =
                {
                    new Setter(Layoutable.HorizontalAlignmentProperty, HorizontalAlignment.Stretch),
                    new Setter(Layoutable.VerticalAlignmentProperty, VerticalAlignment.Stretch),
                    new Setter(TemplatedControl.PaddingProperty, default(Thickness)),
                    new Setter(TemplatedControl.BorderThicknessProperty, default(Thickness)),
                    new Setter(ContentControl.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch),
                    new Setter(ContentControl.VerticalContentAlignmentProperty, VerticalAlignment.Stretch)
                }
            };
            Resources[typeof(Skeleton)] = new ControlTheme(typeof(Skeleton))
            {
                Setters =
                {
                    new Setter(TemplatedControl.TemplateProperty, new FuncControlTemplate<Skeleton>((_, scope) =>
                    {
                        var shimmer = new Border();
                        scope.Register("PART_Shimmer", shimmer);
                        return new Panel { Children = { shimmer } };
                    }))
                }
            };
        }

        public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<IllustrationTestApplication>()
            .UseSkia().UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false });
    }
}
