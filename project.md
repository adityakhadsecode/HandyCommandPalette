# Handy Command Palette Extension (`HandyCommandPalette`)

A native Microsoft PowerToys Command Palette extension integrating with [Handy](https://github.com/cjpais/Handy), the free, open-source, and offline speech-to-text application for Windows.

---

## 1. Overview & Goals
The goal of this extension is to provide a fast, accessible, and native Windows PowerToys Command Palette interface to control Handy and manage speech-to-text workflows without leaving the keyboard:
- Trigger audio transcription recordings.
- Copy or non-destructively paste recent transcripts into any active application.
- Search and browse local transcription history.
- Add and manage custom dictionary terms.
- Switch speech recognition models and target languages.
- Quick-jump to local recording files.

---

## 2. Integration Architecture

```
                  +----------------------------------------+
                  |       PowerToys Command Palette        |
                  +----------------------------------------+
                                      |
                      (Out-of-Process COM Server via WinRT)
                                      v
                  +----------------------------------------+
                  |          HandyCommandPalette           |
                  |                                        |
                  |  +----------------------------------+  |
                  |  |         CommandProvider          |  |
                  |  |  (Registers 9 Top-Level Commands)|  |
                  |  +----------------------------------+  |
                  |  |       Pages & UI Primitives      |  |
                  |  | (ListPage, DynamicList, Forms)   |  |
                  |  +----------------------------------+  |
                  |  |         Service Layer            |  |
                  |  | - HandyService (Orchestrator)    |  |
                  |  | - HandyProcessService (CLI)      |  |
                  |  | - HandySettingsService (JSON)    |  |
                  |  | - HandyHistoryService (SQLite)   |  |
                  |  | - ClipboardService (Win32 Paste) |  |
                  |  +----------------------------------+  |
                  +----------------------------------------+
                         /           |           \
     (CLI Single-Instance)           |            \ (Direct SQLite/FS)
                       v             v             v
       +--------------------+  +-----------+  +----------------------+
       | Running Handy.exe  |  | Settings  |  | SQLite (history.db)  |
       | (--toggle-...)     |  | (.json)   |  | Audio (.wav) files   |
       +--------------------+  +-----------+  +----------------------+
```

### 2.1 Native Windows Integration Points
- **Single-Instance Control**: Handy uses Tauri's single-instance plugin. Calling `handy.exe --toggle-transcription` forwards the toggle action directly to the running Handy process via IPC and terminates immediately. This avoids duplicate processes and requires no UI scripting.
- **Process Discovery**: `HandyPathResolver` dynamically checks running processes (`Process.GetProcessesByName("handy")`), user app installations (`%LOCALAPPDATA%\Programs\Handy\handy.exe`), registry, and `PATH`.
- **Database Access**: Transcripts and history are stored in SQLite (`%APPDATA%\com.pais.handy\history.db`). The extension queries `transcription_history` using `Microsoft.Data.Sqlite` in read-only mode (`Mode=ReadOnly`) with `PRAGMA busy_timeout=3000` to prevent database contention.
- **Settings Store**: Located at `%APPDATA%\com.pais.handy\settings_store.json`. The extension reads the `"settings"` object and performs atomic writes via `.tmp` file replacement to prevent corrupting settings while Handy is running.
- **Non-Destructive Paste**: Backs up current clipboard text, sets the transcript, simulates `Ctrl+V` via Win32 `SendInput`, pauses briefly for the active application to consume the keystroke, and restores the original clipboard.

### 2.2 Compatibility
- **Developed Against**: Handy v0.9.x+ (Tauri v2 architecture).
- **Target Framework**: .NET 10 (`net10.0-windows10.0.26100.0`).
- **Target Platforms**: `x64` and `ARM64`.
- **AOT & Trimming**: Fully compliant with `<IsAotCompatible>true</IsAotCompatible>` and trimming analyzers.

---

## 3. Implemented Commands

| # | Command | UI Component | Description |
|---|---|---|---|
| 1 | **Toggle Recording** | `InvokableCommand` | Toggle Handy transcription recording on/off via `handy.exe --toggle-transcription`. |
| 2 | **Copy Last Transcript** | `InvokableCommand` | Copies the most recent completed transcript from `history.db` to the Windows clipboard. |
| 3 | **Paste Last Transcript** | `InvokableCommand` | Pastes the most recent transcript into the foreground application while preserving previous clipboard contents. |
| 4 | **Add Dictionary Word** | `ContentPage` / `FormContent` | Adaptive Card form to quickly register a new word or phrase in Handy's `custom_words` dictionary. |
| 5 | **Manage Dictionary** | `ListPage` | Searchable list of custom words with one-click copy and context actions to delete entries. |
| 6 | **Select Model** | `ListPage` | Displays Handy models (Whisper, Parakeet, Moonshine, SenseVoice, Canary), indicating download and active status. |
| 7 | **Select Language** | `ListPage` | Searchable list of supported languages (including Auto-detect `auto`), switching the active Handy transcription language. |
| 8 | **Open Recordings Folder** | `InvokableCommand` | Opens Handy's recordings directory (`%APPDATA%\com.pais.handy\recordings`) in Windows Explorer. |
| 9 | **Search Transcripts** | `DynamicListPage` | Interactive real-time search over historical transcripts with detailed markdown preview, copy actions, and audio file location. |

---

## 4. Build & Setup Instructions

### 4.1 Prerequisites
- Windows 10 (Build 19041+) or Windows 11
- .NET 10 SDK (`10.0.401+`)
- PowerToys with Command Palette enabled
- Handy installed and configured

### 4.2 Building the Extension
From the repository root:

```powershell
# Restore dependencies
dotnet restore

# Build x64 (Debug)
dotnet build -p:Platform=x64 -c Debug

# Build ARM64 (Debug)
dotnet build -p:Platform=ARM64 -c Debug

# Build Release
dotnet build -p:Platform=x64 -c Release
```

### 4.3 Deploying to PowerToys Command Palette
1. Open `HandyCommandPalette.sln` in Visual Studio 2022/2026.
2. Select **Debug** or **Release** configuration with platform **x64**.
3. Choose the **(Package)** launch profile.
4. Right-click `HandyCommandPalette` project -> **Deploy** (this registers the MSIX package on the local system).
5. Open PowerToys Command Palette (`Win+Space` or configured hotkey).
6. Type `Reload` and execute **Reload Command Palette extensions**.
7. All 9 Handy commands are now active and discoverable.

---

## 5. Troubleshooting & Error Handling

- **"Handy executable could not be found"**:
  - Make sure Handy is installed. If Handy is located in a custom directory, start Handy once so the extension can automatically detect its running process path.
- **"No previous Handy transcript found"**:
  - The SQLite database does not contain any completed transcriptions yet. Press **Toggle Recording**, speak a phrase, and stop recording to generate your first transcript.
- **Handy Not Running on Toggle Recording**:
  - If Handy is not running when Toggle Recording is invoked, the extension automatically attempts to launch Handy in the background (`--start-hidden`).
- **Permissions / Window Focus during Paste**:
  - If pasting fails into an elevated window (running as Administrator), run PowerToys as Administrator to allow Windows `SendInput` messages across integrity levels.

---

## 6. Known Limitations & Upstream Handy Notes
- **Recording State Push**: Handy does not currently emit an external Windows Named Pipe or COM event when recording state changes; therefore, command item subtitles reflect state at page load rather than continuous real-time background polling.
- **Offline Guarantee**: The extension respects Handy's strictly offline model and never initiates network traffic.

