// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.IO;
using Microsoft.UI.Xaml.Media;
using Pixeval.Util.UI;

namespace Pixeval.Pages;

public record PixivReplyEmojiViewModel(PixivReplyEmoji EmojiEnumValue, Stream ImageStream, ImageSource ImageSource);
