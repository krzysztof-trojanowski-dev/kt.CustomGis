using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using kt.CustomGis.AppMain.Mvvm;
using kt.CustomGis.AppMain.ViewModels;
using kt.CustomGis.Core;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.UI.Wpf;

namespace kt.CustomGis.AppMain;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    
    private Point? _clickPoint;
    private readonly MapControl _mapControl;
    private readonly MemoryLayer? _markerLayer;
    private readonly DispatcherTimer _viewportDebounceTimer;

    private const double Tolerance = 0.0001; // Tolerance for floating-point comparison;
    private const double DefaultZoom = 10;
    
    public GeoPoint? GetCenterCoordinates()
    {
        return _viewModel.SelectedPoint;
    }

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewportDebounceTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(300)
        };
        _viewModel = viewModel;
        DataContext = viewModel;
        Loaded += async (_, _) => await _viewModel.LoadAsync();
        
        _viewModel.PropertyChanged += ViewModelOnPropertyChanged;
        _mapControl = new MapControl();
        _mapControl.Map.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());
        _markerLayer = new MemoryLayer()
        {
            Name = "MarkerLayer",
            Style = null,
        };
        _mapControl.Map.Layers.Add(_markerLayer);
        _mapControl.ContextMenu = ContextMenuCreate();
        _mapControl.MouseRightButtonDown += (_, e) =>
        {
            _clickPoint = e.GetPosition(_mapControl);
            _mapControl.ContextMenu.IsOpen = true;
            e.Handled = true;
        };
        _viewportDebounceTimer.Tick += async (_, _) =>
        {
            _viewportDebounceTimer.Stop();
            RecalculateMapExtentAsync();
            await _viewModel.LoadAsync();
        };
        _mapControl.Map.Navigator.ViewportChanged += async (_, _) =>
        {
            await _mapControl.Dispatcher.BeginInvoke(new Action(() =>
            {
                _viewportDebounceTimer.Stop();
                _viewportDebounceTimer.Start();
            }));
        };
        RecalculateMapExtentAsync();
        MapBorder.Child = _mapControl;
        ViewModelOnPropertyChanged(this, new PropertyChangedEventArgs(nameof(MainViewModel.SelectedPoint)));
        MapBorder.InvalidateVisual();
    }

    private void RecalculateMapExtentAsync()
    {
        var extent = _mapControl.Map.Navigator.Viewport.ToExtent();

        var (minLon, minLat) = SphericalMercator.ToLonLat(extent.MinX, extent.MinY);
        var (maxLon, maxLat) = SphericalMercator.ToLonLat(extent.MaxX, extent.MaxY);

        _viewModel.MinExtent = new GeoPoint(minLat, minLon);
        _viewModel.MaxExtent = new GeoPoint(maxLat, maxLon);
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if(e.PropertyName == nameof(MainViewModel.SelectedPoint))
        {
            var center = SphericalMercator.FromLonLat(
                _viewModel.SelectedPoint.Longitude, // lon    
                _viewModel.SelectedPoint.Latitude   // lat
            );
            
            if (Math.Abs(_mapControl.Map.Navigator.Viewport.CenterX - center.x) > Tolerance ||
                Math.Abs(_mapControl.Map.Navigator.Viewport.CenterY - center.y) > Tolerance)
            {
                if (_mapControl.Map.Navigator.Viewport.Resolution >= DefaultZoom)
                {
                    _mapControl.Map.Navigator.CenterOn(new MPoint(center));
                }
                else
                {
                    _mapControl.Map.Navigator.CenterOnAndZoomTo(new MPoint(center), DefaultZoom);
                }
                    
            }
            
            ShowMarker(_viewModel.SelectedPoint);
        }
    }

    private ContextMenu ContextMenuCreate()
    {
        var menu = new ContextMenu();
        var item = new MenuItem()
        {
            Header = "Create POI",
            Command = new AsyncRelayCommand(async () =>
            {
                if (_clickPoint is not { } p) 
                    return;

                var world = _mapControl.Map.Navigator.Viewport.ScreenToWorld(p.X, p.Y);

                var (lon, lat) = SphericalMercator.ToLonLat(world.X, world.Y);
                _viewModel.SelectedPoint = new GeoPoint(lat, lon);
            })
        };
        menu.Items.Add(item);
        return menu;
    }
    
    private void ShowMarker(GeoPoint point)
    {
        var (x,y) = SphericalMercator.FromLonLat(point.Longitude, point.Latitude);
        var feature = new PointFeature(new MPoint(x, y));
        feature.Styles.Add(new SymbolStyle()
        {
            SymbolType = SymbolType.Triangle,
            Fill = new Brush(Color.Red),
            Outline = new Pen(Color.Black, 2),
            Opacity = 0.7f,
            SymbolScale = 0.4,
        });
        
        _markerLayer?.Features = [feature];
        _markerLayer?.DataHasChanged();
    }

    private void ShowPoints(IList<Poi> poiItems)
    {
        var features = new List<PointFeature>();
        foreach (var poi in poiItems)
        {
            var (x, y) = SphericalMercator.FromLonLat(poi.Lon, poi.Lat);
            var feature = new PointFeature(new MPoint(x, y));
            feature.Styles.Add(new SymbolStyle()
            {
                SymbolType = SymbolType.Triangle,
                Fill = new Brush(Color.Red),
                Outline = new Pen(Color.Black, 2),
                Opacity = 0.7f,
                SymbolScale = 0.4,
            });
            features.Add(feature);
        }

        _markerLayer?.Features = features;
        _markerLayer?.DataHasChanged();
    }

    private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var poiItems = (e.Source as DataGrid)?.SelectedItems.Cast<Poi>().ToList();
        if (poiItems != null)
        {
            ShowPoints(poiItems);
        }
    }
}