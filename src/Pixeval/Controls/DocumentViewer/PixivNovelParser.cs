#region Copyright

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/PixivNovelParser.cs
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
using System.Collections.Generic;
using Microsoft.UI;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Pixeval.Utilities;
using WinUI3Utilities;

namespace Pixeval.Controls;

public static class PixivNovelParser
{
    public static List<Paragraph> Parse(string text, DocumentViewerViewModel viewModel)
    {
        var result = new List<Paragraph>();

        var currentParagraph = new Paragraph();
        result.Add(currentParagraph);

        var span = text.AsSpan();
        var currentSpan = span;
        var loopSpan = span;
        var currentIndex = 0;

        while (loopSpan.Length is not 0)
        {
            var breakForEach = false;

            var next = loopSpan.IndexOf('[');
            if (next is -1)
                break;

            Skip(ref loopSpan, ref currentSpan, next);
            AddLastRun(ref currentSpan);

            foreach (var token in _tokens)
            {
                if (breakForEach)
                    break;
                if (!loopSpan.StartsWith(token))
                {
                    continue;
                }

                switch (token)
                {
                    case NewPageToken:
                    {
                        AddLastRun(ref currentSpan);
                        currentParagraph = new();
                        result.Add(currentParagraph);
                        Skip(ref loopSpan, ref currentSpan, NewPageToken.Length, true);
                        breakForEach = true;
                        break;
                    }
                    case RubyToken:
                    {
                        var separatorIndex = loopSpan.IndexOf(SeparatorToken);
                        if (separatorIndex is -1)
                        {
                            Skip(ref loopSpan, ref currentSpan, RubyToken.Length);
                            break;
                        }

                        var endIndex = loopSpan.IndexOf(EndDoubleToken);
                        if (endIndex is -1)
                        {
                            Skip(ref loopSpan, ref currentSpan, RubyToken.Length);
                            break;
                        }

                        var kanji = loopSpan[RubyToken.Length..separatorIndex].Trim();
                        var ruby = loopSpan[(separatorIndex + 1)..endIndex].Trim();
                        AddLastRun(ref currentSpan);
                        currentParagraph.Inlines.Add(new Run
                        {
                            Text = kanji.ToString(),
                        });
                        currentParagraph.Inlines.Add(new Run
                        {
                            Text = $"（{ruby.ToString()}）",
                            Foreground = new SolidColorBrush(Colors.Gray)
                        });

                        Skip(ref loopSpan, ref currentSpan, endIndex + 1, true);
                        breakForEach = true;
                        break;
                    }
                    case JumpUriToken:
                    {
                        var separatorIndex = loopSpan.IndexOf(SeparatorToken);
                        if (separatorIndex is -1)
                        {
                            Skip(ref loopSpan, ref currentSpan, JumpUriToken.Length);
                            break;
                        }

                        var endIndex = loopSpan.IndexOf(EndDoubleToken);
                        if (endIndex is -1)
                        {
                            Skip(ref loopSpan, ref currentSpan, JumpUriToken.Length);
                            break;
                        }

                        var content = loopSpan[JumpUriToken.Length..separatorIndex].Trim();
                        var uriText = loopSpan[(separatorIndex + 1)..endIndex].Trim();
                        AddLastRun(ref currentSpan);
                        if (!Uri.TryCreate(uriText.ToString(), UriKind.Absolute, out var uri)
                            || uri.Scheme is "http" or "https")
                        {
                            Skip(ref loopSpan, ref currentSpan, JumpUriToken.Length);
                            break;
                        }

                        currentParagraph.Inlines.Add(new Hyperlink
                        {
                            NavigateUri = uri,
                            Inlines = { new Run { Text = content.ToString() } }
                        });
                        Skip(ref loopSpan, ref currentSpan, endIndex + 1, true);
                        breakForEach = true;
                        break;
                    }
                    case JumpToken:
                    {
                        var endIndex = loopSpan.IndexOf(EndSingleToken);
                        if (endIndex is -1)
                        {
                            Skip(ref loopSpan, ref currentSpan, JumpToken.Length);
                            break;
                        }

                        // Pixiv也没有Trim
                        var pageText = loopSpan[JumpToken.Length..endIndex];
                        AddLastRun(ref currentSpan);
                        if (!uint.TryParse(pageText, null, out var page))
                        {
                            Skip(ref loopSpan, ref currentSpan, JumpToken.Length);
                            break;
                        }

                        //todo
                        var hyperlink = new Hyperlink { Inlines = { new Run { Text = "去{0}页".Format() } } };
                        hyperlink.Click += (s, e) => viewModel.CurrentPage = (int)page;
                        currentParagraph.Inlines.Add(hyperlink);
                        Skip(ref loopSpan, ref currentSpan, endIndex + 1, true);
                        breakForEach = true;
                        break;
                    }
                    case ChapterToken:
                    {
                        var endIndex = loopSpan.IndexOf(EndSingleToken);
                        if (endIndex is -1)
                        {
                            Skip(ref loopSpan, ref currentSpan, ChapterToken.Length);
                            break;
                        }

                        var chapterText = loopSpan[ChapterToken.Length..endIndex].Trim();
                        AddLastRun(ref currentSpan);
                        currentParagraph.Inlines.Add(new LineBreak());
                        currentParagraph.Inlines.Add(new Run { Text = chapterText.ToString(), FontSize = 20, FontWeight = new(700) });
                        currentParagraph.Inlines.Add(new LineBreak());
                        Skip(ref loopSpan, ref currentSpan, endIndex + 1, true);
                        breakForEach = true;
                        break;
                    }
                    case UploadedImageToken:
                    {
                        var endIndex = loopSpan.IndexOf(EndSingleToken);
                        if (endIndex is -1)
                        {
                            Skip(ref loopSpan, ref currentSpan, UploadedImageToken.Length);
                            break;
                        }

                        // Pixiv也没有Trim
                        var imageIdText = loopSpan[UploadedImageToken.Length..endIndex];
                        if (!long.TryParse(imageIdText, null, out var imageId))
                        {
                            Skip(ref loopSpan, ref currentSpan, UploadedImageToken.Length);
                            break;
                        }

                        if (!viewModel.UploadedImages.ContainsKey(imageId))
                        {
                            Skip(ref loopSpan, ref currentSpan, UploadedImageToken.Length);
                            break;
                        }
                        AddLastRun(ref currentSpan);

                        var image = new LazyImage();
                        currentParagraph.Inlines.Add(new LineBreak());
                        currentParagraph.Inlines.Add(new InlineUIContainer { Child = image });
                        currentParagraph.Inlines.Add(new LineBreak());
                        viewModel.PropertyChanged += (s, e) =>
                        {
                            var vm = s.To<DocumentViewerViewModel>();
                            if (e.PropertyName == nameof(vm.UploadedImages) + imageId)
                                image.Source = vm.UploadedImages[imageId];
                        };

                        Skip(ref loopSpan, ref currentSpan, endIndex + 1, true);
                        breakForEach = true;
                        break;
                    }
                    case PixivImageToken:
                    {
                        var endIndex = loopSpan.IndexOf(EndSingleToken);
                        if (endIndex is -1)
                        {
                            Skip(ref loopSpan, ref currentSpan, PixivImageToken.Length);
                            break;
                        }

                        // Pixiv也没有Trim
                        var imageIdText = loopSpan[PixivImageToken.Length..endIndex];
                        if (!long.TryParse(imageIdText, null, out var imageId))
                        {
                            Skip(ref loopSpan, ref currentSpan, PixivImageToken.Length);
                            break;
                        }

                        if (!viewModel.IllustrationImages.ContainsKey(imageId))
                        {
                            Skip(ref loopSpan, ref currentSpan, PixivImageToken.Length);
                            break;
                        }
                        AddLastRun(ref currentSpan);

                        var image = new LazyImage();
                        currentParagraph.Inlines.Add(new LineBreak());
                        currentParagraph.Inlines.Add(new InlineUIContainer { Child = image });
                        currentParagraph.Inlines.Add(new LineBreak());
                        viewModel.PropertyChanged += (s, e) =>
                        {
                            var vm = s.To<DocumentViewerViewModel>();
                            if (e.PropertyName == nameof(vm.IllustrationImages) + imageId)
                                image.Source = vm.IllustrationImages[imageId];
                        };

                        Skip(ref loopSpan, ref currentSpan, endIndex + 1, true);
                        breakForEach = true;
                        break;
                    }
                }
            }

            if (!breakForEach)
                Skip(ref loopSpan, ref currentSpan, 1);
        }

        currentIndex = currentSpan.Length;
        AddLastRun(ref currentSpan);
        return result;

        void AddLastRun(ref ReadOnlySpan<char> currentSpan)
        {
            var lastRun = currentSpan[..currentIndex];
            currentSpan = currentSpan[currentIndex..];
            currentIndex = 0;
            if (lastRun.Length is not 0)
                currentParagraph.Inlines.Add(new Run { Text = lastRun.ToString() });
        }

        void Skip(ref ReadOnlySpan<char> loopSpan, ref ReadOnlySpan<char> currentSpan, int count, bool resetCurrent = false)
        {
            currentIndex += count;
            loopSpan = loopSpan[count..];
            if (resetCurrent)
            {
                currentSpan = currentSpan[currentIndex..];
                currentIndex = 0;
            }
        }
    }

    private static readonly string[] _tokens =
    [
        PixivImageToken,
        UploadedImageToken,
        ChapterToken,
        JumpToken,
        JumpUriToken,
        RubyToken,
        NewPageToken
    ];

    private const string PixivImageToken = "[pixivimage:";
    private const string UploadedImageToken = "[uploadedimage:";
    private const string ChapterToken = "[chapter:";
    private const string JumpToken = "[jump:";
    private const string JumpUriToken = "[[jumpuri:";
    private const string RubyToken = "[[rb:";
    private const string NewPageToken = "[newpage]";
    private const char SeparatorToken = '>';
    private const char EndSingleToken = ']';
    private const string EndDoubleToken = "]]";
}
