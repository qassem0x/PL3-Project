module Dictionary

// Pure domain helpers: entry model, in-memory dictionary operations, and JSON utilities.

open System
open System.Text.Json
open System.Text.Json.Serialization

/// A single dictionary entry
type Entry =
    { Word: string
      Definition: string }

/// The full dictionary state: case-insensitive `Map` keyed by normalized word.
type Dictionary = Map<string, Entry>

/// Normalize a word for case-insensitive operations.
let private normalize (word: string) =
    word.Trim().ToLowerInvariant()

/// Create an empty dictionary.
let empty : Dictionary = Map.empty

/// Add or update a word.
/// Returns the updated dictionary and a flag indicating whether this was an update (true) or an insert (false).
let addOrUpdate (word: string) (definition: string) (dict: Dictionary) : Dictionary * bool =
    let key = normalize word
    let updated =
        dict
        |> Map.add key { Word = word; Definition = definition }
    let wasUpdate = dict |> Map.containsKey key
    updated, wasUpdate

/// Try to get an exact word (case-insensitive).
let tryGet (word: string) (dict: Dictionary) : Entry option =
    let key = normalize word
    dict |> Map.tryFind key

/// Delete a word. Returns the updated dictionary and a flag indicating whether something was removed.
let delete (word: string) (dict: Dictionary) : Dictionary * bool =
    let key = normalize word
    if dict |> Map.containsKey key then
        dict |> Map.remove key, true
    else
        dict, false

/// Case-insensitive partial match search on the *word* field.
let searchByWordContains (fragment: string) (dict: Dictionary) : Entry list =
    let frag = fragment.Trim().ToLowerInvariant()
    if String.IsNullOrWhiteSpace frag then
        []
    else
        dict
        |> Seq.map (fun kv -> kv.Value)
        |> Seq.filter (fun e -> e.Word.ToLowerInvariant().Contains frag)
        |> Seq.toList

/// Case-insensitive partial match search on *word or definition*.
let searchAnyFieldContains (fragment: string) (dict: Dictionary) : Entry list =
    let frag = fragment.Trim().ToLowerInvariant()
    if String.IsNullOrWhiteSpace frag then
        []
    else
        dict
        |> Seq.map (fun kv -> kv.Value)
        |> Seq.filter (fun e ->
            e.Word.ToLowerInvariant().Contains frag
            || e.Definition.ToLowerInvariant().Contains frag)
        |> Seq.toList

/// Internal DTO for serialization (so we don't expose Map internals).
type private SerializableState =
    { Entries: Entry list }

let private jsonOptions =
    JsonSerializerOptions(
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    )

/// Serialize dictionary to JSON string.
let toJson (dict: Dictionary) : string =
    let state: SerializableState =
        { Entries = dict |> Seq.map (fun kv -> kv.Value) |> Seq.toList }
    JsonSerializer.Serialize(state, jsonOptions)

/// Deserialize dictionary from JSON string.
let ofJson (json: string) : Result<Dictionary, string> =
    try
        let state =
            JsonSerializer.Deserialize<SerializableState>(json, jsonOptions)

        // JsonSerializer may return null if the JSON is empty or invalid for this shape
        if obj.ReferenceEquals(state, null) then
            Ok empty
        else
            let dict =
                state.Entries
                |> Seq.fold
                    (fun acc e ->
                        let key = normalize e.Word
                        acc |> Map.add key e)
                    Map.empty
            Ok dict
    with ex ->
        Error($"Failed to parse JSON: {ex.Message}")

open System.IO

/// Save dictionary as JSON to a file.
let saveToFile (path: string) (dict: Dictionary) : Result<unit, string> =
    try
        let json = toJson dict
        File.WriteAllText(path, json)
        Ok()
    with ex ->
        Error($"Failed to save to file: {ex.Message}")

/// Load dictionary from a JSON file. Returns empty if file does not exist.
let loadFromFile (path: string) : Result<Dictionary, string> =
    try
        if File.Exists path |> not then
            Ok empty
        else
            let json = File.ReadAllText path
            ofJson json
    with ex ->
        Error($"Failed to load from file: {ex.Message}")


