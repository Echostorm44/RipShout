using RipShout.Helpers;
using RipShout.Services;
using RipShout.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
using Wpf.Ui.Common.Interfaces;
using Wpf.Ui.Mvvm.Contracts;

namespace RipShout.Views;

/// <summary>
/// Interaction logic for StationsPage.xaml
/// </summary>
public partial class StationsPage 
{
    public StationsViewModel ViewModel
    {
        get;
    }

    //private readonly INavigationService _navigationService;
    //IRadioService radio;

    public StationsPage(INavigationService navigationService)//, IRadioService radioService)
    {
        InitializeComponent();
        //radio = radioService;
    }

    private async void btnPlayEnteredURL_Click(object sender, RoutedEventArgs e)
    {
        //var iTunesTrackInfo = TrackInfoHelpers.GetTrackInfoFromItunes("Nirvana", "Polly");
        //var discogTrackInfo =  TrackInfoHelpers.GetTrackInfoFromDiscogs("Nirvana", "Polly", "dzlteADaCwkHvvgoxQKhfIlXujJIZJuFxeaWselC");        
        //var artistID = await TrackInfoHelpers.GetArtistIdFromMusicBrainz("Nirvana", "Nevermind");
        //var fanArt = TrackInfoHelpers.GetFanArtFromFanArt(artistID, "a1da18ae7b743cf897c170678b58d746");


        App.MyRadio.StartStreamFromURL(@"http://prem1.radiotunes.com:80/the80s?5c3aae2e95899da95630a7dc");

        //recordWorker.RunWorkerAsync(@"http://prem1.radiotunes.com/guitar?5c3aae2e95899da95630a7dc");

        //radio.StartStreamFromURL(@"http://prem1.radiotunes.com/guitar?5c3aae2e95899da95630a7dc");

        //var loo = NavigationService;
        //_navigationService.Navigate(typeof(Views.NowPlayingPage));
    }
}
