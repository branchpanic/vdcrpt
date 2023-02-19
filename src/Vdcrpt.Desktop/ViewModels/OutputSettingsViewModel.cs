using CommunityToolkit.Mvvm.ComponentModel;
using Vdcrpt.Desktop.Models;

namespace Vdcrpt.Desktop.ViewModels;

public partial class OutputSettingsViewModel : ViewModelBase
{
    [ObservableProperty] private UserConfig _userConfig;

    public OutputSettingsViewModel(UserConfig userConfig)
    {
        _userConfig = userConfig;
    }

    public OutputSettingsViewModel() : this(new UserConfig())
    {
    }
}