// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace HandyCommandPalette.Services;

public sealed class HandySettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
    };

    private readonly object _lock = new();

    private static JsonObject? LoadRootNode()
    {
        var settingsPath = HandyPathResolver.GetSettingsPath();
        if (!File.Exists(settingsPath))
        {
            return null;
        }

        try
        {
            var text = File.ReadAllText(settingsPath);
            var node = JsonNode.Parse(text);
            return node as JsonObject;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[HandySettingsService] Error reading settings: {ex.Message}");
            return null;
        }
    }

    private static bool SaveRootNode(JsonObject root)
    {
        var settingsPath = HandyPathResolver.GetSettingsPath();
        var tempPath = settingsPath + ".tmp";

        try
        {
            var dir = Path.GetDirectoryName(settingsPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var jsonString = root.ToJsonString(JsonOptions);
            File.WriteAllText(tempPath, jsonString);
            File.Move(tempPath, settingsPath, overwrite: true);
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[HandySettingsService] Error writing settings: {ex.Message}");
            try
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
            catch
            {
                // Cleanup fallback
            }

            return false;
        }
    }

    public Task<string[]> GetCustomWordsAsync()
    {
        lock (_lock)
        {
            var root = LoadRootNode();
            if (root?["settings"] is JsonObject settingsObj &&
                settingsObj["custom_words"] is JsonArray wordsArray)
            {
                var list = new List<string>();
                foreach (var item in wordsArray)
                {
                    var val = item?.GetValue<string>();
                    if (!string.IsNullOrWhiteSpace(val))
                    {
                        list.Add(val.Trim());
                    }
                }

                return Task.FromResult(list.ToArray());
            }

            return Task.FromResult(Array.Empty<string>());
        }
    }

    public Task<(bool Success, string Message)> AddCustomWordAsync(string word)
    {
        if (string.IsNullOrWhiteSpace(word))
        {
            return Task.FromResult((false, "Word cannot be empty."));
        }

        var trimmed = word.Trim();

        lock (_lock)
        {
            var root = LoadRootNode() ?? new JsonObject();
            if (root["settings"] is not JsonObject settingsObj)
            {
                settingsObj = new JsonObject();
                root["settings"] = settingsObj;
            }

            if (settingsObj["custom_words"] is not JsonArray wordsArray)
            {
                wordsArray = new JsonArray();
                settingsObj["custom_words"] = wordsArray;
            }

            foreach (var item in wordsArray)
            {
                if (string.Equals(item?.GetValue<string>(), trimmed, StringComparison.OrdinalIgnoreCase))
                {
                    return Task.FromResult((false, $"'{trimmed}' is already in the dictionary."));
                }
            }

            wordsArray.Add((JsonNode)JsonValue.Create(trimmed)!);

            if (SaveRootNode(root))
            {
                return Task.FromResult((true, $"Added '{trimmed}' to Handy dictionary."));
            }

            return Task.FromResult((false, "Failed to persist dictionary setting."));
        }
    }

    public Task<(bool Success, string Message)> RemoveCustomWordAsync(string word)
    {
        if (string.IsNullOrWhiteSpace(word))
        {
            return Task.FromResult((false, "Word cannot be empty."));
        }

        var trimmed = word.Trim();

        lock (_lock)
        {
            var root = LoadRootNode();
            if (root?["settings"] is not JsonObject settingsObj ||
                settingsObj["custom_words"] is not JsonArray wordsArray)
            {
                return Task.FromResult((false, "Dictionary is empty."));
            }

            int indexToRemove = -1;
            for (int i = 0; i < wordsArray.Count; i++)
            {
                if (string.Equals(wordsArray[i]?.GetValue<string>(), trimmed, StringComparison.OrdinalIgnoreCase))
                {
                    indexToRemove = i;
                    break;
                }
            }

            if (indexToRemove >= 0)
            {
                wordsArray.RemoveAt(indexToRemove);
                if (SaveRootNode(root))
                {
                    return Task.FromResult((true, $"Removed '{trimmed}' from dictionary."));
                }

                return Task.FromResult((false, "Failed to save updated dictionary."));
            }

            return Task.FromResult((false, $"Word '{trimmed}' not found in dictionary."));
        }
    }

    public Task<string> GetSelectedModelAsync()
    {
        lock (_lock)
        {
            var root = LoadRootNode();
            if (root?["settings"] is JsonObject settingsObj &&
                settingsObj["selected_model"] is JsonValue modelVal)
            {
                return Task.FromResult(modelVal.GetValue<string>());
            }

            return Task.FromResult(string.Empty);
        }
    }

    public Task<(bool Success, string Message)> SetSelectedModelAsync(string modelId)
    {
        lock (_lock)
        {
            var root = LoadRootNode() ?? new JsonObject();
            if (root["settings"] is not JsonObject settingsObj)
            {
                settingsObj = new JsonObject();
                root["settings"] = settingsObj;
            }

            settingsObj["selected_model"] = JsonValue.Create(modelId);

            if (SaveRootNode(root))
            {
                return Task.FromResult((true, $"Switched transcription model to '{modelId}'."));
            }

            return Task.FromResult((false, "Failed to save model selection."));
        }
    }

    public Task<string> GetSelectedLanguageAsync()
    {
        lock (_lock)
        {
            var root = LoadRootNode();
            if (root?["settings"] is JsonObject settingsObj &&
                settingsObj["selected_language"] is JsonValue langVal)
            {
                return Task.FromResult(langVal.GetValue<string>());
            }

            return Task.FromResult("auto");
        }
    }

    public Task<(bool Success, string Message)> SetSelectedLanguageAsync(string langCode)
    {
        lock (_lock)
        {
            var root = LoadRootNode() ?? new JsonObject();
            if (root["settings"] is not JsonObject settingsObj)
            {
                settingsObj = new JsonObject();
                root["settings"] = settingsObj;
            }

            settingsObj["selected_language"] = JsonValue.Create(langCode);

            if (SaveRootNode(root))
            {
                return Task.FromResult((true, $"Updated transcription language to '{langCode}'."));
            }

            return Task.FromResult((false, "Failed to save language selection."));
        }
    }
}
