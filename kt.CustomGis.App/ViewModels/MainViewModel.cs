using System.Collections.ObjectModel;
using System.ComponentModel;
using kt.CustomGis.Core;
using kt.CustomGis.AppMain.Mvvm;
using kt.CustomGis.AppMain.Services;

namespace kt.CustomGis.AppMain.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly IPoiRepository _poiRepository;
    private readonly IDialogService _dialogService;
    private readonly IAppSettings _appSettings;

    private int _pageNumber = 1;
    private int _totalPages;
    private long _totalCount;
    private bool _isLoading;
    private string? _errorMessage;
    private GeoPoint _selectedPoint;
    private GeoPoint? _minExtent;
    private GeoPoint? _maxExtent;

    public MainViewModel(IPoiRepository poiRepository, IDialogService dialogService, IAppSettings appSettings)
    {
        _poiRepository = poiRepository;
        _dialogService = dialogService;
        _appSettings = appSettings;
        _appSettings.PropertyChanged += async (s, e) => await OnPageSizeChangedAsync(e);
        PreviousPageCommand = new AsyncRelayCommand(PreviousPageAsync, () => !IsLoading && PageNumber > 1);
        NextPageCommand = new AsyncRelayCommand(NextPageAsync, () => !IsLoading && PageNumber < TotalPages);
        OpenSettingsCommand = new AsyncRelayCommand(OpenSettingsAsync, () => !IsLoading);
        _selectedPoint = new GeoPoint(51.08731249663713, 17.196244296656115);
        PropertyChanged += async (s, e) => await OnPropertyChanged(s, e);
    }

    

    public ObservableCollection<Poi> Items { get; } = new();

    public AsyncRelayCommand PreviousPageCommand { get; }
    public AsyncRelayCommand NextPageCommand { get; }
    public AsyncRelayCommand OpenSettingsCommand { get; }
    
    private int PageSize => _appSettings.PageSize;
    
    public GeoPoint SelectedPoint
    {
        get => _selectedPoint;
        set => SetProperty(ref _selectedPoint, value);
    }
    
    public GeoPoint? MinExtent
    {
        get => _minExtent;
        set => SetProperty(ref _minExtent, value);
    }
    
    public GeoPoint? MaxExtent
    {
        get => _maxExtent;
        set => SetProperty(ref _maxExtent, value);
    }

    public int PageNumber
    {
        get => _pageNumber;
        private set
        {
            if (SetProperty(ref _pageNumber, value))
            {
                RaiseCommandsCanExecuteChanged();
            }
        }
    }

    public int TotalPages
    {
        get => _totalPages;
        private set
        {
            if (SetProperty(ref _totalPages, value))
            {
                RaiseCommandsCanExecuteChanged();
            }
        }
    }

    public long TotalCount
    {
        get => _totalCount;
        private set => SetProperty(ref _totalCount, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (SetProperty(ref _isLoading, value))
            {
                RaiseCommandsCanExecuteChanged();
            }
        }
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }
    
    public async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            var page =
                (_minExtent == null || _maxExtent == null)
                    ? await _poiRepository.GetPoiAsync(new PageRequest()
                        { PageNumber = PageNumber, PageSize = PageSize })
                    : await _poiRepository.GetPoiFilteredAsync(new PoiFilteredRequests()
                    {
                        PageNumber = PageNumber,
                        PageSize = PageSize,
                        MinLat = _minExtent.Latitude,
                        MinLon = _minExtent.Longitude,
                        MaxLat = _maxExtent.Latitude,
                        MaxLon = _maxExtent.Longitude,
                    });

            Items.Clear();
            foreach (var item in page.Items)
            {
                Items.Add(item);
            }

            TotalCount = page.TotalCount;
            TotalPages = page.TotalPages;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task PreviousPageAsync()
    {
        PageNumber--;
        await LoadAsync();
    }

    private async Task NextPageAsync()
    {
        PageNumber++;
        await LoadAsync();
    }
    
    private async Task OpenSettingsAsync()
    {
        await _dialogService.ShowSettingsDialogAsync();
    }

    private void RaiseCommandsCanExecuteChanged()
    {
        PreviousPageCommand.RaiseCanExecuteChanged();
        NextPageCommand.RaiseCanExecuteChanged();
        OpenSettingsCommand.RaiseCanExecuteChanged();
    }
    
    private Task OnPageSizeChangedAsync(PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IAppSettings.PageSize))
        {
            PageNumber = 1;
            return LoadAsync();
        }

        return Task.CompletedTask;
    }
    
    private async Task OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SelectedPoint))
        {
            await LoadAsync();
        }
    }
}

