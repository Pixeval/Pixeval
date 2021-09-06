#region Copyright (c) Pixeval/Mako

// MIT License
// 
// Copyright (c) Pixeval 2021 Mako/MakoClientConfiguration.cs
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

using System.Globalization;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Pixeval.CoreApi.Preference
{
    /// <summary>
    ///     Contains all the user-configurable keys
    /// </summary>
    [PublicAPI]
    public record MakoClientConfiguration
    {
        public MakoClientConfiguration()
        {
            CultureInfo = CultureInfo.CurrentCulture;
            ConnectionTimeout = 5000;
            Bypass = false;
            MirrorHost = string.Empty;
        }

        public MakoClientConfiguration(int connectionTimeout, bool bypass, string? mirrorHost, CultureInfo cultureInfo)
        {
            ConnectionTimeout = connectionTimeout;
            Bypass = bypass;
            MirrorHost = mirrorHost;
            CultureInfo = cultureInfo;
        }

        [JsonIgnore]
        public CultureInfo CultureInfo { get; set; }

        [JsonPropertyName("connectionTimeout")]
        public int ConnectionTimeout { get; set; }

        /// <summary>
        ///     Automatically bypass GFW or not, default is set to true.
        ///     If you are currently living in China Mainland, turn it on to make sure
        ///     you can use Mako without using any kind of proxy, otherwise you will
        ///     need a proper proxy server to bypass the GFW
        /// </summary>
        [JsonPropertyName("bypass")]
        public bool Bypass { get; set; }

        /// <summary>
        ///     Mirror server's host of image downloading
        /// </summary>
        [JsonPropertyName("mirrorHost")]
        public string? MirrorHost { get; set; }
    }
}