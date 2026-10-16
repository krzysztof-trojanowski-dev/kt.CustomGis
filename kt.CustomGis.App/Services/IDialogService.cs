using kt.CustomGis.AppMain.ViewModels;

namespace kt.CustomGis.AppMain.Services;

public interface IDialogService
{
    Task<bool> ShowSettingsDialogAsync();
}