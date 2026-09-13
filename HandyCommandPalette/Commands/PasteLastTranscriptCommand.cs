// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using HandyCommandPalette.Services;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace HandyCommandPalette.Commands;

internal sealed partial class PasteLastTranscriptCommand : InvokableCommand
{
    public PasteLastTranscriptCommand()
    {
        Name = "Paste Last Transcript";
        Icon = new IconInfo("\uE77F");
    }

    public override CommandResult Invoke()
    {
        // Dismiss Command Palette first so target application window recovers focus
        Task.Run(async () =>
        {
            // Give Command Palette window time to close and yield focus
            await Task.Delay(100);
            var result = await HandyService.Instance.PasteLastTranscriptAsync();
            if (!result.Success)
            {
                new ToastStatusMessage(result.Message).Show();
            }
        });

        return CommandResult.Dismiss();
    }
}

