﻿#region Copyright (c) Pixeval/Pixeval.CoreApi

// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2021 Pixeval.CoreApi/ReverseSearchRequest.cs
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

using Refit;

namespace Pixeval.CoreApi.Net.Request;

internal class ReverseSearchRequest
{
    public ReverseSearchRequest(string apiKey)
    {
        ApiKey = apiKey;
        DbMask = "96";
        OutputType = "2";
        NumberResult = "1";
    }

    [AliasAs("api_key")]
    public string ApiKey { get; }

    [AliasAs("dbmask")]
    public string DbMask { get; }

    [AliasAs("output_type")]
    public string OutputType { get; }

    [AliasAs("numres")]
    public string NumberResult { get; }
}