using CommunityToolkit.Mvvm.ComponentModel;
using Vdcrpt.Desktop.Models;

namespace Vdcrpt.Desktop.ViewModels;

public partial class EffectSettingsViewModel : ViewModelBase
{
    public BinaryRepeatEffectSettings Settings { get; }
    public int MinBurstLengthColumnSpan => Settings.UseBurstLengthRange ? 1 : 3;

    public EffectSettingsViewModel(BinaryRepeatEffectSettings settings)
    {
        Settings = settings;
        Settings.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(Settings.UseBurstLengthRange))
            {
                OnPropertyChanged(nameof(MinBurstLengthColumnSpan));
            }
        };
    }

    public EffectSettingsViewModel() : this(new BinaryRepeatEffectSettings()) { }
}
