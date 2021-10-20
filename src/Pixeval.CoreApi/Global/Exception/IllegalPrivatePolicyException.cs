#region Copyright (c) Pixeval/Mako

// MIT License
// 
// Copyright (c) Pixeval 2021 Mako/IllegalPrivatePolicyException.cs
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

using System.Runtime.Serialization;
using JetBrains.Annotations;
using Pixeval.CoreApi.Global.Enum;

namespace Pixeval.CoreApi.Global.Exception
{
    /// <summary>
    ///     When a <see cref="PrivacyPolicy" /> is set to <see cref="PrivacyPolicy.Private" /> while the uid is not equivalent
    ///     to the <see cref="MakoClient.Session" />
    /// </summary>
    [PublicAPI]
    public class IllegalPrivatePolicyException : MakoException
    {
        public IllegalPrivatePolicyException(string uid)
        {
            Uid = uid;
        }

        protected IllegalPrivatePolicyException(SerializationInfo info, StreamingContext context, string uid) : base(info, context)
        {
            Uid = uid;
        }

        public IllegalPrivatePolicyException(string? message, string uid) : base(message)
        {
            Uid = uid;
        }

        public IllegalPrivatePolicyException(string? message, System.Exception? innerException, string uid) : base(message, innerException)
        {
            Uid = uid;
        }

        public string Uid { get; }
    }
}