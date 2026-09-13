// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace HandyCommandPalette.Services;

public sealed class HandyProcessService
{
    public static bool IsHandyRunning()
    {
        try
        {
            var processes = Process.GetProcessesByName("handy");
            return processes.Length > 0;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[HandyProcessService] Error checking process: {ex.Message}");
            return false;
        }
    }

    public static async Task<(bool Success, string Message)> ToggleRecordingAsync()
    {
        var exePath = HandyPathResolver.FindHandyExecutable();
        if (string.IsNullOrEmpty(exePath))
        {
            return (false, "Handy executable could not be found. Please ensure Handy is installed.");
        }

        bool wasRunning = IsHandyRunning();

        try
        {
            // When Handy is running, executing handy.exe --toggle-transcription forwards the
            // toggle message to the active single instance via Tauri single-instance plugin and exits immediately.
            var startInfo = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = wasRunning ? "--toggle-transcription" : "--start-hidden",
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
            };

            using var proc = Process.Start(startInfo);
            if (proc != null)
            {
                if (wasRunning)
                {
                    // Wait briefly for the remote CLI process to communicate and exit
                    await proc.WaitForExitAsync();
                    return (true, "Toggled Handy recording.");
                }
                else
                {
                    return (true, "Handy was not running. Started Handy in background.");
                }
            }

            return (false, "Failed to launch Handy process.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[HandyProcessService] Toggle recording exception: {ex}");
            return (false, $"Failed to communicate with Handy: {ex.Message}");
        }
    }
}
