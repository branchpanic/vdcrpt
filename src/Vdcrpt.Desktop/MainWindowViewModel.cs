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

namespace Vdcrpt.Desktop
{
    public sealed class MainWindowViewModel : INotifyPropertyChanged
    {
        private string _inputPath = string.Empty;
        private int _trailLength = 10;
        private int _burstSize = 5000;
        private int _iterations = 5;
        private bool _openWhenComplete = true;
        private bool _askForFilename = true;

        private DelegateCommand _onCorruptPressed;
        private DelegateCommand _onOpenResultPressed;
        private DelegateCommand _openUrl;
        public ICommand OpenUrl => _openUrl;

        private const string DefaultProgressMessage = "Ready!";
        private int _progressAmount;
        private string _progressMessage = DefaultProgressMessage;
        private string _outputPath = string.Empty;

        private readonly BackgroundWorker _corruptWorker;

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

        public MainWindowViewModel()
        {
            _currentPreset = CurrentPreset = Presets[0];

            _corruptWorker = new BackgroundWorker();
            _corruptWorker.WorkerReportsProgress = true;

            _onCorruptPressed = new DelegateCommand(
                _ => File.Exists(_inputPath) && !_corruptWorker.IsBusy,
                obj =>
                {
                    if (!AskForFilename || string.IsNullOrEmpty(OutputPath))
                    {
                        OutputPath = GenerateOutputPath(_inputPath);
                    }

                    if (AskForFilename)
                    {
                        // FIXME: This blocks the UI thread!
                        var chosenPath = Task.Run(async () =>
                        {
                            var dialog = new SaveFileDialog
                            {
                                Directory = Path.GetDirectoryName(OutputPath),
                                InitialFileName = Path.GetFileName(OutputPath),
                                Filters =
                                {
                                    new FileDialogFilter { Extensions = { "mp4" }, Name = "MP4 video" },
                                },
                                DefaultExtension = "mp4",
                            };
                            return await dialog.ShowAsync((Window)obj!);
                        }).GetAwaiter().GetResult();

                        if (string.IsNullOrEmpty(chosenPath))
                        {
                            ProgressMessage = DefaultProgressMessage;
                            return;
                        }

                        OutputPath = chosenPath;
                    }

                    ProgressMessage = obj?.ToString() ?? "...";

                    _corruptWorker.RunWorkerAsync();
                    OnPropertyChanged(nameof(IsBusy));
                });

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
                        $"A tool (FFmpeg) did not behave as expected. Try re-extracting vdcrpt if the error persists: {args.Error.Message}",
                    not null => $"An unexpected error occurred: {args.Error.Message}",
                    null => $"Done! Saved at {_outputPath}.",
                };

                _onCorruptPressed.RaiseCanExecuteChanged();
                _onOpenResultPressed.RaiseCanExecuteChanged();
                OnPropertyChanged(nameof(IsBusy));

                if (args.Error is null && OpenWhenComplete) _onOpenResultPressed.Execute(null);
            };

            _openUrl = new DelegateCommand(url =>
            {
                if (url is not string urlString) return;

                Process.Start(new ProcessStartInfo
                {
                    FileName = urlString,
                    UseShellExecute = true
                });
            });
        }

        public bool IsBusy => _corruptWorker.IsBusy;

        private void DoBackgroundWork(object? sender, DoWorkEventArgs args)
        {
            if (sender is not BackgroundWorker worker) return;

            worker.ReportProgress(25, "Loading video...");
            var video = Video.Load(_inputPath);

            worker.ReportProgress(50, "Corrupting data...");
            video.Transform(Effects.Repeat(_iterations, _burstSize, _trailLength));

            worker.ReportProgress(75, "Rendering corrupted video...");
            video.Save(_outputPath);

            worker.ReportProgress(100, "Finishing up...");
        }

        public string InputPath
        {
            get => _inputPath;
            set
            {
                if (value == _inputPath) return;
                _inputPath = value;
                OnPropertyChanged();

                _onCorruptPressed.RaiseCanExecuteChanged();
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
        public int TrailLength
        {
            get => _trailLength;
            set
            {
                if (value == _trailLength) return;
                _trailLength = value;
                OnPropertyChanged();
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

        public string ApplicationVersion { get; } = "Version " +
                                                    (Assembly.GetExecutingAssembly().GetName().Version?.ToString(3)
                                                     ?? "<unknown>")
#if DEBUG
                                                    + " (DEBUG)"
#endif
            ;

        // TODO: User presets
        public List<Preset> Presets { get; } = new()
        {
            new Preset { Name = "Melting Chaos", BurstSize = 3000, TrailLength = 8, Iterations = 400 },
            new Preset { Name = "Jittery", BurstSize = 20000, TrailLength = 4, Iterations = 200 },
            new Preset { Name = "Source Engine", BurstSize = 45000, TrailLength = 5, Iterations = 60 },
            new Preset { Name = "Subtle", BurstSize = 200, TrailLength = 2, Iterations = 60 },
            new Preset { Name = "Many Artifacts", BurstSize = 500, TrailLength = 3, Iterations = 2000 },
            new Preset { Name = "Trash (unstable, breaks audio)", BurstSize = 1, TrailLength = 1, Iterations = 10000 },
            new Preset { Name = "Legacy", BurstSize = 1000, TrailLength = 50, Iterations = 50 },
        };

        private Preset _currentPreset;

        public Preset CurrentPreset
        {
            get => _currentPreset;
            set
            {
                if (value == _currentPreset) return;
                _currentPreset = value;
                OnPropertyChanged();

                BurstSize = _currentPreset.BurstSize;
                TrailLength = _currentPreset.TrailLength;
                Iterations = _currentPreset.Iterations;
            }
        }

        public ICommand OnCorruptPressed => _onCorruptPressed;
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

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}