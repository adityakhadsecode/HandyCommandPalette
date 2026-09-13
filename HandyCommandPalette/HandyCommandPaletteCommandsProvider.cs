// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using HandyCommandPalette.Commands;
using HandyCommandPalette.Pages;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace HandyCommandPalette;

public partial class HandyCommandPaletteCommandsProvider : CommandProvider
{
    private readonly ICommandItem[] _commands;

    public HandyCommandPaletteCommandsProvider()
    {
        DisplayName = "Handy";
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        _commands = [
            new CommandItem(new HandyCommandPalettePage())
            {
                Title = "Handy",
                Subtitle = "Open Handy Command Palette dashboard",
            },
            new CommandItem(new ToggleRecordingCommand())
            {
                Title = "Toggle Recording",
                Subtitle = "Toggle Handy transcription recording on/off",
            },
            new CommandItem(new CopyLastTranscriptCommand())
            {
                Title = "Copy Last Transcript",
                Subtitle = "Copy the most recent Handy transcription to clipboard",
            },
            new CommandItem(new PasteLastTranscriptCommand())
            {
                Title = "Paste Last Transcript",
                Subtitle = "Paste the most recent Handy transcription into the active app",
            },
            new CommandItem(new AddDictionaryWordPage())
            {
                Title = "Add Dictionary Word",
                Subtitle = "Quickly add a word to Handy's custom dictionary",
            },
            new CommandItem(new ManageDictionaryPage())
            {
                Title = "Manage Dictionary",
                Subtitle = "Add and remove custom words",
            },
            new CommandItem(new ModelSelectionPage())
            {
                Title = "Select Model",
                Subtitle = "Switch the active transcription model",
            },
            new CommandItem(new LanguageSelectionPage())
            {
                Title = "Select Language",
                Subtitle = "Set the transcription language for the active model",
            },
            new CommandItem(new OpenRecordingsFolderCommand())
            {
                Title = "Open Recordings Folder",
                Subtitle = "Open the Handy recordings directory",
            },
            new CommandItem(new TranscriptSearchPage())
            {
                Title = "Search Transcripts",
                Subtitle = "Browse and copy transcription history",
            },
        ];
    }

    public override ICommandItem[] TopLevelCommands()
    {
        return _commands;
    }
}

