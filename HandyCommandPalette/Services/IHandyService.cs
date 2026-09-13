// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using HandyCommandPalette.Models;

namespace HandyCommandPalette.Services;

public interface IHandyService
{
    Task<bool> IsHandyRunningAsync();

    Task<(bool Success, string Message)> ToggleRecordingAsync();

    Task<TranscriptEntry?> GetLastTranscriptAsync();

    Task<(bool Success, string Message)> CopyLastTranscriptAsync();

    Task<(bool Success, string Message)> PasteLastTranscriptAsync();

    Task<string[]> GetCustomWordsAsync();

    Task<(bool Success, string Message)> AddCustomWordAsync(string word);

    Task<(bool Success, string Message)> RemoveCustomWordAsync(string word);

    Task<HandyModelInfo[]> GetModelsAsync();

    Task<string> GetSelectedModelAsync();

    Task<(bool Success, string Message)> SelectModelAsync(string modelId);

    Task<HandyLanguageInfo[]> GetLanguagesAsync();

    Task<string> GetSelectedLanguageAsync();

    Task<(bool Success, string Message)> SelectLanguageAsync(string langCode);

    string GetRecordingsDirectory();

    Task<TranscriptEntry[]> SearchTranscriptsAsync(string query, int limit = 100);
}

