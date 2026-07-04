// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Media;
using ColorTextBlock.Avalonia;
using ColorTextBlock.Avalonia.Geometries;

namespace Pixeval.Controls;

internal sealed class RubyInline(string body, string annotation) : CInline
{
    private const double AnnotationScale = 0.55;
    private const double GapScale = 0.08;

    public string Body { get; } = body;

    public string Annotation { get; } = annotation;

    protected override IEnumerable<CGeometry> MeasureOverride(double entireWidth, double remainWidth)
    {
        var layout = RubyLayout.Create(this);

        if (layout.Width > remainWidth
            && remainWidth < entireWidth
            && !double.IsPositiveInfinity(remainWidth))
        {
            yield return new RubySoftBreakGeometry(this);
        }

        yield return new RubyGeometry(this, layout);
    }

    public override string AsString() => Body;

    private IBrush ResolveForeground()
    {
        if (Foreground is { } foreground)
            return foreground;

        for (var current = Parent; current is not null; current = current.Parent)
        {
            if (current is CInline { Foreground: { } inlineForeground })
                return inlineForeground;

            if (current is CTextBlock { Foreground: { } textForeground })
                return textForeground;
        }

        return Brushes.Black;
    }

    private sealed class RubyGeometry(RubyInline owner, RubyLayout layout) : CGeometry(owner, layout.Width,
        layout.Height, layout.BaseHeight, owner.TextVerticalAlignment, false)
    {
        public override void Render(DrawingContext context)
        {
            if (owner.Background is not null)
                context.FillRectangle(owner.Background, new Rect(Left, Top, Width, Height));

            var foreground = owner.ResolveForeground();
            layout.Annotation.Draw(context, owner.Typeface, layout.AnnotationFontSize, foreground, Left, Top, Width);
            layout.Body.Draw(context, owner.Typeface, owner.FontSize, foreground, Left, Top + layout.BodyTop, Width);

            var baseline = Top + BaseHeight;
            var pen = new Pen(foreground, Math.Max(1, owner.FontSize / 16));

            if (owner.IsUnderline)
                context.DrawLine(pen, new Point(Left, baseline + pen.Thickness), new Point(Left + Width, baseline + pen.Thickness));

            if (owner.IsStrikethrough)
                context.DrawLine(pen, new Point(Left, Top + layout.BodyTop + (layout.Body.Height * 0.55)), new Point(Left + Width, Top + layout.BodyTop + (layout.Body.Height * 0.55)));
        }

        public override TextPointer CalcuatePointerFrom(double x, double y)
        {
            var ratio = Width <= 0
                ? 0
                : Math.Clamp((x - Left) / Width, 0, 1);
            var index = (int) Math.Round(ratio * owner.Body.Length);

            return CreatePointer(index, GetDistance(index));
        }

        public override TextPointer CalcuatePointerFrom(int index)
        {
            if (index < 0 || index > owner.Body.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            return CreatePointer(index, GetDistance(index));
        }

        public override TextPointer GetBegin() => CreatePointer(0, 0);

        public override TextPointer GetEnd() => CreatePointer(owner.Body.Length, Width);

        private TextPointer CreatePointer(int index, double distance) =>
            TextPointerFactory.Create(this, index, distance);

        private double GetDistance(int index) =>
            owner.Body.Length is 0 ? 0 : Width * index / owner.Body.Length;
    }

    private sealed class RubySoftBreakGeometry(CInline owner)
        : CGeometry(owner, 0, 0, 0, TextVerticalAlignment.Base, true)
    {
        public override int CaretLength => 0;

        public override void Render(DrawingContext context)
        {
        }

        public override TextPointer CalcuatePointerFrom(double x, double y) => GetBegin();

        public override TextPointer CalcuatePointerFrom(int index) =>
            index is 0 ? GetBegin() : throw new ArgumentOutOfRangeException(nameof(index));

        public override TextPointer GetBegin() => TextPointerFactory.Create(this, 0, 0);

        public override TextPointer GetEnd() => TextPointerFactory.Create(this, 0, 0);
    }

    private sealed class RubyLayout
    {
        private RubyLayout(MeasuredText body, MeasuredText annotation, double annotationFontSize, double gap)
        {
            Body = body;
            Annotation = annotation;
            AnnotationFontSize = annotationFontSize;
            Width = Math.Ceiling(Math.Max(body.Width, annotation.Width));
            BodyTop = annotation.Height + gap;
            BaseHeight = BodyTop + body.Baseline;
            Height = BodyTop + body.Height;
        }

        public MeasuredText Body { get; }

        public MeasuredText Annotation { get; }

        public double AnnotationFontSize { get; }

        public double Width { get; }

        public double Height { get; }

        public double BaseHeight { get; }

        public double BodyTop { get; }

        public static RubyLayout Create(RubyInline owner)
        {
            var annotationFontSize = Math.Max(1, owner.FontSize * AnnotationScale);
            var gap = Math.Max(1, owner.FontSize * GapScale);

            return new RubyLayout(
                MeasuredText.Create(owner.Body, owner.Typeface, owner.FontSize, owner.Foreground),
                MeasuredText.Create(owner.Annotation, owner.Typeface, annotationFontSize, owner.Foreground),
                annotationFontSize,
                gap);
        }
    }

    private sealed class MeasuredText
    {
        private MeasuredText(string text, double height, double baseline, TextElement[] elements)
        {
            Text = text;
            Height = height;
            Baseline = baseline;
            Elements = elements;
            Width = elements.Sum(element => element.Width);
        }

        public string Text { get; }

        public double Width { get; }

        public double Height { get; }

        public double Baseline { get; }

        private TextElement[] Elements { get; }

        public static MeasuredText Create(string text, Typeface typeface, double fontSize, IBrush? foreground)
        {
            var full = CreateFormattedText(text, typeface, fontSize, foreground);
            var elements = SplitTextElements(text)
                .Select(element => new TextElement(
                    element,
                    CreateFormattedText(element, typeface, fontSize, foreground).WidthIncludingTrailingWhitespace))
                .ToArray();

            return new MeasuredText(text, full.Height, full.Baseline, elements);
        }

        public void Draw(
            DrawingContext context,
            Typeface typeface,
            double fontSize,
            IBrush foreground,
            double left,
            double top,
            double targetWidth)
        {
            if (Elements.Length is 0)
                return;

            if (Elements.Length is 1)
            {
                DrawElement(context, typeface, fontSize, foreground, Elements[0].Text, left + ((targetWidth - Elements[0].Width) / 2), top);
                return;
            }

            var gap = Math.Max(0, (targetWidth - Width) / (Elements.Length - 1));
            var x = left;

            foreach (var element in Elements)
            {
                DrawElement(context, typeface, fontSize, foreground, element.Text, x, top);
                x += element.Width + gap;
            }
        }

        private static void DrawElement(
            DrawingContext context,
            Typeface typeface,
            double fontSize,
            IBrush foreground,
            string text,
            double left,
            double top)
        {
            context.DrawText(CreateFormattedText(text, typeface, fontSize, foreground), new Point(left, top));
        }

        private static FormattedText CreateFormattedText(
            string text,
            Typeface typeface,
            double fontSize,
            IBrush? foreground) =>
            new(
                text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                fontSize,
                foreground ?? Brushes.Black);

        private static IEnumerable<string> SplitTextElements(string text)
        {
            var enumerator = StringInfo.GetTextElementEnumerator(text);

            while (enumerator.MoveNext())
                yield return (string) enumerator.Current;
        }

        private sealed record TextElement(string Text, double Width);
    }

    private static class TextPointerFactory
    {
        [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
        public static extern TextPointer Create(CGeometry geometry, int index, double distance);
    }
}
