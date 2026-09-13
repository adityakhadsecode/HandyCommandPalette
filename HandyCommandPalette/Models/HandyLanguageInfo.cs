// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace HandyCommandPalette.Models;

public sealed record HandyLanguageInfo(
    string Code,
    string EnglishName,
    string NativeName,
    bool IsActive = false)
{
    public string DisplayTitle => Code.Equals("auto", System.StringComparison.OrdinalIgnoreCase)
        ? "Auto (Automatic detection)"
        : $"{EnglishName} ({NativeName})";

    public string DisplaySubtitle => Code.Equals("auto", System.StringComparison.OrdinalIgnoreCase)
        ? "Detect language automatically"
        : $"Code: {Code}";
}

