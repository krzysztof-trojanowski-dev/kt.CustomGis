using System.Windows;
using Dapper;
using kt.CustomGis.AppMain.Services;
using kt.CustomGis.AppMain.ViewModels;
using kt.CustomGis.Core;
using kt.CustomGis.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;

namespace kt.CustomGis.AppMain;

public partial class App : Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // osm_id -> OsmId, osm_type -> OsmType
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        var builder = Host.CreateApplicationBuilder(e.Args);
        builder.Configuration.AddUserSecrets<App>(optional: true);

        var connectionString = builder.Configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:Postgres");

        builder.Services.AddSingleton(_ => new NpgsqlDataSourceBuilder(connectionString).Build());
        builder.Services.AddTransient<IPoiRepository, PoiRepository>();
        builder.Services.AddTransient<IDialogService, DialogService>();
        builder.Services.AddSingleton<IAppSettings, AppSettings>();
        builder.Services.AddTransient<SettingsWindow>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<MainWindow>();

        _host = builder.Build();
        await _host.StartAsync();

        _host.Services.GetRequiredService<MainWindow>().Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }

        base.OnExit(e);
    }
}
