using System.ComponentModel;
using System.Diagnostics;
using kt.CustomGis.AppMain;
using kt.CustomGis.AppMain.Services;
using kt.CustomGis.AppMain.ViewModels;
using kt.CustomGis.Core;
using NSubstitute;

namespace kt.CustomGis.App.Tests.ViewModels;

[TestFixture]
public class MainViewModelTests
{

    private MainViewModel _mainViewModel;

    private IPoiRepository _poiRepositoryFake;
    private IDialogService _dialogServiceFake;
    private IAppSettings _appSettingsFake;

    [SetUp]
    public void SetUp()
    {
        _poiRepositoryFake = Substitute.For<IPoiRepository>();
        _dialogServiceFake = Substitute.For<IDialogService>();
        _appSettingsFake = Substitute.For<IAppSettings>();

        _mainViewModel = new MainViewModel(_poiRepositoryFake,_dialogServiceFake, _appSettingsFake);
    }

    [Test]
    public async Task LoadAsync_WhenExtentsAreNull_UsesGetPoiAsyncAndUpdatesState()
    {
        _poiRepositoryFake.GetPoiAsync(Arg.Any<PageRequest>())
            .Returns(CreatePageResult(1, 50, 2,
            [
                new Poi { OsmType = "node", OsmId = 1, Category = "amenity", SubType = "school", Lat = 51.1, Lon = 17.1 },
                new Poi { OsmType = "way", OsmId = 2, Category = "amenity", SubType = "hospital", Lat = 51.2, Lon = 17.2 },
            ]));
        _appSettingsFake.PageSize.Returns(50);

        await _mainViewModel.LoadAsync();

        await _poiRepositoryFake.Received(1).GetPoiAsync(Arg.Any<PageRequest>());
        await _poiRepositoryFake.DidNotReceive().GetPoiFilteredAsync(Arg.Any<PoiFilteredRequests>());

        Assert.That(_mainViewModel.Items.Count, Is.EqualTo(2));
        Assert.That(_mainViewModel.TotalCount, Is.EqualTo(2));
        Assert.That(_mainViewModel.TotalPages, Is.EqualTo(1));
        Assert.That(_mainViewModel.ErrorMessage, Is.Null);
        Assert.That(_mainViewModel.IsLoading, Is.False);
    }

    [Test]
    public async Task LoadAsync_WhenExtentsAreSet_UsesFilteredQuery()
    {
        _poiRepositoryFake.GetPoiFilteredAsync(Arg.Any<PoiFilteredRequests>())
            .Returns(CreatePageResult(1, 50, 1,
            [
                new Poi { OsmType = "node", OsmId = 10, Category = "shop", SubType = "bakery", Lat = 51.0, Lon = 17.0 },
            ]));
        
        _mainViewModel.MinExtent = new GeoPoint(50.0, 16.0);
        _mainViewModel.MaxExtent = new GeoPoint(52.0, 18.0);

        await _mainViewModel.LoadAsync();

        await _poiRepositoryFake.DidNotReceive().GetPoiAsync(Arg.Any<PageRequest>());
        // ReSharper disable CompareOfFloatsByEqualityOperator
        await _poiRepositoryFake.Received(1).GetPoiFilteredAsync(Arg.Is<PoiFilteredRequests>(x =>
            x.MinLat == 50.0 &&
            x.MinLon == 16.0 &&
            x.MaxLat == 52.0 &&
            x.MaxLon == 18.0));
        // ReSharper restore CompareOfFloatsByEqualityOperator
    }

    [Test]
    public async Task LoadAsync_WhenRepositoryThrows_SetsErrorMessage()
    {
        _poiRepositoryFake.GetPoiAsync(Arg.Any<PageRequest>())
            .Returns(_ => Task.FromException<PagedResult<Poi>>(new InvalidOperationException("db error")));

        await _mainViewModel.LoadAsync();

        Assert.That(_mainViewModel.ErrorMessage, Is.EqualTo("db error"));
        Assert.That(_mainViewModel.IsLoading, Is.False);
    }

    [Test]
    public async Task SelectedPoint_Change_TriggersReload()
    {
        var getPoiCalls = 0;
        _poiRepositoryFake.GetPoiAsync(Arg.Any<PageRequest>())
            .Returns(callInfo =>
            {
                getPoiCalls++;
                var request = callInfo.Arg<PageRequest>();
                return CreatePageResult(request.PageNumber, request.PageSize, 0, []);
            });

        _mainViewModel.SelectedPoint = new GeoPoint(40.0, 15.0);

        await WaitUntilAsync(() => getPoiCalls >= 1);

        Assert.That(getPoiCalls, Is.GreaterThanOrEqualTo(1));
    }

    [Test]
    public async Task PageSize_Change_ResetsPageNumberAndReloads()
    {
        var getPoiCalls = 0;
        _poiRepositoryFake.GetPoiAsync(Arg.Any<PageRequest>())
            .Returns(callInfo =>
            {
                getPoiCalls++;
                var request = callInfo.Arg<PageRequest>();
                return CreatePageResult(request.PageNumber, request.PageSize, 200, []);
            });
        _appSettingsFake.PageSize.Returns(50);

        await _mainViewModel.LoadAsync();
        _mainViewModel.NextPageCommand.Execute(null);
        
        await WaitUntilAsync(() => _mainViewModel.PageNumber == 2);

        _appSettingsFake.PageSize.Returns(20);
        _appSettingsFake.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(
            _appSettingsFake,
            new PropertyChangedEventArgs(nameof(IAppSettings.PageSize)));

        await WaitUntilAsync(() => _mainViewModel.PageNumber == 1 && getPoiCalls >= 3);

        Assert.That(_mainViewModel.PageNumber, Is.EqualTo(1));
        Assert.That(getPoiCalls, Is.GreaterThanOrEqualTo(3));
    }

    private static Task<PagedResult<Poi>> CreatePageResult(int pageNumber, int pageSize, long totalCount, IEnumerable<Poi> items)
    {
        return Task.FromResult(new PagedResult<Poi>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = items,
        });
    }

    private static async Task WaitUntilAsync(Func<bool> condition, int timeoutMs = 1500)
    {
        var sw = Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < timeoutMs)
        {
            if (condition())
            {
                return;
            }

            await Task.Delay(20);
        }

        Assert.Fail("Condition was not met in allotted time.");
    }
}
