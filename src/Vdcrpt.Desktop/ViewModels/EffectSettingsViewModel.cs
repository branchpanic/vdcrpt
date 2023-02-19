using Vdcrpt.Desktop.Models;

namespace Vdcrpt.Desktop.ViewModels;

public class EffectSettingsViewModel : ViewModelBase
{
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

    public EffectSettingsViewModel() : this(new BinaryRepeatEffectSettings())
    {
    }

    public BinaryRepeatEffectSettings Settings { get; }
    public int MinBurstLengthColumnSpan => Settings.UseBurstLengthRange ? 1 : 3;
}