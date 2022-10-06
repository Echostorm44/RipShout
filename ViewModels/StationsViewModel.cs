using RipShout.Helpers;
using RipShout.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Wpf.Ui.Common.Interfaces;
using Wpf.Ui.Mvvm.Contracts;

namespace RipShout.ViewModels;

public class StationsViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public ObservableCollection<ChannelModel> Channels { get; set; }
    public SettingsModel Settings { get; set; }
    public ICommand PlayChannelCommand { get; private set; }
    public ICommand FavoriteChannelCommand { get; private set; }
    bool StartingUp = false;

    private readonly INavigationService _navigationService;

    public StationsViewModel(INavigationService navigationService)
    {
        Settings = App.MySettings;
        Channels = new ObservableCollection<ChannelModel>();
        PlayChannelCommand = new RelayCommand(a => PlayChannel(a));
        FavoriteChannelCommand = new RelayCommand(a => FavChannel(a));
        _navigationService = navigationService;
    }

    public async Task<bool> LoadChannelsPlease(bool reloadFromWeb = false)
    {
        if(StartingUp)
        {
            return false;
        }
        StartingUp = true;
        var chans = new List<ChannelModel>();
        if(reloadFromWeb || App.CachedChannelList == null || App.CachedChannelList.Count == 0)
        {
            chans = await App.LoadChannels();
        }
        else if(App.CachedChannelList != null && App.CachedChannelList.Count > 0 && Channels.Count == 0 && !reloadFromWeb)
        {
            chans = App.CachedChannelList;
        }

        if(Channels.Count == 0 || reloadFromWeb)
        {
            Channels.Clear();
            foreach(var item in chans.OrderByDescending(a => a.IsFavorite).ThenBy(a => a.Family))
            {
                Channels.Add(item);
            }
        }
        StartingUp = false;
        return true;
    }

    void PlayChannel(object choice)
    {
        if(choice == null)
        {
            return;
        }
        var url = ((ChannelModel)choice).PrimaryURL;
        var backupUrl = ((ChannelModel)choice).PrimaryURL;
        App.MyRadio.StartStreamFromURL(url, backupUrl);
        _navigationService.Navigate(typeof(Views.NowPlayingPage));
    }

    public void PlayChannel(string url, string backupURL = "")
    {
        if(string.IsNullOrEmpty(url))
        {
            return;
        }
        App.MyRadio.StartStreamFromURL(url, backupURL);
        _navigationService.Navigate(typeof(Views.NowPlayingPage));
    }

    void FavChannel(object choice)
    {
        if(choice == null)
        {
            return;
        }
        var id = ((ChannelModel)choice).ID;
        var prevState = ((ChannelModel)choice).IsFavorite;
        ((ChannelModel)choice).IsFavorite = !prevState;
        if(!prevState && !App.MySettings.FavoriteIDs.Contains(id))
        {
            App.MySettings.FavoriteIDs.Add(id);
        }
        else if(prevState && App.MySettings.FavoriteIDs.Contains(id))
        {
            App.MySettings.FavoriteIDs.Remove(id);
        }
        App.MySettings.SaveToFile();
    }
}
