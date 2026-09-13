// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.IO;
using HandyCommandPalette.Services;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace HandyCommandPalette.Commands;

internal sealed partial class OpenRecordingsFolderCommand : InvokableCommand
{
    public OpenRecordingsFolderCommand()
    {
        Name = "Open Recordings Folder";
        Icon = new IconInfo("\uE8B7");
    }

    public override CommandResult Invoke()
    {
        try
        {
            var dir = HandyService.Instance.GetRecordingsDirectory();
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"\"{dir}\"",
                UseShellExecute = true,
            });

            return CommandResult.Dismiss();
        }
        catch (Exception ex)
        {
            new ToastStatusMessage($"Failed to open recordings folder: {ex.Message}").Show();
            return CommandResult.Dismiss();
        }
    }
}

