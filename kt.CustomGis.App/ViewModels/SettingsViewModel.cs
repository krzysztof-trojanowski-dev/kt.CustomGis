using kt.CustomGis.AppMain.Mvvm;
using kt.CustomGis.AppMain.Services;

namespace kt.CustomGis.AppMain.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private readonly IAppSettings _appSettings;
    
    public SettingsViewModel(IAppSettings appSettings)
    {
        _appSettings = appSettings;
        SelectedPageSize = _appSettings.PageSize;
        SaveCommand = new AsyncRelayCommand(SaveAsync);
        CloseCommand = new AsyncRelayCommand(CloseAsync);
    }
    
    public AsyncRelayCommand SaveCommand { get; }
    public AsyncRelayCommand CloseCommand { get; }
    public int[] AvailablePageSizes => [10, 20, 50, 100];
    public int SelectedPageSize { get; set; }

    public event Action<bool?>? CloseRequested;

    private Task SaveAsync()
    {
        CloseRequested?.Invoke(true);
        _appSettings.PageSize = SelectedPageSize;
        return Task.CompletedTask;
    }

    private Task CloseAsync()
    {
        CloseRequested?.Invoke(false);
        return Task.CompletedTask;
    }
}