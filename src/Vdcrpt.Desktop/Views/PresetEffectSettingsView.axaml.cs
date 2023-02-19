using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Vdcrpt.Desktop.Views
{
    public partial class PresetEffectSettingsView : UserControl
    {
        public PresetEffectSettingsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

