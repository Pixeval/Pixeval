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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Pixeval.Objects.Primitive
{
    public static class Strings
    {
        public static byte[] GetBytes(this string str, Encoding encoding = null)
        {
            return encoding == null ? Encoding.UTF8.GetBytes(str) : encoding.GetBytes(str);
        }

        public static string GetString(this byte[] data, Encoding encoding = null)
        {
            return encoding == null ? Encoding.UTF8.GetString(data) : encoding.GetString(data);
        }

        public static string ToJson<T>(this T obj)
        {
            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings { Formatting = Formatting.Indented });
        }

        public static T FromJson<T>(this string src)
        {
            return JsonConvert.DeserializeObject<T>(src);
        }

        public static string Join<T>(this IEnumerable<T> enumerable, Func<T, string> transformer, char delimiter)
        {
            return string.Join(delimiter, enumerable.Select(transformer));
        }

        public static string Join(this IEnumerable<string> enumerable, char delimiter)
        {
            return string.Join(delimiter, enumerable);
        }

        public static bool IsNullOrEmpty(this string src)
        {
            return string.IsNullOrEmpty(src);
        }

        public static bool EqualsIgnoreCase(this string str1, string str2)
        {
            return string.Equals(str1, str2, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsNumber(this string str)
        {
            return int.TryParse(str, out _);
        }

        // see https://stackoverflow.com/questions/146134/how-to-remove-illegal-characters-from-path-and-filenames
        public static string FormatPath(string original)
        {
            return string.Concat(original?.Split(Path.GetInvalidFileNameChars()) ?? Array.Empty<string>());
        }

        public static string AssumeImageContentType(string fileName)
        {
            return fileName[^3..] switch
            {
                "png"  => "image/png",
                "jpg"  => "image/jpeg",
                "jpeg" => "image/jpeg",
                "gif"  => "image/gif",
                _      => "image/jpeg"
            };
        }

        public static string Hash<T>(this string str, Encoding encoding = null) where T : HashAlgorithm, new()
        {
            return str.HashBytes<T>(encoding).Select(b => b.ToString("x2")).Aggregate((s1, s2) => s1 + s2);
        }

        public static byte[] HashBytes<T>(this string str, Encoding encoding = null) where T : HashAlgorithm, new()
        {
            using var crypt = new T();
            var hashBytes = crypt.ComputeHash(str.GetBytes(encoding));
            return hashBytes;
        }

        public static string ToUrlSafeBase64String(this byte[] bytes)
        {
            return Convert.ToBase64String(bytes).TrimEnd(new[] { '=' }).Replace("+", "-").Replace("/", "_");
        }
        
        public static string RegexReplace(this string str, Regex regex, string pattern)
        {
            return regex.Replace(str, pattern);
        }

        public static string RemoveCommonSubstringFromEnd(this string s1, string s2)
        {
            var sb = new StringBuilder();
            if (s1 == s2) return s2;
            for (var i = 0; i < s1.Length;)
            {
                var j = i++;
                foreach (var c in s2)
                {
                    if (j >= s1.Length)
                    {
                        return sb.Length == 0 ? s2 : new Regex(Regex.Escape(sb.ToString())).Replace(s2, string.Empty, 1);
                    }
                    if (c == s1[j++])
                    {
                        sb.Append(c);
                    }
                    else
                    {
                        sb.Clear();
                        break;
                    }
                }
            }
            return sb.Length == 0 ? s2 : new Regex(Regex.Escape(sb.ToString())).Replace(s2, string.Empty, 1);
        }

        public static string RemoveUntil(this string str, char ch, int time = 1, char? before = null)
        {
            if (!str.Contains(ch))
                return str;
            if (before.HasValue && str.IndexOf(before.Value) is { } bIndex && bIndex != -1 && !str.Substring(0, bIndex).Contains(ch))
                return str;
            var t = 0;
            var sb = new StringBuilder();
            foreach (var c in str)
            {
                if (t >= time)
                    sb.Append(c);
                else if (c == ch)
                    ++t;
            }
            return sb.ToString();
        }
    }
}