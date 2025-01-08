// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.CoreApi.Global.Enum;

namespace Pixeval.Messages;

/// <summary>
/// Raises when the tag in WorkInfoPage is clicked
/// </summary>
public record WorkTagClickedMessage(SimpleWorkType Type, string Tag);
