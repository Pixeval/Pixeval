#region Copyright (c) Pixeval/Pixeval.LoginProxy

// GPL v3 License
// 
// Pixeval/Pixeval.LoginProxy
// Copyright (c) 2021 Pixeval.LoginProxy/LoginTokenRequest.cs
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

using System.Text.Json.Serialization;

namespace Pixeval.LoginProxy
{
    public class LoginTokenRequest
    {
        [JsonPropertyName("errno")]
        public int Errno { get; set; }

        [JsonPropertyName("cookie")]
        public string? Cookie { get; set; }

        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("verifier")]
        public string? Verifier { get; set; }
    }
}