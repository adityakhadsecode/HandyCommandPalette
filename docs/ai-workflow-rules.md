# AI Workflow Rules: Handy Command Palette Extension

## 1. Incremental Phased Delivery
Follow the incremental 10-phase delivery plan outlined in `prompt.md`:

1. **Phase 1: Research & Setup**
   - Verify Handy internals, CLI flags, paths, SQLite schema, settings structure, and Command Palette APIs.
   - Setup project docs and implementation plan.
2. **Phase 2: Core Domain Models & Services**
   - Create models: `HandyStatus`, `TranscriptEntry`, `DictionaryWord`, `HandyModelInfo`, `HandyLanguageInfo`.
   - Implement services: `HandyProcessService`, `HandySettingsService`, `HandyHistoryService`, `ClipboardService`, `HandyService`.
3. **Phase 3: Toggle Recording**
   - Implement `ToggleRecordingCommand` using `HandyProcessService` (`handy.exe --toggle-transcription`).
4. **Phase 4: Last Transcript (Copy & Non-Destructive Paste)**
   - Implement `CopyLastTranscriptCommand` and `PasteLastTranscriptCommand`.
5. **Phase 5: Dictionary Management**
   - Implement `ManageDictionaryPage`, `AddDictionaryWordPage` (Adaptive Card form), and associated commands.
6. **Phase 6: Model & Language Selection**
   - Implement `ModelSelectionPage` and `LanguageSelectionPage`.
7. **Phase 7: Recordings Folder**
   - Implement `OpenRecordingsFolderCommand`.
8. **Phase 8: Search Transcripts**
   - Implement `TranscriptSearchPage` (`DynamicListPage`) with real-time SQLite querying.
9. **Phase 9: Command Provider & Top-Level Integration**
   - Register all 9 commands in `HandyCommandPaletteCommandsProvider`.
   - Add empty states, error handling, and visual polish.
10. **Phase 10: Build, Deployment & Verification**
    - Restore packages, build x64 and ARM64 configurations, fix any compiler warnings.
    - Test packaged deployment and document manual test steps.

---

## 2. Invariants During AI Code Generation
- **No Stubs/Placeholders**: No `TODO`, `throw new NotImplementedException()`, or fake hard-coded data where actual Handy data can be read.
- **Verification After Each Phase**: Compile the solution after meaningful changes to catch any breaking API issues early.
- **Update Documentation**: Keep `docs/progress-tracker.md` updated as phases complete.

