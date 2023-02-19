using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vdcrpt.Desktop.Models;

namespace Vdcrpt.Desktop.ViewModels;

public partial class ProjectViewModel : ViewModelBase
{
    private const string DefaultProgressMessage = "Ready!";

    [ObservableProperty] private bool _isBusy;

    // TODO: Maybe part of model?
    [ObservableProperty] private string _outputPath;
    [ObservableProperty] private double _progressAmount;
    [ObservableProperty] private string _progressMessage;
    [ObservableProperty] private bool _running;

    public ProjectViewModel(Project project)
    {
        Project = project;
        PresetEffectSettingsViewModel =
            new PresetEffectSettingsViewModel(project.Config.Presets, project.EffectSettings);
        OutputSettingsViewModel = new OutputSettingsViewModel(project.Config);

        Project.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(Project.InputFile))
            {
                StartCorruptingCommand.NotifyCanExecuteChanged();
            }
        };
    }

    public ProjectViewModel() : this(new Project())
    {
    }

    public Project Project { get; }
    public PresetEffectSettingsViewModel PresetEffectSettingsViewModel { get; }
    public OutputSettingsViewModel OutputSettingsViewModel { get; }

    [RelayCommand(CanExecute = nameof(CanOpenResult))]
    private void OpenResult()
    {
        new Process
        {
            StartInfo = new ProcessStartInfo(OutputPath)
            {
                UseShellExecute = true
            }
        }.Start();
    }

    private bool CanOpenResult()
    {
        return File.Exists(OutputPath);
    }

    [RelayCommand(CanExecute = nameof(CanStartCorrupting))]
    private async Task StartCorrupting()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            return;
        }

        if (Project.Config.AskForFilename)
        {
            var dialog = new SaveFileDialog
            {
                Directory = Path.GetDirectoryName(OutputPath),
                InitialFileName = Path.GetFileName(OutputPath),
                Filters = new List<FileDialogFilter>
                {
                    new() { Extensions = { "mp4" }, Name = "MP4 video" }
                },
                DefaultExtension = "mp4"
            };

            var chosenPath = await dialog.ShowAsync(desktop.MainWindow);

            if (string.IsNullOrEmpty(chosenPath))
            {
                ProgressMessage = DefaultProgressMessage;
                return;
            }

            OutputPath = chosenPath;
        }
        else
        {
            OutputPath = Project.GenerateOutputPath(Project.InputFile);
        }

        try
        {
            Running = true;
            await Task.Run(() => Project.Render(OutputPath, (progress, message) =>
            {
                ProgressMessage = message;
                ProgressAmount = progress;
            }));
        }
        finally
        {
            Running = false;
            OpenResultCommand.NotifyCanExecuteChanged();
        }

        if (Project.Config.OpenWhenComplete)
        {
            OpenResult();
        }
    }

    private bool CanStartCorrupting()
    {
        return !Running && File.Exists(Project.InputFile);
    }
}
