# Architecture: Handy Command Palette Extension

## 1. System Structure

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
                  |  |       Pages & UI Components      |  |
                  |  | (ListPage, DynamicList, Forms)   |  |
                  |  +----------------------------------+  |
                  |  |         Service Layer            |  |
                  |  | - HandyService (Façade)          |  |
                  |  | - HandyProcessService (CLI/Proc) |  |
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

---

## 2. Boundaries & Invariants

### 2.1 Process Separation
- `HandyCommandPalette.exe` runs out-of-process from PowerToys, initiated by COM activation (`-RegisterProcessAsComServer`).
- It does not embed speech recognition engines (Whisper, Parakeet, VAD, etc.).
- It interacts with Handy without duplicating or modifying Handy's internal audio loops.

### 2.2 Handy Discovery & Single-Instance Remote Control
- **Process Discovery**: Uses Win32 / `System.Diagnostics.Process` to check if `handy.exe` is running.
  - Path detection: running process executable path -> registry / `%LOCALAPPDATA%\Programs\Handy\handy.exe` -> PATH -> fallback config.
- **Toggle Recording**: Executes `handy.exe --toggle-transcription`. When Handy is already running, Tauri's single-instance plugin sends the flag to the running instance and the spawned process immediately exits.
- **Graceful Handling**: If Handy is not running, Toggle Recording prompts or launches Handy in background (`--start-hidden`).

### 2.3 Storage Model
- **AppData Directory**:
  - Checks if portable marker `<HandyDir>\portable` exists. If so, root is `<HandyDir>\Data\`.
  - Standard mode: `%APPDATA%\com.pais.handy\`.
- **Settings Store (`settings_store.json`)**:
  - Contains top-level `"settings"` object.
  - Manages `custom_words` (`string[]`), `selected_model` (`string`), `selected_language` (`string`).
  - Writes are atomic: write to a temporary file in the same directory and replace, preventing corruption while Handy is reading.
- **History Store (`history.db`)**:
  - SQLite database accessed via `Microsoft.Data.Sqlite` in read-only / shared read-write mode (`Mode=ReadOnly` or `PRAGMA busy_timeout=3000`).
  - Queries `transcription_history` table: `id`, `file_name`, `timestamp`, `saved`, `title`, `transcription_text`, `post_processed_text`.
  - Transcripts are fetched with newest first (`ORDER BY timestamp DESC`).
- **Recordings Directory**:
  - Located at `<AppData>\recordings`.
  - Opened directly via ShellExecute/`Process.Start("explorer.exe", path)`.

### 2.4 Clipboard & Active Window Non-Destructive Paste
- When `Paste Last Transcript` is invoked:
  1. Fetch last non-empty transcript from `history.db`.
  2. Capture current foreground window handle (`GetForegroundWindow`).
  3. Backup existing clipboard content (format text/Unicode).
  4. Write transcript text to clipboard.
  5. Bring target window to foreground if needed and dispatch synthetic paste keystrokes (`Ctrl+V` via `SendInput`).
  6. Wait a brief delay (e.g. 100-200ms) for the target app to process the paste message.
  7. Restore original clipboard content.

---

## 3. Resilience & Error Handling
- All I/O (file, SQLite, process launch) is wrapped in try/catch blocks within the service layer.
- Operations return typed `Result<T>` or safe fallbacks (empty arrays, informative error messages) so exceptions never crash the COM server or freeze Command Palette.
- Informative `StatusMessage` or `ToastStatusMessage` displays feedback to the user on completion or failure.

