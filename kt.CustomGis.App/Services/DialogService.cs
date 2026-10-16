using System.Windows;
using kt.CustomGis.AppMain.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace kt.CustomGis.AppMain.Services;

public class DialogService : IDialogService
{
    private readonly IServiceProvider _serviceProvider;

    public DialogService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public Task<bool> ShowSettingsDialogAsync()
    {
        var dialog = _serviceProvider.GetRequiredService<SettingsWindow>();
        if (Application.Current.MainWindow is { } owner)
        {
            dialog.Owner = owner;
        }

        return Task.FromResult<bool>(dialog.ShowDialog() == true);
    }
}