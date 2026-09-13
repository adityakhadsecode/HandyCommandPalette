// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace HandyCommandPalette.Models;

public sealed record HandyModelInfo(
    string Id,
    string Name,
    string Description,
    string Filename,
    bool IsDirectory,
    bool SupportsLanguageSelection,
    bool IsDownloaded,
    bool IsActive = false);

