// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.IO;

namespace HandyCommandPalette.Services;

public static class HandyPathResolver
{
    private static string? _cachedExePath;
    private static string? _cachedAppDataDir;

    public static string? FindHandyExecutable()
    {
        if (!string.IsNullOrEmpty(_cachedExePath) && File.Exists(_cachedExePath))
        {
            return _cachedExePath;
        }

        try
        {
            var running = Process.GetProcessesByName("handy");
            foreach (var proc in running)
            {
                try
                {
                    var path = proc.MainModule?.FileName;
                    if (!string.IsNullOrEmpty(path) && File.Exists(path))
                    {
                        _cachedExePath = path;
                        return path;
                    }
                }
                catch
                {
                    // Access denied or 32/64 bit mismatch, continue
                }
            }
        }
        catch
        {
            // Process enumeration error
        }

        // Common Windows installation paths
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

        string[] candidatePaths =
        [
            Path.Combine(localAppData, "Programs", "Handy", "handy.exe"),
            Path.Combine(programFiles, "Handy", "handy.exe"),
            Path.Combine("E:\\Manual\\Handy", "handy.exe"), // User local location
            Path.Combine(localAppData, "Handy", "handy.exe"),
        ];

        foreach (var candidate in candidatePaths)
        {
            if (File.Exists(candidate))
            {
                _cachedExePath = candidate;
                return candidate;
            }
        }

        // Check system PATH
        var pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (!string.IsNullOrEmpty(pathEnv))
        {
            var paths = pathEnv.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);
            foreach (var dir in paths)
            {
                var candidate = Path.Combine(dir.Trim(), "handy.exe");
                if (File.Exists(candidate))
                {
                    _cachedExePath = candidate;
                    return candidate;
                }
            }
        }

        return null;
    }

    public static string GetAppDataDirectory()
    {
        if (!string.IsNullOrEmpty(_cachedAppDataDir) && Directory.Exists(_cachedAppDataDir))
        {
            return _cachedAppDataDir;
        }

        // Check if portable mode applies to the detected executable
        var exePath = FindHandyExecutable();
        if (!string.IsNullOrEmpty(exePath))
        {
            var exeDir = Path.GetDirectoryName(exePath);
            if (!string.IsNullOrEmpty(exeDir))
            {
                var portableMarker = Path.Combine(exeDir, "portable");
                var portableData = Path.Combine(exeDir, "Data");
                if (File.Exists(portableMarker) && Directory.Exists(portableData))
                {
                    _cachedAppDataDir = portableData;
                    return portableData;
                }
            }
        }

        // Standard Windows Tauri AppData location: %APPDATA%\com.pais.handy
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var standardDir = Path.Combine(appData, "com.pais.handy");
        _cachedAppDataDir = standardDir;
        return standardDir;
    }

    public static string GetDatabasePath() => Path.Combine(GetAppDataDirectory(), "history.db");

    public static string GetSettingsPath() => Path.Combine(GetAppDataDirectory(), "settings_store.json");

    public static string GetRecordingsDirectory()
    {
        var dir = Path.Combine(GetAppDataDirectory(), "recordings");
        if (!Directory.Exists(dir))
        {
            try
            {
                Directory.CreateDirectory(dir);
            }
            catch
            {
                // Fallback if unable to create
            }
        }

        return dir;
    }

    public static string GetModelsDirectory() => Path.Combine(GetAppDataDirectory(), "models");
}
