// Copyright (c) Pixeval.Caching.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Caching;

public record struct CacheToken(int MaxMemoryMappedFileCount, int MemoryMappedFileInitialSize, string CacheDirectory, nint Align);
