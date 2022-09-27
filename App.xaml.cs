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
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
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

    public App()
    {
        MyRadio = new RadioService();
        MySettings = SettingsIoHelpers.LoadGeneralSettingsFromDisk();
        MySettings.ValueChanged += MySettings_ValueChanged;
    }

    private void MySettings_ValueChanged(object source)
    {
        SettingsIoHelpers.SaveGeneralSettingsToDisk((SettingsModel)source);
    }

    private static readonly IHost _host = Host
    .CreateDefaultBuilder()
    .ConfigureAppConfiguration(c =>
    {
        c.SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location));
    })
    .ConfigureServices((context, services) =>
    {
        // App Host
        services.AddHostedService<ApplicationHostService>();

        // Theme manipulation
        services.AddSingleton<IThemeService, ThemeService>();

        services.AddSingleton<ISnackbarService, SnackbarService>();
        // Tray icon
        //services.AddSingleton<INotifyIconService, CustomNotifyIconService>();

        // Page resolver service
        services.AddSingleton<IPageService, PageService>();
        // Service containing navigation, same as INavigationWindow... but without window
        services.AddSingleton<INavigationService, NavigationService>();

        services.AddScoped<INavigationWindow, MainWindow>();
        services.AddScoped<MainViewModel>();

        services.AddScoped<Views.StationsPage>();
        services.AddScoped<StationsViewModel>();

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
