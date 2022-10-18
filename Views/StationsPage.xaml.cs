using RipShout.Helpers;
using RipShout.Models;
using RipShout.Services;
using RipShout.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
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
using Wpf.Ui.Common;
using Wpf.Ui.Common.Interfaces;
using Wpf.Ui.Controls;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;

namespace RipShout.Views;

/// <summary>
/// Interaction logic for StationsPage.xaml
/// </summary>
public partial class StationsPage
{
    private readonly INavigationService _navigationService;

    public StationsPage(INavigationService navigationService)
    {
        InitializeComponent();
        //chanPanel.Items.SortDescriptions.Add(new SortDescription("IsFavorite", ListSortDirection.Descending));
        //chanPanel.Items.SortDescriptions.Add(new SortDescription("Family", ListSortDirection.Descending));
        _navigationService = navigationService;
    }

    private void btnPlayEnteredURL_Click(object sender, RoutedEventArgs e)
    {
        var url = txtStationURL.Text?.Trim();
        if(string.IsNullOrEmpty(url))
        {
            return;
        }
        App.MyRadio.StartStreamFromURL(url, null);
        _navigationService.Navigate(typeof(Views.NowPlayingPage));
    }

    private async void UiPage_Loaded(object sender, RoutedEventArgs e)
    {
        //await ViewModel.LoadChannelsPlease();
    }

    private void ToggleSwitch_Checked(object sender, RoutedEventArgs e)
    {
        var foo = (ToggleSwitch)e.Source;
        ToggleStationVisiblity(foo, true);
    }

    private void ToggleStationVisiblity(ToggleSwitch foo, bool isVisible)
    {
        var stationFam = Models.StationFamily.None;
        switch(foo.Tag)
        {
            case "DI":
            {
                stationFam = Models.StationFamily.DI;
            }
                break;
            case "RT":
            {
                stationFam = Models.StationFamily.RadioTunes;
            }
                break;
            case "ZEN":
            {
                stationFam = Models.StationFamily.ZenRadio;
            }
                break;
            case "JAZZ":
            {
                stationFam = Models.StationFamily.JazzRadio;
            }
                break;
            case "ROCK":
            {
                stationFam = Models.StationFamily.RockRadio;
            }
                break;
            case "CLASSICAL":
            {
                stationFam = Models.StationFamily.ClassicalRadio;
            }
                break;
            case "1FM":
            {
                stationFam = Models.StationFamily.OneFM;
            }
                break;
        }

        foreach(var item in App.CachedChannelList)
        {
            if(item.Family == stationFam && !item.IsFavorite)
            {
                item.IsVisible = isVisible;
            }
        }
        if(chanPanel != null)
        {
            //chanPanel.Items.Refresh();
        }
    }

    private void ToggleSwitch_Unchecked(object sender, RoutedEventArgs e)
    {
        var foo = (ToggleSwitch)e.Source;
        ToggleStationVisiblity(foo, false);
    }

    private void PlayChannel(object sender, RoutedEventArgs e)
    {
        try
        {
            var choice = e.Source;
            if(choice == null || e.Source.GetType() != typeof(System.Windows.Controls.Button))
            {
                return;
            }
            if(((System.Windows.Controls.Button)choice).DataContext.GetType() != typeof(ChannelModel) || ((System.Windows.Controls.Button)choice).CommandParameter?.ToString() != "Play")
            {
                return;
            }
            var model = ((ChannelModel)((System.Windows.Controls.Button)choice).DataContext);
            var url = model.PrimaryURL;
            var backupUrl = model.PrimaryURL;
            App.MyRadio.StartStreamFromURL(url, backupUrl);
            _navigationService.Navigate(typeof(Views.NowPlayingPage));
        }
        catch(Exception ex)
        {
            GeneralHelpers.WriteLogEntry(ex.ToString(), GeneralHelpers.LogFileType.Exception);
        }
    }

    private void ToggleFavorite(object sender, RoutedEventArgs e)
    {
        try
        {
            var choice = e.Source;
            if(choice == null || ((System.Windows.Controls.Button)choice).CommandParameter?.ToString() != "Fav")
            {
                return;
            }
            var model = ((ChannelModel)((System.Windows.Controls.Button)choice).DataContext);

            var id = model.ID;
            var prevState = model.IsFavorite;
            model.IsFavorite = !prevState;
            foreach(var chan in App.CachedChannelList)
            {
                if(chan.ID == model.ID)
                {
                    chan.IsFavorite = model.IsFavorite;
                }
            }
            if(!prevState && !App.MySettings.FavoriteIDs.Contains(id))
            {
                App.MySettings.FavoriteIDs.Add(id);
            }
            else if(prevState && App.MySettings.FavoriteIDs.Contains(id))
            {
                App.MySettings.FavoriteIDs.Remove(id);
            }
            App.MySettings.SaveToFile();

            var currentIndex = App.CachedChannelList.IndexOf(model);
            int newIndex = 0;
            if(prevState == true)
            {// It was a fav but now it isn't so we need to put it back with its friends
                var query = App.CachedChannelList.OrderByDescending(a => a.IsFavorite).ThenByDescending(a => a.Family).ToList();
                newIndex = query.IndexOf(model);
            }
            App.CachedChannelList.Move(currentIndex, newIndex);
        }
        catch(Exception ex)
        {
            GeneralHelpers.WriteLogEntry(ex.ToString(), GeneralHelpers.LogFileType.Exception);
        }
    }
}
