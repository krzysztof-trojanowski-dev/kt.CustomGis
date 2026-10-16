using System.Windows;
using kt.CustomGis.AppMain.ViewModels;

namespace kt.CustomGis.AppMain;

public partial class SettingsWindow : Window
{
    private readonly SettingsViewModel _viewModel;
    
    public SettingsWindow(SettingsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
        _viewModel.CloseRequested += OnCloseRequested;
        Closed += (_, _) => _viewModel.CloseRequested -= OnCloseRequested;
    }

    private void OnCloseRequested(bool? dialogResult)
    {
        DialogResult = dialogResult;
    }
}