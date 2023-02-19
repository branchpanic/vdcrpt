using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using Vdcrpt.Desktop.Models;

namespace Vdcrpt.Desktop.ViewModels;

public partial class OutputSettingsViewModel : ViewModelBase
{
    [ObservableProperty] UserConfig _userConfig;

    public OutputSettingsViewModel(UserConfig userConfig)
    {
        _userConfig = userConfig;
    }

    public OutputSettingsViewModel() : this(new UserConfig()) { }
}
