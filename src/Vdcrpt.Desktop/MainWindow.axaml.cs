using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Vdcrpt.Desktop;

public partial class MainWindow : Window
{
    private TextBox _inputPathTextBox;

    public MainWindow()
    {
        InitializeComponent();

#if DEBUG
        this.AttachDevTools();
#endif

        _inputPathTextBox = this.Find<TextBox>("InputPathTextBox");

        AddHandler(DragDrop.DropEvent, OnDrop);
    }

    private void OnDrop(object? sender, DragEventArgs e)
    {
        var filenames = e.Data.GetFileNames();
        if (filenames == null) return;

        var filenamesList = filenames.ToList();
        if (filenamesList.Count <= 0) return;
            
        // Propagates to viewmodel
        _inputPathTextBox.Text = filenamesList[0];
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private async void OnOpenPressed(object? sender, RoutedEventArgs routedEventArgs)
    {
        var dialog = new OpenFileDialog
        {
            Directory = ".",
            AllowMultiple = false,
            Filters =
            {
                new FileDialogFilter
                    { Name = "Common Video Files", Extensions = { "mp4", "avi", "mkv", "mov", "gif" } },
                new FileDialogFilter { Name = "All Files", Extensions = { "*" } },
            }
        };

        var result = await dialog.ShowAsync(this);
        if (result is not { Length: > 0 }) return;

        // Propagates to viewmodel
        _inputPathTextBox.Text = result[0];
    }
}