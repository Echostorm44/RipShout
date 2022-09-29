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

namespace RipShout.ViewModels;

public class StationsViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public ObservableCollection<ChannelModel> Channels { get; set; }
    public SettingsModel Settings { get; set; }
    public ICommand PlayChannelCommand { get; private set; }
    public ICommand FavoriteChannelCommand { get; private set; }
    bool StartingUp = false;

    public StationsViewModel()
    {
        Settings = App.MySettings;
        Channels = new ObservableCollection<ChannelModel>();
        PlayChannelCommand = new RelayCommand(a => PlayChannel(a));
        FavoriteChannelCommand = new RelayCommand(a => FavChannel(a));
    }

    public async Task<bool> StartItUp()
    {
        if(StartingUp)
        {
            return false;
        }
        StartingUp = true;
        Channels.Clear();
        var chans = new List<ChannelModel>();
        if(App.CachedChannelList.Count != 0 && App.CachedChannelListConfig == (App.MySettings.AudioAddictListenKey, App.MySettings.ShowDiChannels, App.MySettings.ShowRadioTunesChannels,
            App.MySettings.ShowJazzRadioChannels, App.MySettings.ShowRockRadioChannels, App.MySettings.ShowZenRadioChannels, App.MySettings.ShowClassicalRadioChannels, App.MySettings.ShowOneFmChannels))
        {
            chans = App.CachedChannelList;
        }
        else
        {
            chans = await AudioAddictChannelServices.AudioAddictGetChannelsService.GetChannelsAsync(App.MySettings.AudioAddictListenKey, App.MySettings.ShowDiChannels, App.MySettings.ShowRadioTunesChannels,
            App.MySettings.ShowJazzRadioChannels, App.MySettings.ShowRockRadioChannels, App.MySettings.ShowZenRadioChannels, App.MySettings.ShowClassicalRadioChannels, App.MySettings.FavoriteIDs);
            // Add oneFm to chans
            var oneFMChans = await OneFmChannelServices.OneFmGetStationsService.GetChannelsAsync(App.MySettings.FavoriteIDs);
            if(oneFMChans != null)
            {
                chans.AddRange(oneFMChans);
            }
            App.CachedChannelList = chans;
            App.CachedChannelListConfig = (App.MySettings.AudioAddictListenKey, App.MySettings.ShowDiChannels, App.MySettings.ShowRadioTunesChannels,
            App.MySettings.ShowJazzRadioChannels, App.MySettings.ShowRockRadioChannels, App.MySettings.ShowZenRadioChannels, App.MySettings.ShowClassicalRadioChannels, App.MySettings.ShowOneFmChannels);
        }
        foreach(var item in chans.OrderByDescending(a => a.IsFavorite).ThenBy(a => a.Family))
        {
            Channels.Add(item);
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
        App.MyRadio.StartStreamFromURL(url);
        App.MyRadio.MediaPlaya.Volume = 1;
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
