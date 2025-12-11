module Database

open System
open Microsoft.Data.Sqlite

open Dictionary

let private connectionString = "Data Source=dictionary.db"

let initialize () : Result<unit, string> =
    try
        use conn = new SqliteConnection(connectionString)
        conn.Open()
        use cmd = conn.CreateCommand()
        cmd.CommandText <- """
            CREATE TABLE IF NOT EXISTS entries (
                word TEXT PRIMARY KEY,
                definition TEXT NOT NULL
            );
            """
        cmd.ExecuteNonQuery() |> ignore
        Ok ()
    with ex ->
        Error $"DB init failed: {ex.Message}"

let private readEntry (reader: SqliteDataReader) =
    { Word = reader.GetString(0)
      Definition = reader.GetString(1) }

let getAllEntries () : Result<Entry list, string> =
    try
        use conn = new SqliteConnection(connectionString)
        conn.Open()
        use cmd = conn.CreateCommand()
        cmd.CommandText <- "SELECT word, definition FROM entries ORDER BY lower(word);"
        use reader = cmd.ExecuteReader()
        let results =
            [ while reader.Read() do
                yield readEntry reader ]
        Ok results
    with ex ->
        Error $"DB read failed: {ex.Message}"

let addOrUpdate (word: string) (definition: string) : Result<bool, string> =
    try
        use conn = new SqliteConnection(connectionString)
        conn.Open()

        // Check if exists
        let exists =
            use checkCmd = conn.CreateCommand()
            checkCmd.CommandText <- "SELECT 1 FROM entries WHERE lower(word)=lower($word) LIMIT 1;"
            checkCmd.Parameters.AddWithValue("$word", word) |> ignore
            let res = checkCmd.ExecuteScalar()
            not (isNull res)

        use cmd = conn.CreateCommand()
        if exists then
            cmd.CommandText <- "UPDATE entries SET definition=$def WHERE lower(word)=lower($word);"
        else
            cmd.CommandText <- "INSERT INTO entries(word, definition) VALUES($word,$def);"
        cmd.Parameters.AddWithValue("$word", word) |> ignore
        cmd.Parameters.AddWithValue("$def", definition) |> ignore
        cmd.ExecuteNonQuery() |> ignore
        Ok exists
    with ex ->
        Error $"DB add/update failed: {ex.Message}"

let delete (word: string) : Result<bool, string> =
    try
        use conn = new SqliteConnection(connectionString)
        conn.Open()
        use cmd = conn.CreateCommand()
        cmd.CommandText <- "DELETE FROM entries WHERE lower(word)=lower($word);"
        cmd.Parameters.AddWithValue("$word", word) |> ignore
        let rows = cmd.ExecuteNonQuery()
        Ok (rows > 0)
    with ex ->
        Error $"DB delete failed: {ex.Message}"

let searchExact (word: string) : Result<Entry option, string> =
    try
        use conn = new SqliteConnection(connectionString)
        conn.Open()
        use cmd = conn.CreateCommand()
        cmd.CommandText <- "SELECT word, definition FROM entries WHERE lower(word)=lower($word) LIMIT 1;"
        cmd.Parameters.AddWithValue("$word", word) |> ignore
        use reader = cmd.ExecuteReader()
        if reader.Read() then
            Ok (Some (readEntry reader))
        else
            Ok None
    with ex ->
        Error $"DB search failed: {ex.Message}"

let searchByWordContains (fragment: string) : Result<Entry list, string> =
    try
        use conn = new SqliteConnection(connectionString)
        conn.Open()
        use cmd = conn.CreateCommand()
        cmd.CommandText <- "SELECT word, definition FROM entries WHERE lower(word) LIKE '%' || lower($frag) || '%' ORDER BY lower(word);"
        cmd.Parameters.AddWithValue("$frag", fragment) |> ignore
        use reader = cmd.ExecuteReader()
        let results =
            [ while reader.Read() do
                yield readEntry reader ]
        Ok results
    with ex ->
        Error $"DB search failed: {ex.Message}"

let searchAnyFieldContains (fragment: string) : Result<Entry list, string> =
    try
        use conn = new SqliteConnection(connectionString)
        conn.Open()
        use cmd = conn.CreateCommand()
        cmd.CommandText <- """
            SELECT word, definition
            FROM entries
            WHERE lower(word) LIKE '%' || lower($frag) || '%'
               OR lower(definition) LIKE '%' || lower($frag) || '%'
            ORDER BY lower(word);
            """
        cmd.Parameters.AddWithValue("$frag", fragment) |> ignore
        use reader = cmd.ExecuteReader()
        let results =
            [ while reader.Read() do
                yield readEntry reader ]
        Ok results
    with ex ->
        Error $"DB search failed: {ex.Message}"


