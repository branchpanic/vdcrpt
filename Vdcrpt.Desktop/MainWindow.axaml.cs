using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Vdcrpt.Desktop
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

#if DEBUG
            this.AttachDevTools();
#endif
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
                    new FileDialogFilter { Name = "Common Video Files", Extensions = { "mp4", "avi", "mkv", "mov", "gif" } },
                    new FileDialogFilter { Name = "All Files", Extensions = { "*" } },
                }
            };

            var result = await dialog.ShowAsync(this);
            if (result.Length <= 0) return;
            
            this.Find<TextBox>("InputPathTextBox").Text = result[0];
        }
    }
}