#region Copyright (c) Pixeval/Mako

// MIT License
// 
// Copyright (c) Pixeval 2021 Mako/RefreshTokenSessionUpdate.cs
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

using System.Threading.Tasks;
using Pixeval.CoreApi.Net.EndPoints;
using Pixeval.CoreApi.Net.Request;

namespace Pixeval.CoreApi.Preference
{
    public class RefreshTokenSessionUpdate : ISessionUpdate
    {
        public async Task<Session> RefreshAsync(MakoClient makoClient)
        {
            return (await makoClient.Resolve<IAuthEndPoint>().RefreshAsync(new RefreshSessionRequest(makoClient.Session.RefreshToken)).ConfigureAwait(false))
                .ToSession() with
                {
                    Cookie = makoClient.Session.Cookie,
                    CookieCreation = makoClient.Session.CookieCreation
                };
        }
    }
}