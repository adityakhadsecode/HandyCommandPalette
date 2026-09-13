# Project Overview: Handy Command Palette Extension

## 1. Product Definition
`HandyCommandPalette` is a native Windows PowerToys Command Palette extension that integrates seamlessly with [Handy](https://github.com/cjpais/Handy), a free, open-source, and offline speech-to-text application for Windows.

The extension acts as a frontend and control layer for Handy, bringing quick voice-to-text actions, transcript history search, dictionary management, and audio recording controls into the user's Command Palette workflow.

---

## 2. Core Invariants & Scope Boundaries
- **No Speech Engine Duplication**: The extension does not embed Whisper, Parakeet, or any speech-to-text models.
- **No Audio Recording Duplication**: Microphone capture, Voice Activity Detection (VAD), and audio file writing remain strictly inside Handy.
- **Offline & Private**: The extension performs all operations locally. No cloud telemetry, no analytics, no external API calls.
- **Single Instance Control**: The extension interacts with the already running Handy process via Handy's CLI single-instance trigger (`handy --toggle-transcription`), local configuration (`settings_store.json`), and SQLite storage (`history.db`). It does not spawn duplicate Handy instances.
- **PowerToys CmdPal Architecture**: Adheres strictly to Microsoft's out-of-process COM Server extension architecture (`Microsoft.CommandPalette.Extensions`, `Shmuelie.WinRTServer`, MSIX packaging, AOT/trimming compatibility).

---

## 3. Features & Command Specifications

The extension exposes 9 primary commands:

| # | Command Name | Type | Description |
|---|---|---|---|
| 1 | **Toggle Recording** | `InvokableCommand` | Toggles Handy recording on/off via Handy single-instance CLI (`handy --toggle-transcription`). Fast and asynchronous. |
| 2 | **Copy Last Transcript** | `InvokableCommand` | Fetches the latest completed transcription from `history.db` and copies plain text to the Windows clipboard. |
| 3 | **Paste Last Transcript** | `InvokableCommand` | Non-destructive paste: saves existing clipboard, copies last transcript, synthesizes paste (Ctrl+V) to target window, and restores clipboard. |
| 4 | **Add Dictionary Word** | `ContentPage` / `FormContent` | Adaptive Card form allowing the user to input a custom word/phrase and persist it into Handy's `custom_words` setting. |
| 5 | **Manage Dictionary** | `ListPage` | Searchable list of custom words stored in Handy, with actions to delete words or open the Add Word form. |
| 6 | **Select Model** | `ListPage` | Displays available/downloaded models in Handy's catalog, indicates the currently active model, and allows switching models. |
| 7 | **Select Language** | `ListPage` | Searchable list of supported languages (including Auto-detect `auto`), indicates current selection, and updates Handy's `selected_language`. |
| 8 | **Open Recordings Folder** | `InvokableCommand` | Opens Handy's recordings directory (`%APPDATA%\com.pais.handy\recordings` or portable equivalent) in Windows Explorer. |
| 9 | **Search Transcripts** | `DynamicListPage` | Searchable history of transcripts from `history.db`. Displays timestamp, title, and text with context actions (Copy, Paste, Open Audio). |

---

## 4. Integration Target Specs
- Developed against Handy v0.9.x+ (Tauri v2 architecture).
- AppData Root:
  - Standard mode: `%APPDATA%\com.pais.handy\`
  - Portable mode: `<HandyDir>\Data\`
- Database: `history.db` (SQLite 3, table `transcription_history`)
- Settings: `settings_store.json` (JSON with top-level `settings` object)
- CLI Flags: `handy.exe --toggle-transcription`, `--toggle-post-process`, `--cancel`

