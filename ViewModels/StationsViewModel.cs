using NAudio.Wave;
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
    private ICommand _navigateCommand;

    public ObservableCollection<ChannelModel> Channels { get; set; }
    public ICommand PlayChannelCommand { get; private set; }

    public StationsViewModel()
    {
        Channels = new ObservableCollection<ChannelModel>();
        PlayChannelCommand = new RelayCommand(a => PlayChannel(a));
    }

    public async Task<bool> StartItUp()
    {
        Channels.Clear();
        var chans = await AudioAddictChannelServices.AudioAddictGetChannelsService.GetChannelsAsync(App.MySettings.AudioAddictListenKey, App.MySettings.ShowDiChannels, App.MySettings.ShowRadioTunesChannels,
            App.MySettings.ShowJazzRadioChannels, App.MySettings.ShowRockRadioChannels, App.MySettings.ShowZenRadioChannels, App.MySettings.ShowClassicalRadioChannels, App.MySettings.FavoriteIDs);
        foreach(var item in chans)
        {
            Channels.Add(item);
        }

        // TODO Fire1  chans

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
        //var selectedAccount = (DaocAccount)choice;
    }
}
