# UI Documentation: Handy Command Palette Extension

## 1. Design System & Theme Alignment
The extension adheres to Microsoft PowerToys Command Palette styling, following Fluent Design principles, system theme integration (Dark/Light mode), and standard Command Palette page and item conventions.

---

## 2. Page & Component Conventions

### 2.1 Top-Level Commands Provider
- Registered in `HandyCommandPaletteCommandsProvider.cs`.
- Uses `Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png")`.
- Exposes 9 top-level commands as `ICommandItem[]`.

### 2.2 Icons
- Fluent System Icons (Unicode Segoe Fluent glyphs) are used for all command items:
  - Toggle Recording: `\uE720` (Microphone) / `\uE71A` (Stop/Record)
  - Copy Last Transcript: `\uE8C8` (Copy)
  - Paste Last Transcript: `\uE77F` (Paste)
  - Add Dictionary Word: `\uE710` (Add)
  - Manage Dictionary: `\uE82D` (Library / Word list)
  - Select Model: `\uE945` (Chip / AI model)
  - Select Language: `\uE774` (Globe / Language)
  - Open Recordings Folder: `\uE8B7` (Folder)
  - Search Transcripts: `\uE721` (Search)

### 2.3 Page Implementations
1. **ManageDictionaryPage (`ListPage`)**:
   - Lists custom words from `settings_store.json`.
   - Searchable by default.
   - Primary action on item: Copy word / Edit word.
   - Context action (`MoreCommands`): Remove word (with trash icon `\uE74D`).
   - Top action / empty state: "Add new word..." pointing to `AddDictionaryWordPage`.

2. **AddDictionaryWordPage (`ContentPage` with `FormContent`)**:
   - Adaptive Card with text input (`Word`), placeholder "Enter word or acronym...", and "Add Word" submit button.
   - On submit, trims input, validates non-empty, checks duplicates, saves to `settings_store.json`, and navigates back or shows toast.

3. **ModelSelectionPage (`ListPage`)**:
   - Lists models known to Handy.
   - Indicates active model with a checkmark badge/subtitle `(Active)`.
   - Selecting a model invokes `SelectModelCommand`, updates `selected_model` in settings, and refreshes the page list.

4. **LanguageSelectionPage (`ListPage`)**:
   - Searchable list of supported languages.
   - Pin "Auto (Automatic detection)" at the top.
   - Shows active language tag / checkmark.
   - Selecting updates `selected_language` in settings.

5. **TranscriptSearchPage (`DynamicListPage`)**:
   - Reactive search on user keystrokes querying SQLite `history.db`.
   - Shows:
     - Title: Transcript text preview (or title).
     - Subtitle: Formatted local date/time + duration/model info.
     - Details pane: Full transcript text in markdown format, copy actions, audio file path.
     - Context actions: Copy, Paste directly to active app, Open audio file.

---

## 3. Empty & Error States
- **Handy Not Running**: When actions require Handy, display clear guidance ("Handy is not running. Launch Handy to enable this feature.").
- **Empty Transcripts**: "No transcriptions found. Press Toggle Recording to transcribe speech."
- **Empty Dictionary**: "No custom words in dictionary. Select 'Add Word' to create one."
- **Toasts**: Non-intrusive `ToastStatusMessage` or `CommandResult.ShowToast(...)` for quick operations like "Transcript copied to clipboard".

