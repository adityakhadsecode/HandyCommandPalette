// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using HandyCommandPalette.Models;
using HandyCommandPalette.Services;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace HandyCommandPalette.Pages;

internal sealed partial class TranscriptSearchPage : DynamicListPage
{
    private IListItem[] _items = [];
    private bool _initialized;

    public TranscriptSearchPage()
    {
        Title = "Search Transcripts";
        Icon = new IconInfo("\uE721");
        Name = "Search";
    }

    public override void UpdateSearchText(string oldSearch, string newSearch)
    {
        Search(newSearch);
    }

    public override IListItem[] GetItems()
    {
        if (!_initialized)
        {
            _initialized = true;
            Search(string.Empty);
        }

        return _items;
    }

    private void Search(string query)
    {
        var transcripts = HandyService.Instance.SearchTranscriptsAsync(query, 100)
            .GetAwaiter().GetResult();

        var list = new List<IListItem>();

        if (transcripts.Length == 0)
        {
            list.Add(new ListItem(new NoOpCommand())
            {
                Title = string.IsNullOrWhiteSpace(query) ? "No transcripts recorded yet" : "No matching transcripts found",
                Subtitle = "Transcripts are automatically saved when you transcribe speech with Handy",
                Icon = new IconInfo("\uE946"),
            });
        }
        else
        {
            foreach (var t in transcripts)
            {
                var text = t.BestText;
                var previewTitle = text.Length > 75 ? string.Concat(text.AsSpan(0, 75), "...") : text;
                if (string.IsNullOrWhiteSpace(previewTitle))
                {
                    previewTitle = t.Title;
                }

                var subtitle = $"{t.FormattedTime} • {(t.Saved ? "★ Saved" : "Recording")}";

                var details = new Details
                {
                    Title = t.Title,
                    Body = $"### Transcript\n\n{text}\n\n---\n*Recorded on {t.FormattedTime}*",
                };

                var audioPath = Path.Combine(HandyPathResolver.GetRecordingsDirectory(), t.FileName);

                var moreCommands = new List<CommandContextItem>
                {
                    new(new AnonymousCommand(() =>
                    {
                        Task.Run(async () =>
                        {
                            await Task.Delay(100);
                            await ClipboardService.PasteTextNonDestructiveAsync(text);
                        });
                    }))
                    {
                        Title = "Paste into Active App",
                    },
                };

                if (File.Exists(audioPath))
                {
                    moreCommands.Add(new CommandContextItem(new AnonymousCommand(() =>
                    {
                        try
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = "explorer.exe",
                                Arguments = $"/select,\"{audioPath}\"",
                                UseShellExecute = true,
                            });
                        }
                        catch
                        {
                            // Fallback
                        }
                    }))
                    {
                        Title = "Locate Audio File",
                    });
                }

                list.Add(new ListItem(new CopyTextCommand(text))
                {
                    Title = previewTitle,
                    Subtitle = subtitle,
                    Icon = new IconInfo("\uE8A5"),
                    Details = details,
                    MoreCommands = moreCommands.ToArray(),
                });
            }
        }

        _items = list.ToArray();
        RaiseItemsChanged();
    }
}
