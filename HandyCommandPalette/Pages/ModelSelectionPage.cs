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

internal sealed partial class ModelSelectionPage : ListPage
{
    public ModelSelectionPage()
    {
        Title = "Select Model";
        Icon = new IconInfo("\uE945");
        Name = "Open";
    }

    public override IListItem[] GetItems()
    {
        var models = HandyService.Instance.GetModelsAsync().GetAwaiter().GetResult();
        var items = new List<IListItem>();

        foreach (var model in models)
        {
            var subtitle = model.IsActive
                ? $"✓ Active • {model.Description}"
                : $"{model.Description}{(model.IsDownloaded ? " • Downloaded" : string.Empty)}";

            var icon = model.IsActive
                ? new IconInfo("\uE73E")
                : new IconInfo("\uE945");

            items.Add(new ListItem(new SelectModelCommand(model, this))
            {
                Title = model.Name,
                Subtitle = subtitle,
                Icon = icon,
            });
        }

        return items.ToArray();
    }
    public void Refresh() => RaiseItemsChanged();
}

internal sealed partial class SelectModelCommand : InvokableCommand
{
    private readonly HandyModelInfo _model;
    private readonly ModelSelectionPage _page;

    public SelectModelCommand(HandyModelInfo model, ModelSelectionPage page)
    {
        _model = model;
        _page = page;
        Name = $"Select {_model.Name}";
        Icon = new IconInfo("\uE73E");
    }

    public override CommandResult Invoke()
    {
        Task.Run(async () =>
        {
            var result = await HandyService.Instance.SelectModelAsync(_model.Id);
            new ToastStatusMessage(result.Message).Show();
            _page.Refresh();
        });

        return CommandResult.KeepOpen();
    }
}
