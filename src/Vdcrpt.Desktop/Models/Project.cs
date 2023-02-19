using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Vdcrpt.Desktop.Models;

public partial class Project : ObservableValidator
{
    [ObservableProperty] [CustomValidation(typeof(Project), nameof(ValidateInputFile))]
    private string _inputFile;

    public Project()
    {
        _inputFile = string.Empty;
        Config = new UserConfig();
        EffectSettings = new BinaryRepeatEffectSettings();
    }

    public UserConfig Config { get; set; }
    public BinaryRepeatEffectSettings EffectSettings { get; set; }

    public static ValidationResult ValidateInputFile(string inputFile, ValidationContext context)
    {
        if (!File.Exists(inputFile))
        {
            return new ValidationResult("File does not exist.");
        }

        return ValidationResult.Success!;
    }

    public void Render(string outputPath, Action<double, string> reportProgress = null)
    {
        var effect = EffectSettings.ToEffectInstance();
        reportProgress?.Invoke(0.5, "Corrupting data...");
        Session.ApplyEffects(InputFile, outputPath, effect);
        reportProgress?.Invoke(1.0, "Finishing up...");
    }

    public static string GenerateOutputPath(string inputPath)
    {
        var pathNoExt = Path.ChangeExtension(inputPath, null);
        var pathTimestampNoExt = $"{pathNoExt}_vdcrpt";

        string result;
        var increment = 0;

        do
        {
            result = Path.ChangeExtension($"{pathTimestampNoExt}_{increment++:00}", "mp4");
        } while (File.Exists(result));

        return result;
    }
}