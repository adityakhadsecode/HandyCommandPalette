// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Threading.Tasks;
using HandyCommandPalette.Models;
using HandyCommandPalette.Services;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace HandyCommandPalette.Pages;

internal sealed partial class LanguageSelectionPage : ListPage
{
    public LanguageSelectionPage()
    {
        Title = "Select Language";
        Icon = new IconInfo("\uE774");
        Name = "Open";
    }

    public override IListItem[] GetItems()
    {
        var languages = HandyService.Instance.GetLanguagesAsync().GetAwaiter().GetResult();
        var items = new List<IListItem>();

        foreach (var lang in languages)
        {
            var subtitle = lang.IsActive
                ? $"✓ Active • {lang.DisplaySubtitle}"
                : lang.DisplaySubtitle;

            var icon = lang.IsActive
                ? new IconInfo("\uE73E")
                : new IconInfo("\uE774");

            items.Add(new ListItem(new SelectLanguageCommand(lang, this))
            {
                Title = lang.DisplayTitle,
                Subtitle = subtitle,
                Icon = icon,
            });
        }

        return items.ToArray();
    }
    public void Refresh() => RaiseItemsChanged();
}

internal sealed partial class SelectLanguageCommand : InvokableCommand
{
    private readonly HandyLanguageInfo _language;
    private readonly LanguageSelectionPage _page;

    public SelectLanguageCommand(HandyLanguageInfo language, LanguageSelectionPage page)
    {
        _language = language;
        _page = page;
        Name = $"Select {_language.EnglishName}";
        Icon = new IconInfo("\uE73E");
    }

    public override CommandResult Invoke()
    {
        Task.Run(async () =>
        {
            var result = await HandyService.Instance.SelectLanguageAsync(_language.Code);
            new ToastStatusMessage(result.Message).Show();
            _page.Refresh();
        });

        return CommandResult.KeepOpen();
    }
}
