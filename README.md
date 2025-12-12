## F# Digital Dictionary (Avalonia + SQLite)

Desktop GUI dictionary built with F#, Avalonia UI, and SQLite. The app lets you add, update, search, and delete word/definition pairs and persists them in `dictionary.db`.

### Project structure

- `Program.fs` — bootstraps Avalonia and starts the desktop lifetime.
- `App.fs` / `App.axaml` — application shell and theme registration.
- `MainWindow.axaml` — layout for the main window (inputs, list, buttons).
- `MainWindow.fs` — UI logic and event wiring; orchestrates database calls and list refresh.
- `Dictionary.fs` — pure domain helpers (entry type, normalization, JSON helpers).
- `Database.fs` — SQLite access layer (create table, CRUD, search).
- `dictionary.db` — SQLite database file created on first run.

### Runtime flow (happy path)

1. `Program.main` configures Avalonia and opens `MainWindow`.
2. `MainWindow` constructor loads XAML controls, then calls `Database.initialize()` to ensure the `entries` table exists.
3. Existing data is fetched via `Database.getAllEntries()` and bound to the list view.
4. User actions:
   - **Add / Update**: reads text boxes; `Database.addOrUpdate` writes to SQLite, then reloads list.
   - **Delete Selected**: deletes the highlighted entry; reloads list.
   - **Search**: uses the selected mode:
     - Exact word (`searchExact`)
     - Word contains (`searchByWordContains`)
     - Word or definition contains (`searchAnyFieldContains`)
   - **Clear**: resets search and shows all entries.
   - **Load/Save**: re-fetches from SQLite (SQLite persists automatically).
   - **Exit**: closes the window.
5. Feedback is shown in the window title for quick, dependency-free status/error messages.

### Key behaviors and constraints

- Words are compared case-insensitively (normalization in `Dictionary.fs`; SQL uses `lower(...)`).
- Database schema: `entries(word TEXT PRIMARY KEY, definition TEXT NOT NULL)`.
- If SQLite is unavailable, the title shows an error and the in-memory list is emptied.

### Requirements

- .NET 8 SDK
- SQLite (bundled via `Microsoft.Data.Sqlite`; no external server needed)

### Build & run

```bash
cd /home/qassem/Desktop/pl3
dotnet run
```

### Extending the app

- Add fields (e.g., part of speech) by updating `Entry`, the XAML bindings, and the SQL schema.
- Replace title-based status messages with dialogs or notifications for richer UX.
- Hook `Dictionary.fs` JSON helpers if you want export/import beyond SQLite.


