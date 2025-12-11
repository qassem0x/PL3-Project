## F# Digital Dictionary

**Objective**: A simple functional digital dictionary using an immutable `Map` with full CRUD, search, and JSON persistence.

### Features

- **Add / update / delete words**
- **Case-insensitive search**
  - Exact word lookup
  - Partial match on word
  - Partial match on word or definition
- **Save / load as JSON** (`dictionary.json` in the working directory)

### Prerequisites

- .NET 8 SDK (or compatible) installed.

### How to Run

```bash
cd /home/qassem/Desktop/pl3
dotnet run
```

The app will:

- Load existing data from `dictionary.json` on startup (if present).
- Let you manage entries via a simple text menu.
- Save back to `dictionary.json` when you choose **Save** or exit.


