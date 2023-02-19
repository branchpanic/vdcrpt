using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Vdcrpt.Desktop.Views;

public class ProjectView : UserControl
{
    private readonly TextBox _inputPathTextBox;

    public ProjectView()
    {
        InitializeComponent();

        _inputPathTextBox = this.Find<TextBox>("InputPathTextBox");
        AddHandler(DragDrop.DropEvent, OnDrop);
    }

    private void OnDrop(object? sender, DragEventArgs e)
    {
        var filenames = e.Data.GetFileNames();
        if (filenames == null)
        {
            return;
        }

        var filenamesList = filenames.ToList();
        if (filenamesList.Count <= 0)
        {
            return;
        }

        // Propagates to viewmodel
        _inputPathTextBox.Text = filenamesList[0];
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    // TODO: Rewrite this as a service
    private async void OnOpenPressed(object? sender, RoutedEventArgs routedEventArgs)
    {
        if (Application.Current!.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            return;
        }

        var dialog = new OpenFileDialog
        {
            Directory = ".",
            AllowMultiple = false,
            Filters = new List<FileDialogFilter>
            {
                new() { Name = "Common Video Files", Extensions = { "mp4", "avi", "mkv", "mov", "gif" } },
                new() { Name = "All Files", Extensions = { "*" } }
            }
        };

        var result = await dialog.ShowAsync(desktopLifetime.MainWindow);
        if (result is not { Length: > 0 })
        {
            return;
        }

        // Propagates to viewmodel
        _inputPathTextBox.Text = result[0];
    }
}