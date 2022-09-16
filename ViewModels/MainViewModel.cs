using CommunityToolkit.Mvvm.Messaging;
using RipShout.Models;
using RipShout.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;

namespace RipShout.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private bool isInitialized = false;

    string applicationTitle;
    public string ApplicationTitle
    {
        get => applicationTitle;
        set
        {
            if (applicationTitle == value)
            {
                return;
            }

            applicationTitle = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ApplicationTitle)));
        }
    }

    ObservableCollection<INavigationControl> navigationItems;
    public ObservableCollection<INavigationControl> NavigationItems
    {
        get => navigationItems;
        set
        {
            if (navigationItems == value)
            {
                return;
            }

            navigationItems = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NavigationItems)));
        }
    }

    ObservableCollection<INavigationControl> navigationFooter;
    public ObservableCollection<INavigationControl> NavigationFooter
    {
        get => navigationFooter;
        set
        {
            if (navigationFooter == value)
            {
                return;
            }

            navigationFooter = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NavigationFooter)));
        }
    }

    ObservableCollection<MenuItem> trayMenuItems;

    public MainViewModel()
    {//Wpf.Ui.Appearance.Theme.Apply(Wpf.Ui.Appearance.ThemeType.Light);
        if (!isInitialized)
        {
            InitializeViewModel();
            
        }
    }
    
    public ObservableCollection<MenuItem> TrayMenuItems
    {
        get => trayMenuItems;
        set
        {
            if (trayMenuItems == value)
            {
                return;
            }

            trayMenuItems = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TrayMenuItems)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;


    private void InitializeViewModel()
    {
        
        ApplicationTitle = "RipShout";
        NavigationItems = new ObservableCollection<INavigationControl>
        {
            new NavigationItem()
            {
                Content = "Now Playing",
                PageTag = "playing",
                Icon = SymbolRegular.Play12,
                PageType = typeof(NowPlayingPage)
            },
            new NavigationItem()
            {
                Content = "Stations",
                PageTag = "stations",
                Icon = SymbolRegular.MusicNote120,
                PageType = typeof(StationsPage)
            }
        };

        NavigationFooter = new ObservableCollection<INavigationControl>
        {
            new NavigationItem()
            {
                Content = "Settings",
                PageTag = "settings",
                Icon = SymbolRegular.Settings24,
                PageType = typeof(NowPlayingPage)
            }
        };

        TrayMenuItems = new ObservableCollection<MenuItem>
        {
            new MenuItem
            {
                Header = "Now Playing",
                Tag = "playing"                    
            },
            new MenuItem
            {
                Header = "Stations",
                Tag = "stations"
            }

        };

        isInitialized = true;
    }
}
