using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Mako.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixeval.Controls;

namespace Pixeval.Tests;

[TestClass]
[DoNotParallelize]
public sealed class ItemsViewListBoxStylesTest
{
    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task ItemTemplateRendersSelectableCommentWithoutSelectionHighlight(bool plainTheme)
    {
        await using var session = HeadlessUnitTestSession.StartNew(typeof(ImageViewerRenderingTest.ViewerTestApplication));
        await session.Dispatch(() =>
        {
            var comment = Comment.CreateDefault();
            comment.Content = "Comment text that can be selected";
            var list = new ListBox
            {
                ItemsSource = new[] { comment },
                ItemTemplate = new FuncDataTemplate<Comment>((item, _) => new SelectableTextBlock
                {
                    Text = item!.Content
                })
            };
            if (plainTheme)
            {
                var themes = (ResourceDictionary) AvaloniaXamlLoader.Load(new Uri("avares://Pixeval/Themes/PlainListBoxItemTheme.axaml"));
                list.ItemContainerTheme = (ControlTheme) themes["PlainListBoxItemTheme"]!;
            }
            else
                list.Styles.Add(new ItemsViewListBoxStyles());
            var window = new Window { Width = 400, Height = 240, Content = list };
            try
            {
                window.Show();
                window.UpdateLayout();
                using var frame = window.CaptureRenderedFrame();
                var text = list.GetVisualDescendants().OfType<SelectableTextBlock>().SingleOrDefault();
                Assert.IsNotNull(text, "The comment must render through ItemTemplate instead of displaying its model type.");
                Assert.AreEqual(comment.Content, text.Text);
                var container = (ListBoxItem) list.ContainerFromIndex(0)!;
                var foreground = text.Foreground;

                var start = text.TranslatePoint(new(1, text.Bounds.Height / 2), window)!.Value;
                var end = text.TranslatePoint(new(80, text.Bounds.Height / 2), window)!.Value;
                Assert.AreSame(text, window.InputHitTest(start), $"Window={window.Bounds}, Text={text.Bounds}, Start={start}, Visible={text.IsEffectivelyVisible}");
                window.MouseMove(start);
                Assert.IsTrue(container.IsPointerOver);
                if (plainTheme)
                    AssertPlainAppearance();
                window.MouseDown(start, MouseButton.Left);
                window.MouseMove(end, RawInputModifiers.LeftMouseButton);
                window.MouseUp(end, MouseButton.Left);
                Assert.IsFalse(string.IsNullOrEmpty(text.SelectedText), "Dragging over the comment must select text.");

                window.MouseMove(new(window.Width - 1, window.Height - 1));
                list.SelectedIndex = 0;
                window.UpdateLayout();
                if (plainTheme)
                {
                    window.MouseMove(start);
                    AssertPlainAppearance();
                }
                else
                {
                    var borders = container.GetVisualDescendants().OfType<Border>().ToArray();
                    Assert.IsFalse(borders.Single(border => border.Name is "StateMask").IsVisible);
                    Assert.IsFalse(borders.Single(border => border.Name is "SelectionBorder").IsVisible);
                }
                Assert.AreEqual(comment.Content, text.Text);

                void AssertPlainAppearance()
                {
                    var presenter = container.GetVisualDescendants().OfType<ContentPresenter>()
                        .Single(control => control.Name is "PART_ContentPresenter");
                    Assert.AreEqual(Colors.Transparent, ((ISolidColorBrush) presenter.Background!).Color);
                    Assert.AreEqual(foreground, text.Foreground);
                }
            }
            finally
            {
                window.Close();
            }
        }, default);
    }
}
