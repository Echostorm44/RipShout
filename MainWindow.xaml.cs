using RipShout.Helpers;
using RipShout.Services;
using RipShout.ViewModels;
using RipShout.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Interop.WinDef;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;
using Wpf.Ui.TaskBar;

namespace RipShout;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : INavigationWindow
{
    public ViewModels.MainViewModel ViewModel { get; set; }
    private bool initialized = false;
    string updatePath = "";


    public MainWindow(MainViewModel viewModel, INavigationService navigationService, 
        IPageService pageService, ISnackbarService snackbarService)
    {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
        navigationService.SetNavigationControl(RootNavigation);
        SetPageService(pageService);
        snackbarService.SetSnackbarControl(RootSnackbar);
    }

    private void InvokeSplashScreen()
    {
        if(initialized)
        {
            return;
        }
        initialized = true;

        // Location has to happen outside constructor
        //if(App.MySettings.LastWindowY >= 0 && App.MySettings.LastWindowX >= 0)
        //{
        //    this.Left = App.MySettings.LastWindowX;
        //    this.Top = App.MySettings.LastWindowY;
        //}

        RootMainGrid.Visibility = Visibility.Collapsed;
        RootWelcomeGrid.Visibility = Visibility.Visible;
        Wpf.Ui.TaskBar.TaskBarProgress.SetValue(this, Wpf.Ui.TaskBar.TaskBarProgressState.Indeterminate, 0);

        Task.Run(async () =>
        {
            // Remember to always include Delays and Sleeps in
            // your applications to be able to charge the client for optimizations later. ;)
            try
            {
                await App.LoadChannels();
                await CheckForUpdatesAsync();
                await Dispatcher.InvokeAsync(() =>
                {
                    RootWelcomeGrid.Visibility = Visibility.Hidden;
                    RootMainGrid.Visibility = Visibility.Visible;
                    RootNavigation.Navigate(typeof(StationsPage));
                    if(!string.IsNullOrEmpty(updatePath))
                    {
                        var mb = new Wpf.Ui.Controls.MessageBox();
                        mb.ButtonLeftAppearance = Wpf.Ui.Common.ControlAppearance.Secondary;
                        mb.ButtonLeftName = "Close & Update Now";
                        mb.ButtonRightName = "Later";
                        mb.ButtonLeftClick += UpdateNowNowNow;
                        mb.ButtonRightClick += UpdateLater;
                        mb.Width = 500;
                        mb.Show("Update Found!!", "Happy Day!!\r\nThere is an update ready to install after you close Ripshout!\r\nWanna do it now?");
                    }
                    Wpf.Ui.TaskBar.TaskBarProgress.SetValue(this, Wpf.Ui.TaskBar.TaskBarProgressState.None, 0);
                });
            }
            catch(Exception ex)
            {
                GeneralHelpers.WriteLogEntry(ex.ToString(), GeneralHelpers.LogFileType.Exception);
            }

            return true;
        });
    }

    private async Task<bool> CheckForUpdatesAsync()
    {
        try
        {
            var assemblyPath = Assembly.GetExecutingAssembly().Location;
            var root = System.IO.Path.GetDirectoryName(assemblyPath);
            var finalFileName = root + "\\Version.txt";
            var downloadFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Ripshout\\UpdateDownload\\";
            if(!Directory.Exists(downloadFolder))
            {
                Directory.CreateDirectory(downloadFolder);
            }
            var localVersion = "";
            using(TextReader tr = new StreamReader(finalFileName))
            {
                if(tr.Peek() > 0)
                {
                    localVersion = tr.ReadToEnd();
                }
            }

            using(HttpClient client = new HttpClient())
            {
                var result = await client.GetAsync("https://raw.githubusercontent.com/Echostorm44/RipShout/main/Version.txt");
                if(result.IsSuccessStatusCode)
                {
                    var serverText = await result.Content.ReadAsStringAsync();
                    var splitServerText = serverText.Split("|");
                    if(serverText != localVersion)
                    {
                        var updateURL = splitServerText[1];
                        using var s = await client.GetStreamAsync(updateURL);
                        updatePath = downloadFolder + "update.msi";
                        using var fs = new FileStream(updatePath, FileMode.OpenOrCreate);
                        await s.CopyToAsync(fs);
                    }
                }
            }
        }
        catch(Exception ex)
        {
            GeneralHelpers.WriteLogEntry(ex.ToString(), GeneralHelpers.LogFileType.Exception);
        }
        return true;
    }

    private void UpdateNowNowNow(object sender, RoutedEventArgs e)
    {
        this.Close();
        (sender as Wpf.Ui.Controls.MessageBox)?.Close();
    }

    private void UpdateLater(object sender, RoutedEventArgs e)
    {
        (sender as Wpf.Ui.Controls.MessageBox)?.Close();
    }

    public Frame GetFrame()
        => RootFrame;

    public INavigation GetNavigation()
        => RootNavigation;

    public bool Navigate(Type pageType)
        => RootNavigation.Navigate(pageType);

    public void SetPageService(IPageService pageService)
        => RootNavigation.PageService = pageService;

    public void ShowWindow()
        => Show();

    public void CloseWindow()
        => Close();

    private void UiWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if(!string.IsNullOrEmpty(updatePath))
        {
            var myProcess = new System.Diagnostics.Process();
            myProcess.StartInfo.UseShellExecute = true;
            myProcess.StartInfo.FileName = updatePath;
            myProcess.Start();
        }
        this.SavePlacement();
    }

    private void UiWindow_Loaded(object sender, RoutedEventArgs e)
    {
        InvokeSplashScreen();
    }

    private void UiWindow_SourceInitialized(object sender, EventArgs e)
    {
        this.ApplyPlacement();
    }
}
