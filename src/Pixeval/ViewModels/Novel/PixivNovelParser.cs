// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;

namespace Pixeval.ViewModels;

public abstract class PixivNovelParser<TText, TImage, TViewModel> where TViewModel : INovelContext<TImage> where TImage : class
{
    protected abstract TText Vector { get; }

    protected abstract void AddLastSpan(TText result, string lastSpan);

    protected abstract void AddRuby(TText result, string kanji, string ruby);

    protected abstract void AddHyperlink(TText result, string content, Uri page);

    protected abstract void AddInlineHyperlink(TText result, uint uri);

    protected abstract void AddChapter(TText result, string chapterText);

    protected abstract void AddUploadedImage(TText result, TViewModel viewModel, long imageId);

    /// <summary>
    /// 此处<paramref name="page"/>是从1开始的
    /// </summary>
    protected abstract void AddPixivImage(TText paragraphs, TViewModel viewModel, long imageId, int page);

    protected virtual void Finish(TText result)
    {
    }

    protected virtual void NewPage(TText result)
    {
    }

    public TText Parse(string text, ref int startIndex, TViewModel viewModel)
    {
        var result = Vector;

        var position = startIndex;
        var runStart = startIndex;

        while (position < text.Length)
        {
            var next = text.AsSpan(position).IndexOf('[');
            if (next is -1)
                break;

            position += next;
            AddLastRun(position);

            var tokenSpan = text.AsSpan(position);
            var token = PixivTokens.Match(tokenSpan);
            var tokenLength = PixivTokens.GetLength(token);
            switch (token)
            {
                case PixivToken.None:
                    ++position;
                    break;
                case PixivToken.NewPage:
                    position += tokenLength;
                    runStart = position;
                    startIndex = position;
                    NewPage(result);
                    return result;
                case PixivToken.Ruby:
                    ParseRuby(tokenSpan, tokenLength);
                    break;
                case PixivToken.JumpUri:
                    ParseJumpUri(tokenSpan, tokenLength);
                    break;
                case PixivToken.Jump:
                    ParseJump(tokenSpan, tokenLength);
                    break;
                case PixivToken.Chapter:
                    ParseChapter(tokenSpan, tokenLength);
                    break;
                case PixivToken.UploadedImage:
                    ParseUploadedImage(tokenSpan, tokenLength);
                    break;
                case PixivToken.PixivImage:
                    ParsePixivImage(tokenSpan, tokenLength);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(token));
            }
        }

        startIndex = text.Length;
        AddLastRun(text.Length);
        Finish(result);
        return result;

        void ParseRuby(ReadOnlySpan<char> tokenSpan, int tokenLength)
        {
            var separatorIndex = tokenSpan.IndexOf(PixivTokens.SeparatorToken);
            if (separatorIndex is -1)
            {
                position += tokenLength;
                return;
            }

            var endIndex = tokenSpan.IndexOf(PixivTokens.EndDoubleToken);
            if (endIndex is -1)
            {
                position += tokenLength;
                return;
            }

            var kanji = tokenSpan[tokenLength..separatorIndex].Trim();
            var ruby = tokenSpan[(separatorIndex + 1)..endIndex].Trim();
            AddRuby(result, kanji.ToString(), ruby.ToString());

            var end = endIndex + PixivTokens.EndDoubleToken.Length;
            position += end;
            runStart = position;
        }

        void ParseJumpUri(ReadOnlySpan<char> tokenSpan, int tokenLength)
        {
            var separatorIndex = tokenSpan.IndexOf(PixivTokens.SeparatorToken);
            if (separatorIndex is -1)
            {
                position += tokenLength;
                return;
            }

            var endIndex = tokenSpan.IndexOf(PixivTokens.EndDoubleToken);
            if (endIndex is -1)
            {
                position += tokenLength;
                return;
            }

            var content = tokenSpan[tokenLength..separatorIndex].Trim();
            var uriText = tokenSpan[(separatorIndex + 1)..endIndex].Trim();
            if (!Uri.TryCreate(uriText.ToString(), UriKind.Absolute, out var uri)
                || uri.Scheme is not ("http" or "https"))
            {
                position += tokenLength;
                return;
            }

            AddHyperlink(result, content.ToString(), uri);

            var end = endIndex + PixivTokens.EndDoubleToken.Length;
            position += end;
            runStart = position;
        }

