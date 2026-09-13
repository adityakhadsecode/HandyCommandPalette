// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Text.Json;
using HandyCommandPalette.Services;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace HandyCommandPalette.Pages;

internal sealed partial class AddDictionaryWordPage : ContentPage
{
    public AddDictionaryWordPage()
    {
        Title = "Add Dictionary Word";
        Icon = new IconInfo("\uE710");
        Name = "Add";
    }

    public override IContent[] GetContent() =>
    [
        new AddDictionaryWordForm(),
    ];
}

internal sealed partial class AddDictionaryWordForm : FormContent
{
    public AddDictionaryWordForm()
    {
        TemplateJson = """
        {
          "type": "AdaptiveCard",
          "$schema": "https://adaptivecards.io/schemas/adaptive-card.json",
          "version": "1.5",
          "body": [
            {
              "type": "TextBlock",
              "text": "Add Word to Handy Dictionary",
              "weight": "Bolder",
              "size": "Medium"
            },
            {
              "type": "TextBlock",
              "text": "Add domain-specific vocabulary, technical acronyms, or names to help Handy transcribe accurately.",
              "wrap": true,
              "isSubtle": true
            },
            {
              "type": "Input.Text",
              "id": "word",
              "placeholder": "Enter custom word or phrase...",
              "isRequired": true,
              "errorMessage": "Word is required"
            }
          ],
          "actions": [
            {
              "type": "Action.Submit",
              "title": "Add Word"
            }
          ]
        }
        """;
    }

    public override CommandResult SubmitForm(string payload)
    {
        try
        {
            using var doc = JsonDocument.Parse(payload);
            if (doc.RootElement.TryGetProperty("word", out var wordElem))
            {
                var word = wordElem.GetString();
                if (!string.IsNullOrWhiteSpace(word))
                {
                    var result = HandyService.Instance.AddCustomWordAsync(word.Trim()).GetAwaiter().GetResult();
                    new ToastStatusMessage(result.Message).Show();
                    if (result.Success)
                    {
                        return CommandResult.GoBack();
                    }

                    return CommandResult.KeepOpen();
                }
            }
        }
        catch (Exception ex)
        {
            new ToastStatusMessage($"Error adding word: {ex.Message}").Show();
        }

        return CommandResult.KeepOpen();
    }
}

