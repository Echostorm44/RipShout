using RipShout.Helpers;
using RipShout.Services;
using RipShout.ViewModels;
using System;
using System.Collections.Generic;
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

namespace RipShout.Views;

/// <summary>
/// Interaction logic for StationsPage.xaml
/// </summary>
public partial class StationsPage
{
    public StationsViewModel ViewModel { get; }

    public StationsPage(StationsViewModel vm)//, IRadioService radioService)
    {
        ViewModel = vm;
        this.DataContext = ViewModel;
        InitializeComponent();
    }

    private async void btnPlayEnteredURL_Click(object sender, RoutedEventArgs e)
    {
        //var iTunesTrackInfo = TrackInfoHelpers.GetTrackInfoFromItunes("Nirvana", "Polly");
        //var discogTrackInfo =  TrackInfoHelpers.GetTrackInfoFromDiscogs("Nirvana", "Polly", "dzlteADaCwkHvvgoxQKhfIlXujJIZJuFxeaWselC");        
        //var artistID = await TrackInfoHelpers.GetArtistIdFromMusicBrainz("Nirvana", "Nevermind");
        //var fanArt = TrackInfoHelpers.GetFanArtFromFanArt(artistID, "a1da18ae7b743cf897c170678b58d746");
        //var loo = NavigationService;
        //_navigationService.Navigate(typeof(Views.NowPlayingPage));
    }

    private async void UiPage_Loaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.StartItUp();
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

        foreach(var item in ViewModel.Channels)
        {
            if(item.Family == stationFam && !item.IsFavorite)
            {
                item.IsVisible = isVisible;
            }
        }
    }

    private void ToggleSwitch_Unchecked(object sender, RoutedEventArgs e)
    {
        var foo = (ToggleSwitch)e.Source;
        ToggleStationVisiblity(foo, false);
    }
}
