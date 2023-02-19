using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Vdcrpt.Desktop.Models;

namespace Vdcrpt.Desktop.ViewModels;

public partial class PresetEffectSettingsViewModel : ViewModelBase
{
    [ObservableProperty] private Preset _currentPreset;

    public PresetEffectSettingsViewModel(IReadOnlyList<Preset> presets, BinaryRepeatEffectSettings settings)
    {
        Presets = presets;
        EffectSettingsViewModel = new EffectSettingsViewModel(settings);

        if (presets.Count > 0)
        {
            CurrentPreset = presets[0];
        }
    }

    public PresetEffectSettingsViewModel() : this(new List<Preset>(), new BinaryRepeatEffectSettings())
    {
    }

    public IReadOnlyList<Preset> Presets { get; }
    public EffectSettingsViewModel EffectSettingsViewModel { get; }

    partial void OnCurrentPresetChanged(Preset value)
    {
        if (value is null)
        {
            return;
        }

        EffectSettingsViewModel.Settings.CopyFrom(value.Settings);
    }
}
