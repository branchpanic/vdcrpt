using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data;
using FFMpegCore.Exceptions;

namespace Vdcrpt.Desktop.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    private const string DefaultProgressMessage = "Ready!";

    private readonly BackgroundWorker _corruptWorker;

    private string _inputPath = string.Empty;
    private EffectSettingsViewModel _effectSettings;
    private OutputSettingsViewModel _outputSettings;

    // TODO: Avalonia lets us bind directly to methods, commands not always necessary
    private DelegateCommand _onOpenResultPressed;
    private string _outputPath = string.Empty;

    private int _progressAmount;
    private string _progressMessage = DefaultProgressMessage;

    public bool CanStartCorrupting => File.Exists(InputPath) && !IsBusy;

    public bool IsBusy => _corruptWorker.IsBusy;

    public string InputPath
    {
        get => _inputPath;
        set
        {
            if (value == _inputPath)
            {
                return;
            }
            _inputPath = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanStartCorrupting));

            if (!File.Exists(_inputPath))
            {
                throw new DataValidationException("File does not exist.");
            }
        }
    }

    public EffectSettingsViewModel EffectViewModel
    {
        get => _effectSettings;
        set => RaiseAndSetIfChanged(ref _effectSettings, value);
    }

    public OutputSettingsViewModel OutputSettingsViewModel
    {
        get => _outputSettings;
        set => RaiseAndSetIfChanged(ref _outputSettings, value);
    }

    public int ProgressAmount
    {
        get => _progressAmount;
        set
        {
            if (value == _progressAmount) return;
            _progressAmount = value;
            OnPropertyChanged();
        }
    }

    public string ProgressMessage
    {
        get => _progressMessage;
        set
        {
            if (value == _progressMessage) return;
            _progressMessage = value;
            OnPropertyChanged();
        }
    }

    public string OutputPath
    {
        get => _outputPath;
        set
        {
            if (value == _outputPath) return;
            _outputPath = value;
            OnPropertyChanged();
            _onOpenResultPressed.RaiseCanExecuteChanged();
        }
    }

    public string VersionText
    {
        get
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var versionString = version is not null
                ? $"{version.Major:00}.{version.Minor:00}.{version.Build:00}"
                : "UNKNOWN";
#if DEBUG
            versionString += " (DEBUG)";
#endif

            return $"Version {versionString}";
        }
    }

    public ICommand OnOpenResultPressed => _onOpenResultPressed;

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

    public MainWindowViewModel()
    {
        _effectSettings = new EffectSettingsViewModel();
        _outputSettings = new OutputSettingsViewModel();

        _corruptWorker = new BackgroundWorker();
        _corruptWorker.WorkerReportsProgress = true;

        _onOpenResultPressed = new DelegateCommand(
            _ => File.Exists(_outputPath) && !_corruptWorker.IsBusy,
            _ => new Process
            {
                StartInfo = new ProcessStartInfo(OutputPath)
                {
                    UseShellExecute = true
                }
            }.Start()
        );

        _corruptWorker.DoWork += DoBackgroundWork;
        _corruptWorker.ProgressChanged += (_, args) =>
        {
            ProgressAmount = args.ProgressPercentage;
            if (args.UserState is string state)
            {
                ProgressMessage = state;
            }
        };

        _corruptWorker.RunWorkerCompleted += (_, args) =>
        {
            ProgressAmount = 0;

            ProgressMessage = args.Error switch
            {
                FFMpegException { Type: FFMpegExceptionType.Process } =>
                    "The video failed to render. Try again or lower the settings.",
                FFMpegException { Type: FFMpegExceptionType.Operation } =>
                    $"FFmpeg did not behave as expected. Redownload vdcrpt or file a bug report if the error persists: {args.Error.Message}",
                not null => $"An unexpected error occurred: {args.Error.Message}",
                null => $"Done! Saved at {_outputPath}.",
            };

            _onOpenResultPressed.RaiseCanExecuteChanged();

            OnPropertyChanged(nameof(IsBusy));
            OnPropertyChanged(nameof(CanStartCorrupting));

            if (args.Error is null && _outputSettings.OpenWhenComplete) _onOpenResultPressed.Execute(null);
        };
    }

    private static string GenerateOutputPath(string inputPath)
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

    public async Task StartCorrupting()
    {
        if (Application.Current?.ApplicationLifetime is not ClassicDesktopStyleApplicationLifetime app) return;

        if (!_outputSettings.AskForFilename || string.IsNullOrEmpty(OutputPath))
        {
            OutputPath = GenerateOutputPath(_inputPath);
        }

        if (_outputSettings.AskForFilename)
        {
            var dialog = new SaveFileDialog
            {
                Directory = Path.GetDirectoryName(OutputPath),
                InitialFileName = Path.GetFileName(OutputPath),
                Filters = new List<FileDialogFilter>
                {
                    new FileDialogFilter { Extensions = { "mp4" }, Name = "MP4 video" },
                },
                DefaultExtension = "mp4",
            };

            var chosenPath = await dialog.ShowAsync(app.MainWindow);

            if (string.IsNullOrEmpty(chosenPath))
            {
                ProgressMessage = DefaultProgressMessage;
                return;
            }

            OutputPath = chosenPath;
        }

        _corruptWorker.RunWorkerAsync();
        OnPropertyChanged(nameof(IsBusy));
        OnPropertyChanged(nameof(CanStartCorrupting));
    }

    public void OpenUrl(string url)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }

    private void DoBackgroundWork(object? sender, DoWorkEventArgs args)
    {
        if (sender is not BackgroundWorker worker) return;

        var effect = _effectSettings.MakeEffect();

        worker.ReportProgress(50, "Corrupting data...");
        Session.ApplyEffects(_inputPath, _outputPath, effect);

        worker.ReportProgress(100, "Finishing up...");
    }
}