        void ParseJump(ReadOnlySpan<char> tokenSpan, int tokenLength)
        {
            var endIndex = tokenSpan.IndexOf(PixivTokens.EndSingleToken);
            if (endIndex is -1)
            {
                position += tokenLength;
                return;
            }

            // Pixiv也没有Trim
            var pageText = tokenSpan[tokenLength..endIndex];
            if (!uint.TryParse(pageText, null, out var page))
            {
                position += tokenLength;
                return;
            }

            AddInlineHyperlink(result, page);

            var end = endIndex + 1;
            position += end;
            runStart = position;
        }

        void ParseChapter(ReadOnlySpan<char> tokenSpan, int tokenLength)
        {
            var endIndex = tokenSpan.IndexOf(PixivTokens.EndSingleToken);
            if (endIndex is -1)
            {
                position += tokenLength;
                return;
            }

            var chapterText = tokenSpan[tokenLength..endIndex].Trim();

            AddChapter(result, chapterText.ToString());

            var end = endIndex + 1;
            position += end;
            runStart = position;
        }

        void ParseUploadedImage(ReadOnlySpan<char> tokenSpan, int tokenLength)
        {
            var endIndex = tokenSpan.IndexOf(PixivTokens.EndSingleToken);
            if (endIndex is -1)
            {
                position += tokenLength;
                return;
            }

            // Pixiv也没有Trim
            var imageIdText = tokenSpan[tokenLength..endIndex];
            if (!long.TryParse(imageIdText, null, out var imageId) || !viewModel.UploadedImages.ContainsKey(imageId))
            {
                position += tokenLength;
                return;
            }

            AddUploadedImage(result, viewModel, imageId);

            var end = endIndex + 1;
            position += end;
            runStart = position;
        }

        void ParsePixivImage(ReadOnlySpan<char> tokenSpan, int tokenLength)
        {
            var endIndex = tokenSpan.IndexOf(PixivTokens.EndSingleToken);
            if (endIndex is -1)
            {
                position += tokenLength;
                return;
            }

            // Pixiv也没有Trim
            var imageIdText = tokenSpan[tokenLength..endIndex];
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
                || ((imageId, page) is var key && !viewModel.IllustrationImages.ContainsKey(key)))
            {
                position += tokenLength;
                return;
            }

            AddPixivImage(result, viewModel, imageId, page);

            var end = endIndex + 1;
            position += end;
            runStart = position;
        }

        void AddLastRun(int end)
        {
            if (end <= runStart)
                return;

            AddLastSpan(result, text[runStart..end]);
            runStart = end;
        }
    }
}

file enum PixivToken
{
    None,
    PixivImage,
    UploadedImage,
    Chapter,
    Jump,
    JumpUri,
    Ruby,
    NewPage
}

file static class PixivTokens
{
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

    public static PixivToken Match(ReadOnlySpan<char> text)
    {
        if (text.Length < 2)
            return PixivToken.None;

        if (text[1] is '[')
        {
            if (text.StartsWith(JumpUriToken, StringComparison.Ordinal))
                return PixivToken.JumpUri;
            if (text.StartsWith(RubyToken, StringComparison.Ordinal))
                return PixivToken.Ruby;

            return PixivToken.None;
        }

        return text[1] switch
        {
            'p' when text.StartsWith(PixivImageToken, StringComparison.Ordinal) => PixivToken.PixivImage,
            'u' when text.StartsWith(UploadedImageToken, StringComparison.Ordinal) => PixivToken.UploadedImage,
            'c' when text.StartsWith(ChapterToken, StringComparison.Ordinal) => PixivToken.Chapter,
            'j' when text.StartsWith(JumpToken, StringComparison.Ordinal) => PixivToken.Jump,
            'n' when text.StartsWith(NewPageToken, StringComparison.Ordinal) => PixivToken.NewPage,
            _ => PixivToken.None
        };
    }

    public static int GetLength(PixivToken token)
        => token switch
        {
            PixivToken.PixivImage => PixivImageToken.Length,
            PixivToken.UploadedImage => UploadedImageToken.Length,
            PixivToken.Chapter => ChapterToken.Length,
            PixivToken.Jump => JumpToken.Length,
            PixivToken.JumpUri => JumpUriToken.Length,
            PixivToken.Ruby => RubyToken.Length,
            PixivToken.NewPage => NewPageToken.Length,
            _ => 0
        };
}
