using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Avalonia.Data;
using JetBrains.Annotations;

namespace Vdcrpt.Desktop
{
    class DelegateCommand : ICommand
    {
        private readonly Func<object?, bool> _doCanExecute;
        private readonly Action<object?> _doExecute;

        public DelegateCommand(Action<object?> doExecute) : this(_ => true, doExecute)
        {
        }

        public DelegateCommand(Func<object?, bool> doCanExecute, Action<object?> doExecute)
        {
            _doCanExecute = doCanExecute;
            _doExecute = doExecute;
        }

        public bool CanExecute(object? parameter) => _doCanExecute(parameter);
        public void Execute(object? parameter) => _doExecute(parameter);
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        public event EventHandler? CanExecuteChanged;
    }

    public sealed class MainWindowViewModel : INotifyPropertyChanged
    {
        private string _inputPath = string.Empty;
        private int _burstLength = 10;
        private int _burstSize = 5000;
        private int _iterations = 5;
        private bool _openWhenComplete;
        private bool _useTimestampedNames;

        private DelegateCommand _onCorruptPressed;
        private DelegateCommand _onOpenResultPressed;

        private const string DefaultProgressMessage = "Ready!";
        private int _progressAmount;
        private string _progressMessage = DefaultProgressMessage;
        private string _outputPath = "out.mp4";

        public MainWindowViewModel()
        {
            var corruptWorker = new BackgroundWorker();
            corruptWorker.WorkerReportsProgress = true;

            _onCorruptPressed = new DelegateCommand(
                _ => corruptWorker.RunWorkerAsync()
            );

            _onOpenResultPressed = new DelegateCommand(
                _ => File.Exists(_outputPath),
                _ => new Process
                {
                    StartInfo = new ProcessStartInfo(OutputPath)
                    {
                        UseShellExecute = true
                    }
                }.Start()
            );

            corruptWorker.DoWork += DoBackgroundWork;
            corruptWorker.ProgressChanged += (_, args) =>
            {
                ProgressAmount = args.ProgressPercentage;
                if (args.UserState is string state)
                {
                    ProgressMessage = state;
                }
            };

            corruptWorker.RunWorkerCompleted += (_, args) =>
            {
                ProgressAmount = 0;
                ProgressMessage = args.Error is not null ? args.Error.Message : DefaultProgressMessage;
                _onOpenResultPressed.RaiseCanExecuteChanged();
            };
        }

        private void DoBackgroundWork(object? sender, DoWorkEventArgs args)
        {
            if (sender is not BackgroundWorker worker) return;

            worker.ReportProgress(33, "Loading video");
            var video = Video.Load(_inputPath);

            worker.ReportProgress(66, "Corrupting data");
            video.ModifyBytes(Effects.Repeat(_iterations, _burstSize, _burstLength));

            worker.ReportProgress(100, "Saving video");
            video.Save(_outputPath);
        }

        public string InputPath
        {
            get => _inputPath;
            set
            {
                if (value == _inputPath) return;
                _inputPath = value;
                OnPropertyChanged();

                if (!File.Exists(_inputPath)) throw new DataValidationException("File does not exist.");
            }
        }

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

        public int BurstLength
        {
            get => _burstLength;
            set
            {
                if (value == _burstLength) return;
                _burstLength = value;
                OnPropertyChanged();
            }
        }

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

        public bool UseTimestampedNames
        {
            get => _useTimestampedNames;
            set
            {
                if (value == _useTimestampedNames) return;
                _useTimestampedNames = value;
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
                                                     ?? "<unknown>");
        
        public ICommand OnCorruptPressed => _onCorruptPressed;
        public ICommand OnOpenResultPressed => _onOpenResultPressed;

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