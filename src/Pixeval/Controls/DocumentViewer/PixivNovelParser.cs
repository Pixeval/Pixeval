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

namespace Pixeval.Controls;

public abstract class PixivNovelParser<T, TImage, TViewModel> where TViewModel : INovelParserViewModel<TImage>
{
    protected abstract T Vector { get; }

    protected abstract void AddLastSpan(T result, string lastSpan);

    protected abstract void AddRuby(T result, string kanji, string ruby);

    protected abstract void AddHyperlink(T result, string content, Uri page);

    protected abstract void AddInlineHyperlink(T result, uint uri, TViewModel viewModel);

    protected abstract void AddChapter(T result, string chapterText);

    protected abstract void AddUploadedImage(T result, TViewModel viewModel, long imageId);

    /// <summary>
    /// 此处<paramref name="page"/>是从1开始的
    /// </summary>
    protected abstract void AddPixivImage(T paragraphs, TViewModel viewModel, long imageId, int page);

    protected virtual void Finish(T result)
    {
    }

    protected virtual void NewPage(T result)
    {
    }

    public T Parse(string text, ref int startIndex, TViewModel viewModel)
    {
        var result = Vector;

        var currentIndex = 0;
        var span = text.AsSpan(startIndex);
        var currentSpan = span;
        var loopSpan = span;

        while (loopSpan.Length is not 0)
        {
            var breakForEach = false;

            var next = loopSpan.IndexOf('[');
            if (next is -1)
                break;

            Skip(ref loopSpan, ref currentSpan, ref startIndex, next);
            AddLastRun(ref currentSpan);

            foreach (var token in PixivTokens.Tokens)
            {
                if (breakForEach)
                    break;
                if (!loopSpan.StartsWith(token))
                    continue;

                switch (token)
                {
                    case PixivTokens.NewPageToken:
                    {
                        AddLastRun(ref currentSpan);
                        Skip(ref loopSpan, ref currentSpan, ref startIndex, token.Length, true);
                        NewPage(result);
                        return result;
                    }
                    case PixivTokens.RubyToken:
                    {
                        var separatorIndex = loopSpan.IndexOf(PixivTokens.SeparatorToken);
                        if (separatorIndex is -1)
                        {
                            Skip(ref loopSpan, ref currentSpan, ref startIndex, token.Length);
                            break;
                        }

                        var endIndex = loopSpan.IndexOf(PixivTokens.EndDoubleToken);
                        if (endIndex is -1)
                        {
                            Skip(ref loopSpan, ref currentSpan, ref startIndex, token.Length);
                            break;
                        }

                        var kanji = loopSpan[token.Length..separatorIndex].Trim();
                        var ruby = loopSpan[(separatorIndex + 1)..endIndex].Trim();
                        AddLastRun(ref currentSpan);
                        AddRuby(result, kanji.ToString(), ruby.ToString());

                        var end = endIndex + PixivTokens.EndDoubleToken.Length;
                        Skip(ref loopSpan, ref currentSpan, ref startIndex, end, true);
                        breakForEach = true;
                        break;
                    }
                    case PixivTokens.JumpUriToken:
                    {
                        var separatorIndex = loopSpan.IndexOf(PixivTokens.SeparatorToken);
                        if (separatorIndex is -1)
                        {
                            Skip(ref loopSpan, ref currentSpan, ref startIndex, token.Length);
                            break;
                        }

                        var endIndex = loopSpan.IndexOf(PixivTokens.EndDoubleToken);
                        if (endIndex is -1)
                        {
                            Skip(ref loopSpan, ref currentSpan, ref startIndex, token.Length);
                            break;
                        }

                        var content = loopSpan[token.Length..separatorIndex].Trim();
                        var uriText = loopSpan[(separatorIndex + 1)..endIndex].Trim();
                        AddLastRun(ref currentSpan);
                        if (!Uri.TryCreate(uriText.ToString(), UriKind.Absolute, out var uri)
                            || uri.Scheme is not ("http" or "https"))
                        {
                            Skip(ref loopSpan, ref currentSpan, ref startIndex, token.Length);
                            break;
                        }

                        AddHyperlink(result, content.ToString(), uri);

                        var end = endIndex + PixivTokens.EndDoubleToken.Length;
                        Skip(ref loopSpan, ref currentSpan, ref startIndex, end, true);
                        breakForEach = true;
                        break;
                    }
                    case PixivTokens.JumpToken:
                    {
                        var endIndex = loopSpan.IndexOf(PixivTokens.EndSingleToken);
                        if (endIndex is -1)
                        {
                            Skip(ref loopSpan, ref currentSpan, ref startIndex, token.Length);
                            break;
                        }

                        // Pixiv也没有Trim
                        var pageText = loopSpan[token.Length..endIndex];
                        AddLastRun(ref currentSpan);
                        if (!uint.TryParse(pageText, null, out var page))
                        {
                            Skip(ref loopSpan, ref currentSpan, ref startIndex, token.Length);
                            break;
                        }

                        AddInlineHyperlink(result, page, viewModel);

                        var end = endIndex + 1;
                        Skip(ref loopSpan, ref currentSpan, ref startIndex, end, true);
                        breakForEach = true;
                        break;
                    }
                    case PixivTokens.ChapterToken:
                    {
                        var endIndex = loopSpan.IndexOf(PixivTokens.EndSingleToken);
                        if (endIndex is -1)
                        {
                            Skip(ref loopSpan, ref currentSpan, ref startIndex, token.Length);
                            break;
                        }

                        var chapterText = loopSpan[token.Length..endIndex].Trim();
                        AddLastRun(ref currentSpan);

                        AddChapter(result, chapterText.ToString());

                        var end = endIndex + 1;
                        Skip(ref loopSpan, ref currentSpan, ref startIndex, end, true);
                        breakForEach = true;
                        break;
                    }
                    case PixivTokens.UploadedImageToken:
                    {
                        var endIndex = loopSpan.IndexOf(PixivTokens.EndSingleToken);
                        if (endIndex is -1)
                        {
                            Skip(ref loopSpan, ref currentSpan, ref startIndex, token.Length);
                            break;
                        }

                        // Pixiv也没有Trim
                        var imageIdText = loopSpan[token.Length..endIndex];
                        if (!long.TryParse(imageIdText, null, out var imageId))
                        {
                            Skip(ref loopSpan, ref currentSpan, ref startIndex, token.Length);
                            break;
                        }

                        if (!viewModel.UploadedImages.ContainsKey(imageId))
                        {
                            Skip(ref loopSpan, ref currentSpan, ref startIndex, token.Length);
                            break;
                        }
                        AddLastRun(ref currentSpan);

                        AddUploadedImage(result, viewModel, imageId);

                        var end = endIndex + 1;
                        Skip(ref loopSpan, ref currentSpan, ref startIndex, end, true);
                        breakForEach = true;
                        break;
                    }
                    case PixivTokens.PixivImageToken:
                    {
                        var endIndex = loopSpan.IndexOf(PixivTokens.EndSingleToken);
                        if (endIndex is -1)
                        {
                            Skip(ref loopSpan, ref currentSpan, ref startIndex, token.Length);
                            break;
                        }

                        // Pixiv也没有Trim
                        var imageIdText = loopSpan[token.Length..endIndex];
                        var dest = new Span<Range>([new(), new(), new()]);
                        var page = 1;
                        var length = imageIdText.Split(dest, '-');
                        var imageId = 0L;
                        // 只有不包含空白字符的纯数字，或者被'-'分割的两个数字才能解析（后者序号从1开始）
                        // 例如："12345678"、"12345678-2"
                        // 反例："12345678-1-1"、" 12345678"、"12345678-1 "、"12345678-"
                        if (length is not (1 or 2)
                            || (length is 2 && (!long.TryParse(imageIdText[dest[0]], null, out imageId) || !int.TryParse(imageIdText[dest[1]], null, out page)))
                            || (length is 1 && !long.TryParse(imageIdText, null, out imageId))
                            || (imageId, page) is var key && !viewModel.IllustrationImages.ContainsKey(key))
                        {
                            Skip(ref loopSpan, ref currentSpan, ref startIndex, token.Length);
                            break;
                        }

                        AddLastRun(ref currentSpan);

                        AddPixivImage(result, viewModel, imageId, page);

                        var end = endIndex + 1;
                        Skip(ref loopSpan, ref currentSpan, ref startIndex, end, true);
                        breakForEach = true;
                        break;
                    }
                }
            }

            if (!breakForEach)
                Skip(ref loopSpan, ref currentSpan, ref startIndex, 1);
        }

        startIndex = text.Length;
        currentIndex = currentSpan.Length;
        AddLastRun(ref currentSpan);
        Finish(result);
        return result;

        void AddLastRun(ref ReadOnlySpan<char> currentSpan)
        {
            var lastRun = currentSpan[..currentIndex];
            currentSpan = currentSpan[currentIndex..];
            currentIndex = 0;
            if (lastRun.Length is not 0)
                AddLastSpan(result, lastRun.ToString());
        }

        void Skip(ref ReadOnlySpan<char> loopSpan, ref ReadOnlySpan<char> currentSpan, ref int startIndex, int count, bool resetCurrent = false)
        {
            startIndex += count;
            currentIndex += count;
            loopSpan = loopSpan[count..];
            if (resetCurrent)
            {
                currentSpan = currentSpan[currentIndex..];
                currentIndex = 0;
            }
        }
    }
}

static file class PixivTokens
{
    public static readonly string[] Tokens =
    [
        PixivImageToken,
        UploadedImageToken,
        ChapterToken,
        JumpToken,
        JumpUriToken,
        RubyToken,
        NewPageToken
    ];

    public const string PixivImageToken = "[pixivimage:";
    public const string UploadedImageToken = "[uploadedimage:";
    public const string ChapterToken = "[chapter:";
    public const string JumpToken = "[jump:";
    public const string JumpUriToken = "[[jumpuri:";
    public const string RubyToken = "[[rb:";
    public const string NewPageToken = "[newpage]";
    public const char SeparatorToken = '>';
    public const char EndSingleToken = ']';
    public const string EndDoubleToken = "]]";
}
