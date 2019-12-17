// Pixeval
// Copyright (C) 2019 Dylech30th <decem0730@gmail.com>
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
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Pixeval.Objects
{
    public static class Texts
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
            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            });
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
    }
}