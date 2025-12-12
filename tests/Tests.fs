module DictionaryApp.Tests

// Unit tests for the pure Dictionary module (no GUI/DB).

open Xunit
open Dictionary

[<Fact>]
let ``addOrUpdate inserts then updates case-insensitively`` () =
    // Insert new word, then update using different casing; lookup remains case-insensitive.
    let dict1, wasUpdate1 = addOrUpdate "Hello" "world" Dictionary.empty
    let dict2, wasUpdate2 = addOrUpdate "hello" "everyone" dict1
    Assert.False(wasUpdate1)
    Assert.True(wasUpdate2)
    let entry = dict2 |> tryGet "HELLO" |> Option.defaultValue { Word = ""; Definition = "" }
    Assert.Equal("hello", entry.Word.ToLowerInvariant())
    Assert.Equal("everyone", entry.Definition)

[<Fact>]
let ``delete removes existing entry and leaves others intact`` () =
    // Delete one entry and ensure others stay present.
    let dict1, _ = addOrUpdate "One" "1" Dictionary.empty
    let dict2, _ = addOrUpdate "Two" "2" dict1
    let dictAfterDelete, removed = delete "one" dict2
    Assert.True(removed)
    Assert.True(dictAfterDelete |> tryGet "one" |> Option.isNone)
    Assert.True(dictAfterDelete |> tryGet "Two" |> Option.isSome)

[<Fact>]
let ``searchByWordContains is case-insensitive and trims input`` () =
    // Partial word search should ignore case and surrounding whitespace.
    let dict, _ = addOrUpdate "Notebook" "Paper" Dictionary.empty
    let results = searchByWordContains "  book  " dict
    Assert.Single(results)
    Assert.Equal("Notebook", results[0].Word)

[<Fact>]
let ``searchAnyFieldContains matches word or definition`` () =
    // Should match fragment in either word or definition.
    let dict1, _ = addOrUpdate "Cloud" "Water vapor" Dictionary.empty
    let dict2, _ = addOrUpdate "Rain" "Cloud droplets" dict1
    let results = searchAnyFieldContains "cloud" dict2
    Assert.Equal(2, results.Length)

[<Fact>]
let ``json roundtrip preserves entries`` () =
    // Serialize then deserialize; resulting map should equal original.
    let dict1, _ = addOrUpdate "Alpha" "First" Dictionary.empty
    let dict2, _ = addOrUpdate "Beta" "Second" dict1
    let json = toJson dict2
    let parsed = ofJson json |> Result.defaultValue Dictionary.empty
    Assert.True(parsed = dict2)

