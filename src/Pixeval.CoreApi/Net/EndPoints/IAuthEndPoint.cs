#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2023 Pixeval.CoreApi/IAuthEndPoint.cs
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
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System.Threading.Tasks;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net.Request;
using Refit;

namespace Pixeval.CoreApi.Net.EndPoints;

[Headers("User-Agent: PixivAndroidApp/5.0.64 (Android 6.0)", "Content-Type: application/x-www-form-urlencoded")]
internal interface IAuthEndPoint
{
    [Post("/auth/token")]
    Task<TokenResponse> RefreshAsync([Body(BodySerializationMethod.UrlEncoded)] RefreshSessionRequest request);
}