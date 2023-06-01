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

namespace Pixeval.Data.Web
{
    public class HttpResponse<T> : Tuple<bool, T>
    {
        private HttpResponse(bool status, T response) : base(status, response)
        {
        }

        public static HttpResponse<T> Wrap(bool status)
        {
            return new HttpResponse<T>(status, default);
        }

        public static HttpResponse<T> Wrap(bool status, T response)
        {
            return new HttpResponse<T>(status, response);
        }

        public void Deconstruct(out bool status, out T response)
        {
            status = Item1;
            response = Item2;
        }
    }
}