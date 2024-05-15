using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RipShout.Helpers;
using RipShout.Models;
using RipShout.Services;
using RipShout.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;

namespace RipShout;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static RadioService? MyRadio { get; set; }
    public static SettingsModel MySettings { get; set; }
    public static ObservableCollection<ChannelModel> CachedChannelList { get; set; }
    public static (string listenKey, bool getDI, bool getRt, bool getJazz, bool getRock, bool getZen, bool getClassical, bool getOneFm) CachedChannelListConfig { get; set; }
    static SemaphoreSlim fetchSemi = new SemaphoreSlim(1, 1);
    Debouncer bounce = new Debouncer(2000);

    public App()
    {
        MySettings = SettingsIoHelpers.LoadGeneralSettingsFromDisk();
        MyRadio = new RadioService();
        MySettings.ValueChanged += MySettings_ValueChanged;
        CachedChannelList = new ObservableCollection<ChannelModel>();
        CachedChannelListConfig = (MySettings.AudioAddictListenKey, MySettings.ShowDiChannels, MySettings.ShowRadioTunesChannels,
            MySettings.ShowJazzRadioChannels, MySettings.ShowRockRadioChannels, MySettings.ShowZenRadioChannels, MySettings.ShowClassicalRadioChannels, MySettings.ShowOneFmChannels);
    }

    public static async Task<bool> LoadChannels()
    {
        CachedChannelList.Clear();
        if (CachedChannelList.Count != 0 && App.CachedChannelListConfig == (App.MySettings.AudioAddictListenKey, App.MySettings.ShowDiChannels, App.MySettings.ShowRadioTunesChannels,
                        App.MySettings.ShowJazzRadioChannels, App.MySettings.ShowRockRadioChannels, App.MySettings.ShowZenRadioChannels, App.MySettings.ShowClassicalRadioChannels, App.MySettings.ShowOneFmChannels))
        {
            // Nothing to do, bail
            return false;
        }
        else
        {
            await fetchSemi.WaitAsync();
            try
            {
                var channelList = new List<ChannelModel>();
                var tempAaChans = await AudioAddictChannelServices.AudioAddictGetChannelsService.GetChannelsAsync(App.MySettings.AudioAddictListenKey, App.MySettings.ShowDiChannels, App.MySettings.ShowRadioTunesChannels,
                App.MySettings.ShowJazzRadioChannels, App.MySettings.ShowRockRadioChannels, App.MySettings.ShowZenRadioChannels, App.MySettings.ShowClassicalRadioChannels, App.MySettings.FavoriteIDs);
                foreach (var aaChan in tempAaChans)
                {
                    aaChan.IsFavorite = MySettings.FavoriteIDs.Contains(aaChan.ID);
                    channelList.Add(aaChan);
                }
                // Add oneFm to chans
                if (MySettings.ShowOneFmChannels)
                {
                    var oneFMChans = await OneFmChannelServices.OneFmGetStationsService.GetChannelsAsync(App.MySettings.FavoriteIDs);
                    if (oneFMChans != null)
                    {
                        foreach (var oneChan in oneFMChans)
                        {
                            oneChan.IsFavorite = MySettings.FavoriteIDs.Contains(oneChan.ID);
                            channelList.Add(oneChan);
                        }
                    }
                }
                foreach (var item in channelList.OrderByDescending(a => a.IsFavorite).ThenByDescending(a => a.Family))
                {
                    CachedChannelList.Add(item);
                }
                App.CachedChannelListConfig = (App.MySettings.AudioAddictListenKey, App.MySettings.ShowDiChannels, App.MySettings.ShowRadioTunesChannels,
                    App.MySettings.ShowJazzRadioChannels, App.MySettings.ShowRockRadioChannels, App.MySettings.ShowZenRadioChannels, App.MySettings.ShowClassicalRadioChannels, App.MySettings.ShowOneFmChannels);
            }
            finally
            {
                fetchSemi.Release();
            }
        }
        return true;
    }

    public void MySettings_ValueChanged(object source)
    {
        bounce.Debounce(async () =>
        {
            SettingsIoHelpers.SaveGeneralSettingsToDisk((SettingsModel)source);
            await ForceRefreshChansFromWebAsync();
        });
    }

    public static async Task ForceRefreshChansFromWebAsync()
    {
        App.CachedChannelList.Clear();
        await LoadChannels();
    }

    private static readonly IHost _host = Host
    .CreateDefaultBuilder()
    .ConfigureAppConfiguration(c =>
    {
        c.SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location));
    })
    .ConfigureServices((context, services) =>
    {
        services.AddHostedService<ApplicationHostService>();
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<ISnackbarService, SnackbarService>();
        services.AddSingleton<IPageService, PageService>();
        services.AddSingleton<INavigationService, NavigationService>();

        services.AddScoped<INavigationWindow, MainWindow>();
        services.AddScoped<MainViewModel>();
        services.AddScoped<Views.StationsPage>();
        services.AddScoped<Views.NowPlayingPage>();
        services.AddScoped<NowPlayingViewModel>();
        services.AddScoped<Views.SettingsPage>();
        services.AddScoped<SettingsViewModel>();

        // Configuration
        services.Configure<AppConfig>(context.Configuration.GetSection(nameof(AppConfig)));
    }).Build();

    /// <summary>
    /// Gets registered service.
    /// </summary>
    /// <typeparam name="T">Type of the service to get.</typeparam>
    /// <returns>Instance of the service or <see langword="null"/>.</returns>
    public static T GetService<T>()
        where T : class
    {
        return _host.Services.GetService(typeof(T)) as T;
    }

    /// <summary>
    /// Occurs when the application is loading.
    /// </summary>
    private async void OnStartup(object sender, StartupEventArgs e)
    {
        await _host.StartAsync();
    }

    /// <summary>
    /// Occurs when the application is closing.
    /// </summary>
    private async void OnExit(object sender, ExitEventArgs e)
    {
        MyRadio.Dispose();

        foreach (var doomedFolder in Directory.EnumerateDirectories(MySettings.SaveTempMusicToFolder))
        {
            try
            {// TODO this needs a beat for the stream to close || it won't be able to access
                Directory.Delete(doomedFolder, true);
            }
            catch (Exception ex)
            {
                GeneralHelpers.WriteLogEntry(ex.ToString(), GeneralHelpers.LogFileType.Exception);
            }
        }
        await _host.StopAsync();

        _host.Dispose();
    }

    /// <summary>
    /// Occurs when an exception is thrown by an application but not handled.
    /// </summary>
    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        // For more info see https://docs.microsoft.com/en-us/dotnet/api/system.windows.application.dispatcherunhandledexception?view=windowsdesktop-6.0
    }
}
