#region Copyright (C) 2019-2020 Dylech30th. All rights reserved.
// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019-2020 Dylech30th
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Pixeval.Data.ViewModel;
using Pixeval.Objects.Primitive;

namespace Pixeval.Core
{
    public class DownloadPathMacros
    {
        public class MacroToken
        {
            public MacroToken(string macro, bool isConditional)
            {
                Macro = macro;
                IsConditional = isConditional;
            }

            public string Macro { get; }
            
            public bool IsConditional { get;  }
        }
        
        private static class Macros
        {
            public const string MacroIllustId = "{illust.id}";
            public const string MacroIllustTitle = "{illust.title}";
            public const string MacroSpotlightId = "{spot.id}";
            public const string MacroSpotlightTitle = "{spot.title}";
            public const string MacroMangaIndex = "{manga.index}";
            public const string MacroIllustExt = "{illust.ext}";
            public const string MacroUserId = "{user.id}";
            public const string MacroUserName = "{user.name}";
            
            public static readonly Regex MacroConditionIfManga = new Regex("\\{if\\:manga\\:(.+?\\}*)\\}");
            public static readonly Regex MacroConditionIfNotManga = new Regex("\\{ifn\\:manga\\:(.+?\\}*)\\}");
            public static readonly Regex MacroConditionIfIllust = new Regex("\\{if\\:illust\\:(.+?\\}*)\\}");
            public static readonly Regex MacroConditionIfNotIllust = new Regex("\\{ifn\\:illust\\:(.+?\\}*)\\}");
            public static readonly Regex MacroConditionIfUgoira = new Regex("\\{if\\:ugo\\:(.+?\\}*)\\}");
            public static readonly Regex MacroConditionIfNotUgoira = new Regex("\\{ifn\\:ugo\\:(.+?\\}*)\\}");
            public static readonly Regex MacroConditionIfSpotlight = new Regex("\\{if\\:spot\\:(.+?\\}*)\\}");
            public static readonly Regex MacroConditionIfNotSpotlight = new Regex("\\{ifn\\:spot\\:(.+?\\}*)\\}");
        }

        public static string FormatDownloadPath(string pathWithMacro, Illustration illustration)
        {
            const char Separator = '\\';
            if (!pathWithMacro.EndsWith(Macros.MacroIllustExt))
            {
                pathWithMacro += $"{(pathWithMacro.EndsWith(".") ? string.Empty : ".")}{Macros.MacroIllustExt}";
            }
            var segments = pathWithMacro.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
            // the directory and filesystem will behaves weird once removed the Select(p => p.Trim()) call, if the directory contains trailing
            // spaces, it will be separated into two directories in File Explorer, one with trailing spaces preserved, and one not, but they're
            // actually the same directory, if you deleted one of them, the other one will unable to be deleted
            return Path.Combine(segments.Select(ReplaceMacro).Select(p => p.Trim()).Where(p => !p.IsNullOrEmpty()).ToArray());


            string ReplaceMacro(string s)
            {
                const string Replacement = "$1";
                s = illustration switch
                {
                    { IsManga: true } => s.RegexReplace(Macros.MacroConditionIfManga, Replacement)
                        .RegexReplace(Macros.MacroConditionIfNotManga, string.Empty)
                        .RegexReplace(Macros.MacroConditionIfIllust, string.Empty)
                        .RegexReplace(Macros.MacroConditionIfNotIllust, Replacement)
                        .RegexReplace(Macros.MacroConditionIfUgoira, string.Empty)
                        .RegexReplace(Macros.MacroConditionIfNotUgoira, Replacement),
                    { IsUgoira: true } => s.RegexReplace(Macros.MacroConditionIfManga, string.Empty)
                        .RegexReplace(Macros.MacroConditionIfNotManga, Replacement)
                        .RegexReplace(Macros.MacroConditionIfIllust, string.Empty)
                        .RegexReplace(Macros.MacroConditionIfNotIllust, Replacement)
                        .RegexReplace(Macros.MacroConditionIfUgoira, Replacement)
                        .RegexReplace(Macros.MacroConditionIfNotUgoira, string.Empty),
                    _ => s.RegexReplace(Macros.MacroConditionIfManga, string.Empty)
                        .RegexReplace(Macros.MacroConditionIfNotManga, Replacement)
                        .RegexReplace(Macros.MacroConditionIfIllust, Replacement)
                        .RegexReplace(Macros.MacroConditionIfNotIllust, string.Empty)
                        .RegexReplace(Macros.MacroConditionIfUgoira, string.Empty)
                        .RegexReplace(Macros.MacroConditionIfNotUgoira, Replacement)
                };

                s = s.RegexReplace(Macros.MacroConditionIfSpotlight, illustration.FromSpotlight ? Replacement : string.Empty)
                    .RegexReplace(Macros.MacroConditionIfNotSpotlight, illustration.FromSpotlight ? string.Empty : Replacement);

                return s.Replace(Macros.MacroIllustId, illustration.Id)
                    .Replace(Macros.MacroIllustTitle, Strings.FormatPath(illustration.Title))
                    .Replace(Macros.MacroMangaIndex, (illustration.IsManga ? illustration.MangaMetadata.ToList().IndexOf(illustration) : 0).ToString())
                    .Replace(Macros.MacroSpotlightTitle, Strings.FormatPath(illustration.SpotlightTitle))
                    .Replace(Macros.MacroSpotlightId, illustration.SpotlightArticleId)
                    .Replace(Macros.MacroIllustExt, Path.GetExtension(illustration.GetDownloadUrl())![1..])
                    .Replace(Macros.MacroUserId, illustration.UserId)
                    .Replace(Macros.MacroUserName, Strings.FormatPath(illustration.UserName));
            }
        }

        private static readonly Lazy<IReadOnlyList<FieldInfo>> MacroFields = new Lazy<IReadOnlyList<FieldInfo>>(() => typeof(Macros).GetFields(BindingFlags.Public | BindingFlags.Static));

        private static readonly Lazy<IReadOnlyList<string>> MacroStrings = new Lazy<IReadOnlyList<string>>(
            () => MacroFields.Value.Select(
                f => f.FieldType == typeof(string)
                    ? (string) f.GetValue(null)
                    : f.FieldType == typeof(Regex)
                        ? ((Regex) f.GetValue(null))!.ToString()
                        : null
            ).Where(s => !s.IsNullOrEmpty()).ToList()
        );
        
        public static IReadOnlyList<string> GetMacros()
        {
            return MacroStrings.Value;
        }

        public static IReadOnlyList<MacroToken> GetEscapedMacros()
        {
            var list = MacroFields.Value.Where(f => f.FieldType == typeof(string)).Select(f => new MacroToken((string) f.GetValue(null), false)).ToList();
            list.Add(new MacroToken("{if:manga:}", true));
            list.Add(new MacroToken("{ifn:manga:}", true));
            list.Add(new MacroToken("{if:illust:}", true));
            list.Add(new MacroToken("{ifn:illust:}", true));
            list.Add(new MacroToken("{if:ugoira:}", true));
            list.Add(new MacroToken("{ifn:ugoira:}", true));
            list.Add(new MacroToken("{if:spot:}", true));
            list.Add(new MacroToken("{ifn:spot:}", true));
            return list;
        }
    }
}