namespace DictionaryGui

// Avalonia application shell: loads global XAML and opens the main window.

open Avalonia
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.Markup.Xaml

type App() =
    inherit Application()

    override this.Initialize() =
        AvaloniaXamlLoader.Load(this)

    override this.OnFrameworkInitializationCompleted() =
        match this.ApplicationLifetime with
        | :? IClassicDesktopStyleApplicationLifetime as desktop ->
            desktop.MainWindow <- new MainWindow()
        | _ -> ()

        base.OnFrameworkInitializationCompleted()


