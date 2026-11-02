using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Themes.Fluent;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixeval.Controls;
using SmoothScroll.Avalonia.Controls;

namespace Pixeval.Tests;

[TestClass]
[DoNotParallelize]
public sealed class MarkdownScrollChainingTest
{
    [TestMethod]
    public async Task WheelOverCodeThatFitsScrollsTheDocument()
    {
        await using var session = HeadlessUnitTestSession.StartNew(typeof(MarkdownTestApplication));
        await session.Dispatch(() =>
        {
            var markdown = new MarkdownBox
            {
                Markdown = "```text\nhello\n```\n\n" + string.Join("\n\n", Enumerable.Repeat("Paragraph", 50))
            };
            var window = new Window { Width = 500, Height = 400, Content = markdown };
            try
            {
                window.Show();
                window.UpdateLayout();
                Dispatcher.UIThread.RunJobs();
                AvaloniaHeadlessPlatform.ForceRenderTimerTick();
                var viewers = markdown.GetVisualDescendants().OfType<ScrollViewer>().ToArray();
                var code = viewers.Single(view => view.Classes.Contains("CodeBlock"));
                var document = viewers.First();
                Assert.AreNotSame(document, code);
                foreach (var viewer in viewers)
                    viewer.IsScrollInertiaEnabled = false;
                Assert.IsTrue(code.Extent.Width <= code.Viewport.Width);
                Assert.IsTrue(document.Extent.Height > document.Viewport.Height);
                var point = code.TranslatePoint(new(20, 10), window)!.Value;
                Assert.IsInstanceOfType<ScrollViewerPresenter>(code.Presenter);
                Assert.IsTrue(code.IsScrollChainingEnabled);
                var hit = (Visual) window.InputHitTest(point)!;
                Assert.Contains(code, hit.GetVisualAncestors().ToArray());

                window.MouseWheel(point, new Vector(0, -1));
                Dispatcher.UIThread.RunJobs();
                AvaloniaHeadlessPlatform.ForceRenderTimerTick();

                Assert.IsTrue(document.Offset.Y > 0, $"Code={code.Offset}, Document={document.Offset}");
            }
            finally
            {
                window.Close();
            }
        }, default);
    }

    public sealed class MarkdownTestApplication : Application
    {
        public override void Initialize()
        {
            Styles.Add(new FluentTheme());
            Styles.Add(new ScrollViewerSmoothTheme());
        }

        public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<MarkdownTestApplication>()
            .UseSkia().UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false });
    }
}
