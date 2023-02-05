namespace Vdcrpt.Desktop.ViewModels;

public class OutputSettingsViewModel : ViewModelBase
{
    private bool _openWhenComplete = false;
    private bool _askForFilename = true;

    public bool OpenWhenComplete
    {
        get => _openWhenComplete;
        set => RaiseAndSetIfChanged(ref _openWhenComplete, value);
    }

    public bool AskForFilename
    {
        get => _askForFilename;
        set => RaiseAndSetIfChanged(ref _askForFilename, value);
    }
}
