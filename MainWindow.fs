namespace DictionaryGui

open System
open Avalonia
open Avalonia.Controls
open Avalonia.Markup.Xaml
open Avalonia.Interactivity

open Dictionary
open Database

type MainWindow() as this =
    inherit Window()

    let mutable entries: Entry list = []

    // Controls (assigned after XAML is loaded)
    let mutable wordBox: TextBox = Unchecked.defaultof<_>
    let mutable definitionBox: TextBox = Unchecked.defaultof<_>
    let mutable searchBox: TextBox = Unchecked.defaultof<_>
    let mutable searchModeBox: ComboBox = Unchecked.defaultof<_>
    let mutable resultsList: ListBox = Unchecked.defaultof<_>
    let mutable addUpdateButton: Button = Unchecked.defaultof<_>
    let mutable deleteButton: Button = Unchecked.defaultof<_>
    let mutable saveButton: Button = Unchecked.defaultof<_>
    let mutable loadButton: Button = Unchecked.defaultof<_>
    let mutable searchButton: Button = Unchecked.defaultof<_>
    let mutable clearSearchButton: Button = Unchecked.defaultof<_>
    let mutable exitButton: Button = Unchecked.defaultof<_>

    let refreshList (items: Entry list option) =
        let toShow =
            match items with
            | Some xs -> xs
            | None -> entries

        // Bind entries to the ItemsSource of the list
        resultsList.ItemsSource <- (toShow :> System.Collections.IEnumerable)

    let showError (msg: string) =
        // Simple feedback via window title to avoid extra dependencies
        this.Title <- $"Error: {msg}"

    let showInfo (msg: string) =
        this.Title <- msg

    do
        AvaloniaXamlLoader.Load(this)

        // Resolve controls
        wordBox <- this.FindControl<TextBox>("wordBox")
        definitionBox <- this.FindControl<TextBox>("definitionBox")
        searchBox <- this.FindControl<TextBox>("searchBox")
        searchModeBox <- this.FindControl<ComboBox>("searchModeBox")
        resultsList <- this.FindControl<ListBox>("resultsList")
        addUpdateButton <- this.FindControl<Button>("addUpdateButton")
        deleteButton <- this.FindControl<Button>("deleteButton")
        saveButton <- this.FindControl<Button>("saveButton")
        loadButton <- this.FindControl<Button>("loadButton")
        searchButton <- this.FindControl<Button>("searchButton")
        clearSearchButton <- this.FindControl<Button>("clearSearchButton")
        exitButton <- this.FindControl<Button>("exitButton")

        // Initial load
        // Initialize database and load entries
        match Database.initialize() with
        | Ok () ->
            match Database.getAllEntries() with
            | Ok es ->
                entries <- es
                refreshList None
                showInfo "Database ready."
            | Error msg ->
                showError msg
                entries <- []
                refreshList None
        | Error msg ->
            showError msg
            entries <- []
            refreshList None

        // Wire up events
        addUpdateButton.Click.Add(fun _ ->
            let word = wordBox.Text |> string
            let def = definitionBox.Text |> string
            if String.IsNullOrWhiteSpace word || String.IsNullOrWhiteSpace def then
                showError "Word and definition cannot be empty."
            else
                match Database.addOrUpdate word def with
                | Ok wasUpdate ->
                    match Database.getAllEntries() with
                    | Ok es ->
                        entries <- es
                        refreshList None
                        if wasUpdate then showInfo "Updated existing entry."
                        else showInfo "Added new entry."
                    | Error msg -> showError msg
                | Error msg -> showError msg)

        deleteButton.Click.Add(fun _ ->
            match resultsList.SelectedItem with
            | :? Entry as entry ->
                match Database.delete entry.Word with
                | Ok removed ->
                    if removed then
                        match Database.getAllEntries() with
                        | Ok es ->
                            entries <- es
                            refreshList None
                            showInfo $"Deleted '{entry.Word}'."
                        | Error msg -> showError msg
                    else
                        showError "Entry not found."
                | Error msg -> showError msg
            | _ ->
                showError "Select an entry to delete.")

        saveButton.Click.Add(fun _ ->
            // SQLite persists automatically; just refresh view
            match Database.getAllEntries() with
            | Ok es ->
                entries <- es
                refreshList None
                showInfo "Database refreshed (saved)."
            | Error msg -> showError msg)

        loadButton.Click.Add(fun _ ->
            match Database.getAllEntries() with
            | Ok es ->
                entries <- es
                refreshList None
                showInfo "Loaded from database."
            | Error msg -> showError msg)

        searchButton.Click.Add(fun _ ->
            let text = searchBox.Text |> string
            let idx = searchModeBox.SelectedIndex
            let results =
                match idx with
                | 0 -> // Exact word
                    match Database.searchExact text with
                    | Ok (Some e) -> [ e ]
                    | Ok None -> []
                    | Error msg ->
                        showError msg
                        []
                | 1 -> // Partial word
                    match Database.searchByWordContains text with
                    | Ok es -> es
                    | Error msg ->
                        showError msg
                        []
                | 2 -> // Word or definition
                    match Database.searchAnyFieldContains text with
                    | Ok es -> es
                    | Error msg ->
                        showError msg
                        []
                | _ ->
                    []
            refreshList (Some results))

        clearSearchButton.Click.Add(fun _ ->
            searchBox.Text <- ""
            refreshList None)

        exitButton.Click.Add(fun _ ->
            this.Close())

