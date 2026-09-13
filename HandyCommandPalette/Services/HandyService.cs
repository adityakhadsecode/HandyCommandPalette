// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HandyCommandPalette.Models;

namespace HandyCommandPalette.Services;

public sealed class HandyService : IHandyService
{
    private static readonly Lazy<HandyService> _instance = new(() => new HandyService());
    public static HandyService Instance => _instance.Value;

    private readonly HandyProcessService _processService = new();
    private readonly HandySettingsService _settingsService = new();
    private readonly HandyHistoryService _historyService = new();

    private static readonly HandyModelInfo[] KnownModels =
    [
        new("handy-computer/parakeet-unified-en-0.6b-gguf/parakeet-unified-en-0.6b-Q8_0.gguf", "Parakeet Unified 0.6B", "Fast CPU-optimized unified English model", "parakeet-unified-en-0.6b-Q8_0.gguf", false, false, true),
        new("small", "Whisper Small", "Fast and fairly accurate Whisper model", "ggml-small.bin", false, true, false),
        new("medium", "Whisper Medium", "Good accuracy, balanced speed", "whisper-medium-q4_1.bin", false, true, false),
        new("turbo", "Whisper Turbo", "Large V3 Turbo balanced speed and accuracy", "ggml-large-v3-turbo.bin", false, true, false),
        new("large", "Whisper Large", "Maximum accuracy Whisper model", "ggml-large-v3-q5_0.bin", false, true, false),
        new("parakeet-tdt-0.6b-v2", "Parakeet V2", "English CPU-optimized speech model", "parakeet-tdt-0.6b-v2-int8", true, false, false),
        new("parakeet-tdt-0.6b-v3", "Parakeet V3", "25 European languages speech model", "parakeet-tdt-0.6b-v3-int8", true, false, false),
        new("breeze-asr", "Breeze ASR", "Taiwanese Mandarin & English code-switching", "breeze-asr-q5_k.bin", false, true, false),
        new("moonshine-base", "Moonshine Base", "Ultra-fast English model", "moonshine-base", true, false, false),
        new("sense-voice-int8", "SenseVoice", "Multilingual: ZH/EN/JA/KO/Cantonese", "sense-voice-int8", true, true, false),
        new("canary-180m-flash", "Canary 180M Flash", "Fast multilingual model", "canary-180m-flash", true, true, false),
        new("canary-1b-v2", "Canary 1B v2", "Accurate 25 European languages model", "canary-1b-v2", true, true, false),
    ];

    private static readonly HandyLanguageInfo[] SupportedLanguages =
    [
        new("auto", "Auto", "Auto (Automatic detection)"),
        new("en", "English", "English"),
        new("es", "Spanish", "Español"),
        new("fr", "French", "Français"),
        new("de", "German", "Deutsch"),
        new("it", "Italian", "Italiano"),
        new("pt", "Portuguese", "Português"),
        new("zh", "Chinese", "中文"),
        new("ja", "Japanese", "日本語"),
        new("ko", "Korean", "한국어"),
        new("ru", "Russian", "Русский"),
        new("ar", "Arabic", "العربية"),
        new("hi", "Hindi", "हिन्दी"),
        new("nl", "Dutch", "Nederlands"),
        new("pl", "Polish", "Polski"),
        new("tr", "Turkish", "Türkçe"),
        new("uk", "Ukrainian", "Українська"),
        new("vi", "Vietnamese", "Tiếng Việt"),
        new("sv", "Swedish", "Svenska"),
        new("cs", "Czech", "Čeština"),
        new("da", "Danish", "Dansk"),
        new("fi", "Finnish", "Suomi"),
        new("el", "Greek", "Ελληνικά"),
        new("he", "Hebrew", "עברית"),
        new("id", "Indonesian", "Bahasa Indonesia"),
        new("ro", "Romanian", "Română"),
        new("hu", "Hungarian", "Magyar"),
        new("no", "Norwegian", "Norsk"),
        new("th", "Thai", "ไทย"),
        new("bg", "Bulgarian", "Български"),
        new("hr", "Croatian", "Hrvatski"),
        new("sk", "Slovak", "Slovenčina"),
        new("sl", "Slovenian", "Slovenščina"),
        new("et", "Estonian", "Eesti"),
        new("lt", "Lithuanian", "Lietuvių"),
        new("lv", "Latvian", "Latviešu"),
        new("ta", "Tamil", "தமிழ்"),
        new("te", "Telugu", "తెలుగు"),
        new("ur", "Urdu", "اردو"),
    ];

