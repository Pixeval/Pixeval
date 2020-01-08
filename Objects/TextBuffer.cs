// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019 Dylech30th
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
using System.IO;

namespace Pixeval.Objects
{
    internal class TextBuffer
    {
        public static FileInfo GetOrCreateFile(string path)
        {
            if (!File.Exists(path)) File.Create(path);
            return new FileInfo(path);
        }

        public static string GetOrCreateDirectory(string path)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            return path;
        }

        public static string GetExtension(string file)
        {
            return file[file.LastIndexOf('.')..];
        }

        public static void WriteAllBytesTaskAsync(string path, byte[] bytes)
        {
            File.WriteAllBytesAsync(path, bytes);
        }
    }
}