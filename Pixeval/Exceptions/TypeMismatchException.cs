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
using System.Runtime.Serialization;

namespace Pixeval.Exceptions
{
    internal class TypeMismatchException : Exception
    {
        public TypeMismatchException() { }

        protected TypeMismatchException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public TypeMismatchException(string message) : base(message) { }

        public TypeMismatchException(string message, Exception innerException) : base(message, innerException) { }
    }
}