// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using HandyCommandPalette.Services;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace HandyCommandPalette.Commands;

internal sealed partial class ToggleRecordingCommand : InvokableCommand
{
    public ToggleRecordingCommand()
    {
        Name = "Toggle Recording";
        Icon = new IconInfo("\uE720");
    }

    public override CommandResult Invoke()
    {
        Task.Run(async () =>
        {
            var result = await HandyService.Instance.ToggleRecordingAsync();
            if (!result.Success)
            {
                new ToastStatusMessage(result.Message).Show();
            }
        });

        return CommandResult.Dismiss();
    }
}

