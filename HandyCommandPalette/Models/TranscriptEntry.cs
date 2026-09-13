// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace HandyCommandPalette.Models;

public sealed record TranscriptEntry(
    long Id,
    string FileName,
    long Timestamp,
    bool Saved,
    string Title,
    string TranscriptionText,
    string? PostProcessedText,
    string? PostProcessPrompt,
    bool PostProcessRequested)
{
    public string BestText => !string.IsNullOrWhiteSpace(PostProcessedText)
        ? PostProcessedText
        : TranscriptionText;

    public DateTime LocalTimestamp =>
        DateTimeOffset.FromUnixTimeSeconds(Timestamp).ToLocalTime().DateTime;

    public string FormattedTime => LocalTimestamp.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
}
