using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using Vdcrpt.BuiltIns.Effects;
using Vdcrpt.Desktop.ViewModels;

namespace Vdcrpt.Desktop.Models
{
    public partial class BinaryRepeatEffectSettings : ObservableValidator
    {
        [ObservableProperty] [Range(1, int.MaxValue)]
        private int _burstSize;

        [ObservableProperty] [Range(1, int.MaxValue)]
        private int _iterations;

        [ObservableProperty] [Range(1, int.MaxValue)]
        private int _maxBurstLength;

        [ObservableProperty] [Range(1, int.MaxValue)]
        private int _minBurstLength;

        [ObservableProperty] private bool _useBurstLengthRange;

        public BinaryRepeatEffect ToEffectInstance()
        {
            return new()
            {
                Iterations = Iterations,
                MinBurstLength = MinBurstLength,
                MaxBurstLength = UseBurstLengthRange ? MaxBurstLength : MinBurstLength,
                BurstSize = BurstSize
            };
        }

        public void CopyFrom(in BinaryRepeatEffectSettings other)
        {
            BurstSize = other.BurstSize;
            Iterations = other.Iterations;
            MaxBurstLength = other.MaxBurstLength;
            MinBurstLength = other.MinBurstLength;
            UseBurstLengthRange = other.UseBurstLengthRange;
        }

        partial void OnMinBurstLengthChanged(int value)
        {
            if (value > MaxBurstLength)
            {
                MaxBurstLength = value;
            }
        }

        partial void OnMaxBurstLengthChanged(int value)
        {
            if (value < MinBurstLength)
            {
                MinBurstLength = value;
            }
        }
    }
}
