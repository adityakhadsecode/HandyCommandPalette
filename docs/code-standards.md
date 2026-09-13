# Code Standards: Handy Command Palette Extension

## 1. Language & Framework Standards
- Target: **C# 13 / .NET 10** (`net10.0-windows10.0.26100.0`).
- Target Platforms: **x64** and **ARM64**.
- Nullable Reference Types: **Enabled** (`<Nullable>enable</Nullable>`). No unhandled null dereferences.
- Trimming & AOT Compatibility: Maintain `<IsAotCompatible>true</IsAotCompatible>` and avoid dynamic code generation or reflection patterns that break trimming.

---

## 2. Architecture & Separation of Concerns
- **Thin Commands & Pages**: Commands and Pages handle UI representation, input routing, and user feedback only.
- **Service Layer**: All business logic, file I/O, SQLite operations, process execution, and Win32 interop live in dedicated service classes under `HandyCommandPalette/Services/`.
- **Façade Pattern**: `IHandyService` / `HandyService` orchestrates the sub-services:
  - `IHandyProcessService`: Process discovery, launching, and CLI dispatch.
  - `IHandySettingsService`: Safe JSON reading and atomic writing.
  - `IHandyHistoryService`: SQLite reading, pagination, and full-text search.
  - `IClipboardService`: Windows clipboard read/write and non-destructive synthetic paste.

---

## 3. Concurrency & Performance
- **Asynchronous Execution**: All I/O operations (SQLite queries, settings file reads/writes, process invocations) must use `async` / `await` and `Task`.
- **UI Responsiveness**: Never block `GetItems()` or UI thread with heavy synchronous disk or network reads.
- **SQLite Concurrency**: Use `PRAGMA busy_timeout=3000` and read-only connection strings when reading SQLite to prevent lock contention with the running Handy process.
- **Atomic File Writes**: Write new settings to `<file>.tmp` and call `File.Move(..., overwrite: true)` or `File.Replace` to avoid corrupting `settings_store.json`.

---

## 4. Error Handling & Logging
- Wrap all boundary calls (process execution, file access, SQLite access, clipboard APIs) in comprehensive try/catch blocks.
- Never let an unhandled exception escape into the COM server runtime to crash Command Palette.
- Use `System.Diagnostics.Debug.WriteLine` for diagnostic logging.
- Return user-friendly error messages through `ToastStatusMessage`, `StatusMessage`, or UI item subtitles.

---

## 5. Coding Conventions
- File-scoped namespaces (`namespace HandyCommandPalette;`).
- PascalCase for classes, records, interfaces, methods, and public properties.
- Prefix interfaces with `I` (e.g., `IHandyService`).
- Private fields prefixed with an underscore (`_handyService`).
- Meaningful variable and parameter names; avoid single-letter variables except in trivial lambdas.
- Pattern matching (`switch` expressions) and collection expressions (`[...]`) preferred for conciseness.

