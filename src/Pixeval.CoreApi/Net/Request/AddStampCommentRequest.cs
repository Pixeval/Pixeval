#region Copyright

// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2024 Pixeval.CoreApi/AddStampCommentRequest.cs
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

namespace Pixeval.CoreApi.Net.Request;

public record AddStampIllustCommentRequest(
    [property: JsonPropertyName("illust_id")] long Id,
    [property: JsonPropertyName("parent_comment_id")] long? ParentCommentId,
    [property: JsonPropertyName("stamp_id")] int StampId
);

public record AddStampNovelCommentRequest(
    [property: JsonPropertyName("novel_id")] long Id,
    [property: JsonPropertyName("parent_comment_id")] long? ParentCommentId,
    [property: JsonPropertyName("stamp_id")] int StampId
);
