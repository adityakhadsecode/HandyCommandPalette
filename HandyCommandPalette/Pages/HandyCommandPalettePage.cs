// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using HandyCommandPalette.Commands;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace HandyCommandPalette.Pages;

internal sealed partial class HandyCommandPalettePage : ListPage
{
    public HandyCommandPalettePage()
    {
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        Title = "Handy";
        Name = "Open";
    }

    public override IListItem[] GetItems()
    {
        return [
            new ListItem(new ToggleRecordingCommand())
            {
                Title = "Toggle Recording",
                Subtitle = "Toggle Handy transcription recording on/off",
                Icon = new IconInfo("\uE720"),
            },
            new ListItem(new CopyLastTranscriptCommand())
            {
                Title = "Copy Last Transcript",
                Subtitle = "Copy the most recent Handy transcription to clipboard",
                Icon = new IconInfo("\uE8C8"),
            },
            new ListItem(new PasteLastTranscriptCommand())
            {
                Title = "Paste Last Transcript",
                Subtitle = "Paste the most recent Handy transcription into the active app",
                Icon = new IconInfo("\uE77F"),
            },
            new ListItem(new AddDictionaryWordPage())
            {
                Title = "Add Dictionary Word",
                Subtitle = "Quickly add a word to Handy's custom dictionary",
                Icon = new IconInfo("\uE710"),
            },
            new ListItem(new ManageDictionaryPage())
            {
                Title = "Manage Dictionary",
                Subtitle = "Add and remove custom words",
                Icon = new IconInfo("\uE82D"),
            },
            new ListItem(new ModelSelectionPage())
            {
                Title = "Select Model",
                Subtitle = "Switch the active transcription model",
                Icon = new IconInfo("\uE945"),
            },
            new ListItem(new LanguageSelectionPage())
            {
                Title = "Select Language",
                Subtitle = "Set the transcription language for the active model",
                Icon = new IconInfo("\uE774"),
            },
            new ListItem(new OpenRecordingsFolderCommand())
            {
                Title = "Open Recordings Folder",
                Subtitle = "Open the recordings directory",
                Icon = new IconInfo("\uE8B7"),
            },
            new ListItem(new TranscriptSearchPage())
            {
                Title = "Search Transcripts",
                Subtitle = "Browse and copy transcription history",
                Icon = new IconInfo("\uE721"),
            },
        ];
    }
}

