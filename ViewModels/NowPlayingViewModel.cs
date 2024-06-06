using CommunityToolkit.Mvvm.Messaging;
using RipShout.Helpers;
using RipShout.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RipShout.ViewModels;

public class NowPlayingViewModel : INotifyPropertyChanged, IDisposable
{
    public event PropertyChangedEventHandler? PropertyChanged;
    string currentBytesRead = "0";
    public string CurrentBytesRead
    {
        get
        {
            return currentBytesRead;
        }

        set
        {
            if(currentBytesRead == value)
            {
                return;
            }

            currentBytesRead = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentBytesRead)));
        }
    }
    string currentSongName = "";
    public string CurrentArtistAndSongNames
    {
        get
        {
            return currentSongName;
        }

        set
        {
            if(currentSongName == value)
            {
                return;
            }

            currentSongName = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentArtistAndSongNames)));
        }
    }
    public ConcurrentDictionary<int, string> BackDropImages { get; set; }
    public string DefaultBackDropImagePath { get; set; }
    string songName;
    public string SongName
    {
        get
        {
            return songName;
        }

        set
        {
            if(songName == value)
            {
                return;
            }

            songName = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SongName)));
        }
    }
    string artistName;
    public string ArtistName
    {
        get
        {
            return artistName;
        }

        set
        {
            if(artistName == value)
            {
                return;
            }

            artistName = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ArtistName)));
        }
    }
    Timer imageTimer;
    bool albumArtLoaded = false;
    bool backdropsLoaded = false;

    string currentAlbumImagePath;
    public string CurrentAlbumImagePath
    {
        get
        {
            return currentAlbumImagePath;
        }

        set
        {
            if(currentAlbumImagePath == value)
            {
                return;
            }

            currentAlbumImagePath = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentAlbumImagePath)));
        }
    }

    string currentBackdropImagePath;
    public string CurrentBackdropImagePath
    {
        get
        {
            return currentBackdropImagePath;
        }

        set
        {
            if(currentBackdropImagePath == value)
            {
                return;
            }

            currentBackdropImagePath = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentBackdropImagePath)));
        }
    }

    int currentBackdropIndex = 0;

    //public MediaPlayer Playa { get; set; }
    double playerVolume;
    private string volPercent;

    public double PlayerVolume
    {
        get
        {
            return playerVolume;
        }

        set
        {
            if(playerVolume == value)
            {
                return;
            }
            int volAsInt = (int)(value * 100);
            VolPercent = volAsInt.ToString() + "%";
            App.MySettings.PlayerVolume = volAsInt;
            App.MyRadio.VlcMediaPlayerShell.Volume = volAsInt;
            App.MyRadio.WaveOutPlayer.Volume = (float)value;
            playerVolume = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlayerVolume)));
        }
    }

    public string VolPercent
    {
        get
        {
            return volPercent;
        }
        set
        {
            volPercent = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VolPercent)));
        }
    }

    public NowPlayingViewModel()
    {
        //Playa = App.MyRadio.MyMediaPlaya;
        PlayerVolume = App.MySettings.PlayerVolume;
        BackDropImages = new ConcurrentDictionary<int, string>();
        DefaultBackDropImagePath = System.IO.Path.Combine(Assembly.GetExecutingAssembly().Location, "/Images/DefaultBackdrop.png");

        CurrentBackdropImagePath = DefaultBackDropImagePath;
        // We'll have to have a timed loop that will rotate the background images every now and then
        imageTimer = new Timer(ImageTimerElapsed, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));

        WeakReferenceMessenger.Default.Register<CurrentStreamStatsChangedMessage>(this, (r, m) =>
        {
            var incomingSongName = m.Value.SongArtistCombined;
            if(CurrentArtistAndSongNames != incomingSongName)
            {
                // Track has changed, 
                albumArtLoaded = false;
                backdropsLoaded = false;
            }
            CurrentArtistAndSongNames = incomingSongName;
            SongName = m.Value.SongName?.Trim();
            ArtistName = m.Value.ArtistName?.Trim();
            CurrentBytesRead = GeneralHelpers.GetHumanReadableFileSize(m.Value.BytesRead);
            // Handle the message here, with r being the recipient and m being the
            // input message. Using the recipient passed as input makes it so that
            // the lambda expression doesn't capture "this", improving performance.

            // These two are loading too soon.  Need to wait for message saying there is something to look at or not.
            if(!albumArtLoaded && m.Value.ArtLoaded)
            {
                if(!string.IsNullOrEmpty(m.Value.PathToAlbumArt))
                {
                    CurrentAlbumImagePath = m.Value.PathToAlbumArt;
                }
                else
                {
                    CurrentAlbumImagePath = DefaultBackDropImagePath;
                }
                albumArtLoaded = true;
            }

            if(!backdropsLoaded && m.Value.ArtLoaded)
            {
                BackDropImages.Clear();
                if(m.Value.HasArtistImagesInLocalFolder)
                {
                    // Load up the images
                    int counter = 0;
                    foreach(var item in Directory.GetFiles(m.Value.PathToBackdrops))
                    {
                        BackDropImages.TryAdd(counter, item);
                        counter++;
                    }
                }
                else
                {
                    CurrentBackdropImagePath = DefaultBackDropImagePath;
                }
                backdropsLoaded = true;
            }
        });
    }

    void ImageTimerElapsed(object state)
    {
        if(backdropsLoaded && BackDropImages.Count > 0)
        {
            if(currentBackdropIndex > (BackDropImages.Count - 1))
            {
                currentBackdropIndex = 0;
            }

            CurrentBackdropImagePath = BackDropImages[currentBackdropIndex];
            currentBackdropIndex++;
        }
    }

    public void Dispose()
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }
}
