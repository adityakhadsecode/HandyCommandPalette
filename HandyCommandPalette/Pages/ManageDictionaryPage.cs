// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Threading.Tasks;
using HandyCommandPalette.Services;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace HandyCommandPalette.Pages;

internal sealed partial class ManageDictionaryPage : ListPage
{
    public ManageDictionaryPage()
    {
        Title = "Manage Dictionary";
        Icon = new IconInfo("\uE82D");
        Name = "Open";
    }

    public override IListItem[] GetItems()
    {
        var words = HandyService.Instance.GetCustomWordsAsync().GetAwaiter().GetResult();
        var items = new List<IListItem>();

        // Always show the quick action to add a new word at the top
        items.Add(new ListItem(new AddDictionaryWordPage())
        {
            Title = "Add new word...",
            Subtitle = "Register a new word or phrase in the custom dictionary",
            Icon = new IconInfo("\uE710"),
        });

        if (words.Length == 0)
        {
            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "No custom words in dictionary",
                Subtitle = "Click 'Add new word...' above to add your first word",
                Icon = new IconInfo("\uE946"),
            });
        }
        else
        {
            foreach (var word in words)
            {
                items.Add(new ListItem(new CopyTextCommand(word))
                {
                    Title = word,
                    Subtitle = "Custom dictionary entry • Click to copy",
                    Icon = new IconInfo("\uE82D"),
                    MoreCommands =
                    [
                        new CommandContextItem(new DeleteDictionaryWordCommand(word, this))
                        {
                            Title = "Remove Word",
                        },
                    ],
                });
            }
        }

        return items.ToArray();
    }
    public void Refresh() => RaiseItemsChanged();
}

internal sealed partial class DeleteDictionaryWordCommand : InvokableCommand
{
    private readonly string _word;
    private readonly ManageDictionaryPage _page;

    public DeleteDictionaryWordCommand(string word, ManageDictionaryPage page)
    {
        _word = word;
        _page = page;
        Name = $"Delete '{word}'";
        Icon = new IconInfo("\uE74D");
    }

    public override CommandResult Invoke()
    {
        Task.Run(async () =>
        {
            var result = await HandyService.Instance.RemoveCustomWordAsync(_word);
            new ToastStatusMessage(result.Message).Show();
            _page.Refresh();
        });

        return CommandResult.KeepOpen();
    }
}
