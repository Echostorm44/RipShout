using NAudio.Wave;
using RipShout.Helpers;
using RipShout.Services;
using RipShout.ViewModels;
using RipShout.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
                await Dispatcher.InvokeAsync(() =>
                {
                    RootWelcomeGrid.Visibility = Visibility.Hidden;
                    RootMainGrid.Visibility = Visibility.Visible;
                    RootNavigation.Navigate(typeof(StationsPage));
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

    public Frame GetFrame()
    {
        return RootFrame;
    }

    public INavigation GetNavigation()
    {
        return RootNavigation;
    }

    public bool Navigate(Type pageType)
    {
        return RootNavigation.Navigate(pageType);
    }

    public void SetPageService(IPageService pageService)
    {
        RootNavigation.PageService = pageService;
    }

    public void ShowWindow()
    {
        Show();
    }

    public void CloseWindow()
    {
        Close();
    }

    private void UiWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
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
