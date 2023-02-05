using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vdcrpt.BuiltIns.Effects;

namespace Vdcrpt.Desktop.ViewModels
{
    public class EffectSettingsViewModel : ViewModelBase
    {
        private Preset _preset;
        private int _burstSize;
        private int _minBurstLength;
        private int _maxBurstLength;
        private int _iterations;
        private bool _useBurstLengthRange;

        // TODO: User-defined presets
        public List<Preset> Presets { get; } = Preset.DefaultPresets;

        public Preset CurrentPreset
        {
            get => _preset;
            set
            {
                RaiseAndSetIfChanged(ref _preset, value);

                BurstSize = _preset.BurstSize;
                MinBurstLength = _preset.MinBurstLength;

                UseBurstLengthRange = _preset.UseLengthRange;
                if (_preset.UseLengthRange) MaxBurstLength = _preset.MaxBurstLength;

                Iterations = _preset.Iterations;
            }
        }

        [Range(1, int.MaxValue)]
        public int BurstSize
        {
            get => _burstSize;
            set => RaiseAndSetIfChanged(ref _burstSize, value);
        }

        [Range(1, int.MaxValue)]
        public int MinBurstLength
        {
            get => _minBurstLength;
            set
            {
                RaiseAndSetIfChanged(ref _minBurstLength, value);

                if (_maxBurstLength < _minBurstLength) _maxBurstLength = _minBurstLength;
                OnPropertyChanged(nameof(MaxBurstLength));
            }
        }

        [Range(1, int.MaxValue)]
        public int MaxBurstLength
        {
            get => _maxBurstLength;
            set
            {
                RaiseAndSetIfChanged(ref _maxBurstLength, value);

                if (_minBurstLength > _maxBurstLength) _minBurstLength = _maxBurstLength;
                OnPropertyChanged(nameof(MinBurstLength));
            }
        }

        [Range(1, int.MaxValue)]
        public int Iterations
        {
            get => _iterations;
            set => RaiseAndSetIfChanged(ref _iterations, value);
        }

        public bool UseBurstLengthRange
        {
            get => _useBurstLengthRange;
            set
            {
                if (value == _useBurstLengthRange) return;
                _useBurstLengthRange = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(MinBurstLengthColumnSpan));
            }
        }

        public int MinBurstLengthColumnSpan => _useBurstLengthRange ? 1 : 3;

        public EffectSettingsViewModel()
        {
            _preset = CurrentPreset = Presets[0];
        }

        public BinaryRepeatEffect MakeEffect() => new()
        {
            BurstSize = _burstSize,
            MinBurstLength = _minBurstLength,
            MaxBurstLength = _useBurstLengthRange ? _maxBurstLength : _minBurstLength,
            Iterations = _iterations,
        };
    }
}
