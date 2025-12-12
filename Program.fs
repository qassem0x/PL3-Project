namespace DictionaryGui

// Application entry point: configures Avalonia and starts the desktop lifetime.

open System
open Avalonia
open Avalonia.ReactiveUI
open Avalonia.Controls.ApplicationLifetimes

module Program =

    let buildAvaloniaApp() : AppBuilder =
        AppBuilder
            .Configure<App>()
            .UsePlatformDetect()
            .UseReactiveUI()

    [<EntryPoint; STAThread>]
    let main argv =
        buildAvaloniaApp()
            .StartWithClassicDesktopLifetime(argv)
        |> ignore
        0

