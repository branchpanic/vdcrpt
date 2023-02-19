using System;
using System.Diagnostics;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using Vdcrpt.Desktop.Models;

namespace Vdcrpt.Desktop.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] private Project _project;

    public MainWindowViewModel()
    {
        var config = UserConfig.CreateDefault();

        _project = new Project
        {
            InputFile = string.Empty,
            Config = config,
            EffectSettings = new BinaryRepeatEffectSettings()
        };

        ProjectViewModel = new ProjectViewModel(_project);
    }

    public ProgramInfo ProgramInfo => ProgramInfo.Default;
    public string VersionWithPrefix => string.Concat("Version ", ProgramInfo.Version);

    public ICommand OnExitPressed { get; } = new DelegateCommand(_ =>
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
        else
        {
            // Not sure what's best to do here, but we shouldn't ever reach this branch anyway.
            Environment.Exit(0);
        }
    });

    public ProjectViewModel ProjectViewModel { get; }

    public void OpenUrl(string url)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }
}
