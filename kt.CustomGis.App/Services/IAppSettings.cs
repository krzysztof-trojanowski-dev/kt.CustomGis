using System.ComponentModel;

namespace kt.CustomGis.AppMain.Services;

public interface IAppSettings : INotifyPropertyChanged
{
    int PageSize { get; set; }
}