using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ColorTextBlock.Avalonia;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixeval.Controls;

namespace Pixeval.Tests;

[TestClass]
[DoNotParallelize]
public sealed class MarkdownCompatibilityTest
{
    [TestMethod]
    [DataRow(false, 500)]
    [DataRow(true, 500)]
    [DataRow(false, 80)]
    [DataRow(true, 80)]
    public async Task HtmlExtensionsPreserveRubySelection(bool inline, int width)
    {
        await using var session = HeadlessUnitTestSession.StartNew(typeof(MarkdownScrollChainingTest.MarkdownTestApplication));
        await session.Dispatch(async () =>
        {
            const string html = "<color foreground=\"#ff0000\">red <ruby>漢字<rt>かんじ</rt></ruby></color>";
            var markdown = new MarkdownBox
            {
                Markdown = inline ? $"Before {html} after" : $"<div id=\"chapter\"><p>{html}</p></div>"
            };
            var window = new Window { Width = width, Height = 400, Content = markdown };
            try
            {
                window.Show();
                window.UpdateLayout();
                Dispatcher.UIThread.RunJobs();
                AvaloniaHeadlessPlatform.ForceRenderTimerTick();

                var text = markdown.GetVisualDescendants().OfType<CTextBlock>()
                    .Single(block => EnumerateInlines(block.Content).OfType<RubyInline>().Any());
                var inlines = EnumerateInlines(text.Content).ToArray();
                var ruby = inlines.OfType<RubyInline>().Single();
                Assert.AreEqual("漢字", ruby.Body);
                Assert.AreEqual("かんじ", ruby.Annotation);
                Assert.IsTrue(inlines.OfType<CSpan>().Any(span => span.Foreground is ISolidColorBrush { Color: var color } && color == Colors.Red));

                text.Select(text.GetBegin(), text.GetEnd());
                var selection = text.Selection;
                Assert.IsNotNull(selection);
                Assert.AreEqual(0, selection.From);
                Assert.AreEqual(text.Text.Length, selection.To);

                // Copy through MarkdownBox so custom ruby geometry contributes its body text.
                var copy = new KeyEventArgs
                {
                    RoutedEvent = InputElement.KeyDownEvent,
                    Key = Key.C,
                    KeyModifiers = KeyModifiers.Control
                };
                text.RaiseEvent(copy);
                Assert.IsTrue(copy.Handled);
                Assert.IsNotNull(window.Clipboard);
                Assert.AreEqual(inline ? "Before red 漢字 after" : "red 漢字", await window.Clipboard.TryGetTextAsync());

                if (!inline)
                {
                    Assert.IsTrue(markdown.GetVisualDescendants().OfType<Border>()
                        .Any(border => border.Classes.Contains(MarkdownAnchor.ClassName) && border.Tag is "chapter"));
                }
            }
            finally
            {
                window.Close();
            }
        }, default);
    }

    private static IEnumerable<CInline> EnumerateInlines(IEnumerable<CInline> inlines)
    {
        foreach (var inline in inlines)
        {
            yield return inline;
            if (inline is CSpan { Content: { } children })
            {
                foreach (var child in EnumerateInlines(children))
                    yield return child;
            }
        }
    }
}
