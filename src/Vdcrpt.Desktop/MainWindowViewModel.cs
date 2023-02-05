using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data;
using FFMpegCore.Exceptions;
using JetBrains.Annotations;
using Vdcrpt;
using Vdcrpt.BuiltIns.Effects;

namespace Vdcrpt.Desktop;

// TODO: This should be broken into smaller components
public sealed class MainWindowViewModel : INotifyPropertyChanged
{
    private const string DefaultProgressMessage = "Ready!";

    private readonly BackgroundWorker _corruptWorker;

    private string _inputPath = string.Empty;
    private Preset _currentPreset;
    private int _burstSize = 5000;
    private int _iterations = 5;
    private int _minTrailLength = 10;
    private int _maxTrailLength = 20;

    // TODO: Avalonia lets us bind directly to methods, commands not always necessary
    private DelegateCommand _onOpenResultPressed;
    private bool _openWhenComplete = false;
    private bool _askForFilename = true;
    private string _outputPath = string.Empty;

    private int _progressAmount;
    private string _progressMessage = DefaultProgressMessage;

    #region Properties

    public ICommand OpenUrl { get; } = new DelegateCommand(url =>
    {
        if (url is not string urlString) return;

        Process.Start(new ProcessStartInfo
        {
            FileName = urlString,
            UseShellExecute = true
        });
    });

    public bool CanStartCorrupting => File.Exists(InputPath) && !IsBusy;

    public bool IsBusy => _corruptWorker.IsBusy;

    public string InputPath
    {
        get => _inputPath;
        set
        {
            if (value == _inputPath) return;
            _inputPath = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanStartCorrupting));

            if (!File.Exists(_inputPath)) throw new DataValidationException("File does not exist.");
        }
    }

    [Range(1, int.MaxValue)]
    public int BurstSize
    {
        get => _burstSize;
        set
        {
            if (value == _burstSize) return;
            _burstSize = value;
            OnPropertyChanged();
        }
    }

    [Range(1, int.MaxValue)]
    public int MinTrailLength
    {
        get => _minTrailLength;
        set
        {
            if (value == _minTrailLength) return;
            _minTrailLength = value;
            OnPropertyChanged();

            if (_maxTrailLength < _minTrailLength) _maxTrailLength = _minTrailLength;
            OnPropertyChanged(nameof(MaxTrailLength));
        }
    }

    [Range(1, int.MaxValue)]
    public int MaxTrailLength
    {
        get => _maxTrailLength;
        set
        {
            if (value == _maxTrailLength) return;
            _maxTrailLength = value;
            OnPropertyChanged();

            if (_minTrailLength > _maxTrailLength) _minTrailLength = _maxTrailLength;
            OnPropertyChanged(nameof(MinTrailLength));
        }
    }

    private bool _useTrailLengthRange;
    public int MinTrailLengthColumnSpan => _useTrailLengthRange ? 1 : 3;

    public bool UseTrailLengthRange
    {
        get => _useTrailLengthRange;
        set
        {
            if (value == _useTrailLengthRange) return;
            _useTrailLengthRange = value;

            OnPropertyChanged();
            OnPropertyChanged(nameof(MinTrailLengthColumnSpan));
        }
    }

    [Range(1, int.MaxValue)]
    public int Iterations
    {
        get => _iterations;
        set
        {
            if (value == _iterations) return;
            _iterations = value;
            OnPropertyChanged();
        }
    }

    public bool OpenWhenComplete
    {
        get => _openWhenComplete;
        set
        {
            if (value == _openWhenComplete) return;
            _openWhenComplete = value;
            OnPropertyChanged();
        }
    }

    public bool AskForFilename
    {
        get => _askForFilename;
        set
        {
            if (value == _askForFilename) return;
            _askForFilename = value;
            OnPropertyChanged();
        }
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

    // TODO: User-defined presets
    public List<Preset> Presets { get; } = Preset.DefaultPresets;

    public Preset CurrentPreset
    {
        get => _currentPreset;
        set
        {
            if (value == _currentPreset) return;
            _currentPreset = value;
            OnPropertyChanged();

            BurstSize = _currentPreset.BurstSize;
            MinTrailLength = _currentPreset.MinBurstLength;

            UseTrailLengthRange = _currentPreset.UseLengthRange;
            if (_currentPreset.UseLengthRange) MaxTrailLength = _currentPreset.MaxBurstLength;

            Iterations = _currentPreset.Iterations;
        }
    }

    #endregion


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
        _currentPreset = CurrentPreset = Presets[0];

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

            if (args.Error is null && OpenWhenComplete) _onOpenResultPressed.Execute(null);
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

        if (!AskForFilename || string.IsNullOrEmpty(OutputPath))
        {
            OutputPath = GenerateOutputPath(_inputPath);
        }

        if (AskForFilename)
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

    private void DoBackgroundWork(object? sender, DoWorkEventArgs args)
    {
        if (sender is not BackgroundWorker worker) return;

        var effect = new BinaryRepeatEffect
        {
            Iterations = _iterations,
            BurstSize = _burstSize,
            MinBurstLength = _minTrailLength,
            MaxBurstLength = UseTrailLengthRange ? _maxTrailLength : _minTrailLength,
        };

        worker.ReportProgress(50, "Corrupting data...");
        Session.ApplyEffects(_inputPath, _outputPath, effect);

        worker.ReportProgress(100, "Finishing up...");
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    [NotifyPropertyChangedInvocator]
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}