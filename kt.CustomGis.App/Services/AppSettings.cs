using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace kt.CustomGis.AppMain.Services;

public class AppSettings : IAppSettings
{
    public event PropertyChangedEventHandler? PropertyChanged;
    
    private int _pageSize = 50;
    
    public int PageSize
    {
        get => _pageSize;
        set
        {
            if (_pageSize != value)
            {
                _pageSize = value;
                OnPropertyChanged();
            }
        }
    }

    private int _searchRange = 10000;

    public int SearchRange
    {
        get => _searchRange;
        set
        {
            if (_searchRange != value)
            {
                _searchRange = value;
                OnPropertyChanged();
            }
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null!)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}