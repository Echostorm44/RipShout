using RipShout.ViewModels;
using RipShout.Views;
using System;
using System.Collections.Generic;
using System.Linq;
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
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;
using Wpf.Ui.TaskBar;

namespace RipShout;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : INavigationWindow
{
    //<a href="https://www.flaticon.com/free-icons/radio" title="radio icons">Radio icons created by Freepik - Flaticon</a>
    public ViewModels.MainViewModel ViewModel { get; set; }
    private bool initialized = false;
    private readonly IThemeService themeService;


    public MainWindow(MainViewModel viewModel, INavigationService navigationService, 
        IPageService pageService, IThemeService ts, ISnackbarService snackbarService)
    {
        themeService = ts;
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
        navigationService.SetNavigationControl(RootNavigation);
        SetPageService(pageService);
        snackbarService.SetSnackbarControl(RootSnackbar);
        if(App.MySettings.LastWindowWidth == 0)
        {
            var win = GetWindowResolutionAndLocation();
            App.MySettings.LastWindowHeight = win.height;
            App.MySettings.LastWindowWidth = win.width;
            App.MySettings.LastWindowX = win.x >= 0 ? win.x : 0;
            App.MySettings.LastWindowY = win.y >= 0 ? win.y : 0;
            App.MySettings.SaveToFile();
        }
        else
        {
            this.Height = App.MySettings.LastWindowHeight;// Might need to move this to after loaded
            this.Width = App.MySettings.LastWindowWidth;
        }
    }

    private void InvokeSplashScreen()
    {
        if(initialized)
        {
            return;
        }
        initialized = true;

        // Location has to happen outside constructor
        if (App.MySettings.LastWindowY >= 0 && App.MySettings.LastWindowX >= 0)
        {
            this.Left = App.MySettings.LastWindowX;
            this.Top = App.MySettings.LastWindowY;
        }

        RootMainGrid.Visibility = Visibility.Collapsed;
        RootWelcomeGrid.Visibility = Visibility.Visible;

        Task.Run(async () =>
        {
            // Remember to always include Delays and Sleeps in
            // your applications to be able to charge the client for optimizations later. ;)
            await Task.Delay(1000);

            await Dispatcher.InvokeAsync(() =>
            {
                RootWelcomeGrid.Visibility = Visibility.Hidden;
                RootMainGrid.Visibility = Visibility.Visible;
                RootNavigation.Navigate(typeof(StationsPage));
            });

            return true;
        });
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
        // Remember where the window was for next time.
        var win = GetWindowResolutionAndLocation();
        App.MySettings.LastWindowHeight = win.height;
        App.MySettings.LastWindowWidth = win.width;
        App.MySettings.LastWindowX = win.x;
        App.MySettings.LastWindowY = win.y;
        App.MySettings.SaveToFile();
    }

    public (int height, int width, int x, int y) GetWindowResolutionAndLocation()
    {
        var point = new System.Drawing.Point((int)this.Left, (int)this.Top);
        var height = (int)this.ActualHeight;
        var width = (int)this.ActualWidth;
        var x = point.X;
        var y = point.Y;
        return (height, width, x, y);
    }

    private void UiWindow_Loaded(object sender, RoutedEventArgs e)
    {
        InvokeSplashScreen();
    }

    //private void Button_Click(object sender, RoutedEventArgs e)
    //{
    //    themeService.SetTheme(themeService.GetTheme() == ThemeType.Dark ? ThemeType.Light : ThemeType.Dark);
    //}
}
