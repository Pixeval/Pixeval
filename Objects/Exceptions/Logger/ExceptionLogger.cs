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
using System;
using System.Globalization;
using System.IO;
using Pixeval.Persisting;

namespace Pixeval.Objects.Exceptions.Logger
{
    public class ExceptionLogger
    {
        public static void WriteException(Exception e)
        {
            File.WriteAllTextAsync(Path.Combine(PixevalEnvironment.ExceptionReportFolder, $"{DateTime.Now.ToString(CultureInfo.InvariantCulture)}.txt".Replace("/", "-").Replace(":", "-")), e.ToString());
        }
    }
}