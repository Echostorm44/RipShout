using RipShout.Helpers;
using RipShout.Services;
using RipShout.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
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
using Wpf.Ui.Mvvm.Contracts;

namespace RipShout.Views;

/// <summary>
/// Interaction logic for StationsPage.xaml
/// </summary>
public partial class StationsPage
{
    public StationsViewModel ViewModel { get; }

    //private readonly INavigationService _navigationService;
    //IRadioService radio;

    public StationsPage(StationsViewModel vm)//, IRadioService radioService)
    {
        ViewModel = vm;
        //vm.StartItUp().Wait();
        this.DataContext = ViewModel;
        InitializeComponent();
    }

    //https://api.audioaddict.com/v1/radiotunes/currently_playing
    // Root myDeserializedClass = JsonConvert.DeserializeObject<List<Root>>(myJsonResponse);
    //public class Root
    //{
    //    public int channel_id { get; set; }
    //    public string channel_key { get; set; }
    //    public Track track { get; set; }
    //}

    //public class Track
    //{
    //    public string display_artist { get; set; }
    //    public string display_title { get; set; }
    //    public double duration { get; set; }
    //    public int id { get; set; }
    //    public DateTime start_time { get; set; }
    //}

    private async void btnPlayEnteredURL_Click(object sender, RoutedEventArgs e)
    {
        /*        
            We'll need a play button, Name, description, url, fav && a type maybe &&  maybe support playlist url too
            cache stations for session
            do a now playing row too for each station
         */

        // These are better
        //https://api.audioaddict.com/v1/radiotunes/channels

        //https://api.audioaddict.com/v1/di/channels

        //https://listen.radiotunes.com/premium/00sdance.pls?listen_key=5c3aae2e95899da95630a7dc 

        //

        //HttpClient client = new HttpClient();
        //var request = new HttpRequestMessage()
        //{
        //    RequestUri = new Uri("https://www.di.fm/"), //new Uri("https://www.radiotunes.com/"),
        //    Method = HttpMethod.Get,
        //};

        //await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ContinueWith((tm) =>
        //{
        //    var response = tm.Result;
        //    var rooo = response.Content.ReadAsStringAsync().Result;
        //    Regex regStuff = new Regex(@"di.app.start\((.+?)\);");
        //    var moo = regStuff.Match(rooo);
        //    if(moo.Groups != null && moo.Groups.Count == 2)
        //    {
        //        var hooo = moo.Groups[1].Value;
        //        var myDeserializedClass = Newtonsoft.Json.JsonConvert.DeserializeObject<AudioAddictServerInfo.Root>(hooo);
        //        var wwww = 1;
        //    }
        //});


        //var iTunesTrackInfo = TrackInfoHelpers.GetTrackInfoFromItunes("Nirvana", "Polly");
        //var discogTrackInfo =  TrackInfoHelpers.GetTrackInfoFromDiscogs("Nirvana", "Polly", "dzlteADaCwkHvvgoxQKhfIlXujJIZJuFxeaWselC");        
        //var artistID = await TrackInfoHelpers.GetArtistIdFromMusicBrainz("Nirvana", "Nevermind");
        //var fanArt = TrackInfoHelpers.GetFanArtFromFanArt(artistID, "a1da18ae7b743cf897c170678b58d746");
        //https://listen.radiotunes.com/premium_high/00srock.pls?listen_key=5c3aae2e95899da95630a7dc

        //App.MyRadio.StartStreamFromURL(@"https://listen.radiotunes.com/premium_high/00srock.pls?listen_key=5c3aae2e95899da95630a7dc");


        //App.MyRadio.StartStreamFromURL(@"https://strm112.1.fm/90s_mobile_mp3?aw_0_req.gdpr=true");

        //App.MyRadio.StartStreamFromURL(@"http://prem1.radiotunes.com:80/the80s?5c3aae2e95899da95630a7dc");

        //recordWorker.RunWorkerAsync(@"http://prem1.radiotunes.com/guitar?5c3aae2e95899da95630a7dc");

        //radio.StartStreamFromURL(@"http://prem1.radiotunes.com/guitar?5c3aae2e95899da95630a7dc");

        //var loo = NavigationService;
        //_navigationService.Navigate(typeof(Views.NowPlayingPage));
    }

    private async void UiPage_Loaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.StartItUp();
    }
}