    public Task<bool> IsHandyRunningAsync() => Task.FromResult(HandyProcessService.IsHandyRunning());

    public Task<(bool Success, string Message)> ToggleRecordingAsync() =>
        HandyProcessService.ToggleRecordingAsync();

    public Task<TranscriptEntry?> GetLastTranscriptAsync() =>
        HandyHistoryService.GetLastCompletedTranscriptAsync();

    public async Task<(bool Success, string Message)> CopyLastTranscriptAsync()
    {
        var transcript = await GetLastTranscriptAsync();
        if (transcript == null || string.IsNullOrWhiteSpace(transcript.BestText))
        {
            return (false, "No previous Handy transcript found.");
        }

        bool copied = ClipboardService.SetText(transcript.BestText);
        if (copied)
        {
            return (true, "Last transcript copied to clipboard.");
        }

        return (false, "Failed to copy transcript to clipboard.");
    }

    public async Task<(bool Success, string Message)> PasteLastTranscriptAsync()
    {
        var transcript = await GetLastTranscriptAsync();
        if (transcript == null || string.IsNullOrWhiteSpace(transcript.BestText))
        {
            return (false, "No previous Handy transcript found.");
        }

        bool pasted = await ClipboardService.PasteTextNonDestructiveAsync(transcript.BestText);
        if (pasted)
        {
            return (true, "Pasted transcript into active window.");
        }

        return (false, "Failed to paste transcript.");
    }

    public Task<string[]> GetCustomWordsAsync() => _settingsService.GetCustomWordsAsync();

    public Task<(bool Success, string Message)> AddCustomWordAsync(string word) =>
        _settingsService.AddCustomWordAsync(word);

    public Task<(bool Success, string Message)> RemoveCustomWordAsync(string word) =>
        _settingsService.RemoveCustomWordAsync(word);

    public async Task<HandyModelInfo[]> GetModelsAsync()
    {
        var selected = await GetSelectedModelAsync();
        var modelsDir = HandyPathResolver.GetModelsDirectory();

        var result = new List<HandyModelInfo>();
        var seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var model in KnownModels)
        {
            bool downloaded = model.IsDownloaded;
            if (!downloaded && Directory.Exists(modelsDir))
            {
                var filePath = Path.Combine(modelsDir, model.Filename);
                downloaded = File.Exists(filePath) || Directory.Exists(filePath);
            }

            bool isActive = string.Equals(model.Id, selected, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(model.Filename, selected, StringComparison.OrdinalIgnoreCase);

            seenIds.Add(model.Id);
            seenIds.Add(model.Filename);

            result.Add(model with { IsDownloaded = downloaded, IsActive = isActive });
        }

        // Also check models directory for custom models
        if (Directory.Exists(modelsDir))
        {
            try
            {
                var entries = Directory.GetFileSystemEntries(modelsDir);
                foreach (var entry in entries)
                {
                    var name = Path.GetFileName(entry);
                    if (!seenIds.Contains(name))
                    {
                        bool isDir = Directory.Exists(entry);
                        bool isActive = string.Equals(name, selected, StringComparison.OrdinalIgnoreCase);
                        result.Add(new(name, name, "Custom model in models directory", name, isDir, true, true, isActive));
                    }
                }
            }
            catch
            {
                // Fallback
            }
        }

        return result.ToArray();
    }

    public Task<string> GetSelectedModelAsync() => _settingsService.GetSelectedModelAsync();

    public Task<(bool Success, string Message)> SelectModelAsync(string modelId) =>
        _settingsService.SetSelectedModelAsync(modelId);

    public async Task<HandyLanguageInfo[]> GetLanguagesAsync()
    {
        var selected = await GetSelectedLanguageAsync();
        return SupportedLanguages.Select(lang =>
            lang with { IsActive = string.Equals(lang.Code, selected, StringComparison.OrdinalIgnoreCase) })
            .ToArray();
    }

    public Task<string> GetSelectedLanguageAsync() => _settingsService.GetSelectedLanguageAsync();

    public Task<(bool Success, string Message)> SelectLanguageAsync(string langCode) =>
        _settingsService.SetSelectedLanguageAsync(langCode);

    public string GetRecordingsDirectory() => HandyPathResolver.GetRecordingsDirectory();

    public Task<TranscriptEntry[]> SearchTranscriptsAsync(string query, int limit = 100) =>
        HandyHistoryService.SearchTranscriptsAsync(query, limit);
}
