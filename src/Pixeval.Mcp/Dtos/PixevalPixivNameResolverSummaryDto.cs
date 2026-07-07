// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalPixivNameResolverSummaryDto(
    int AppApi,
    int WebApi,
    int Account,
    int OAuth,
    int Image,
    int Image2);