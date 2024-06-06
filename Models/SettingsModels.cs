using CommunityToolkit.Mvvm.ComponentModel;
using RipShout.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RipShout.Models;

public class SettingsModel : INotifyPropertyChanged
{
    public SettingsModel()
    {
        audioAddictListenKey = "";
        showClassicalRadioChannels = false;
        showDiChannels = false;
        showJazzRadioChannels = false;
        showOneFmChannels = true;
        showRadioTunesChannels = false;
        showRockRadioChannels = false;
        showZenRadioChannels = false;
        playerVolume = 1;
        FavoriteIDs = new List<string>();
        LoggingOn = false;
    }

    bool loggingOn;
    public bool LoggingOn
    {
        get
        {
            return loggingOn;
        }

        set
        {
            SetField(ref loggingOn, value);
        }
    }
    string audioAddictListenKey;
    public string AudioAddictListenKey
    {
        get
        {
            return audioAddictListenKey;
        }

        set
        {
            SetField(ref audioAddictListenKey, value);
        }
    }
    bool showDiChannels;
    public bool ShowDiChannels
    {
        get
        {
            return showDiChannels;
        }
        set
        {
            SetField(ref showDiChannels, value);
        }
    }
    bool showRadioTunesChannels;
    public bool ShowRadioTunesChannels
    {
        get
        {
            return showRadioTunesChannels;
        }

        set
        {
            SetField(ref showRadioTunesChannels, value);
        }
    }
    bool showZenRadioChannels;
    public bool ShowZenRadioChannels
    {
        get
        {
            return showZenRadioChannels;
        }

        set
        {
            SetField(ref showZenRadioChannels, value);
        }
    }
    bool showJazzRadioChannels;
    public bool ShowJazzRadioChannels
    {
        get
        {
            return showJazzRadioChannels;
        }

        set
        {
            SetField(ref showJazzRadioChannels, value);
        }
    }
    bool showRockRadioChannels;
    public bool ShowRockRadioChannels
    {
        get
        {
            return showRockRadioChannels;
        }

        set
        {
            SetField(ref showRockRadioChannels, value);
        }
    }
    bool showClassicalRadioChannels;
    public bool ShowClassicalRadioChannels
    {
        get
        {
            return showClassicalRadioChannels;
        }

        set
        {
            SetField(ref showClassicalRadioChannels, value);
        }
    }
    bool showOneFmChannels;
    public bool ShowOneFmChannels
    {
        get
        {
            return showOneFmChannels;
        }

        set
        {
            SetField(ref showOneFmChannels, value);
        }
    }
    public List<string> FavoriteIDs { get; set; }
    string saveTempMusicToFolder;
    public string SaveTempMusicToFolder
    {
        get
        {
            return saveTempMusicToFolder;
        }

        set
        {
            SetField(ref saveTempMusicToFolder, value);
        }
    }
    string saveFinalMusicToFolder;
    public string SaveFinalMusicToFolder
    {
        get
        {
            return saveFinalMusicToFolder;
        }

        set
        {
            SetField(ref saveFinalMusicToFolder, value);
        }
    }
    string artistImageCacheFolder;
    public string ArtistImageCacheFolder
    {
        get
        {
            return artistImageCacheFolder;
        }

        set
        {
            SetField(ref artistImageCacheFolder, value);
        }
    }
    string albumImageCacheFolder;
    public string AlbumImageCacheFolder
    {
        get
        {
            return albumImageCacheFolder;
        }

        set
        {
            SetField(ref albumImageCacheFolder, value);
        }
    }
    public int LastWindowHeight { get; set; }
    public int LastWindowWidth { get; set; }
    public int LastWindowX { get; set; }
    public int LastWindowY { get; set; }
    double playerVolume;
    public double PlayerVolume
    {
        get
        {
            if(playerVolume > 1)
            {
                playerVolume = playerVolume / 100;
            }
            return playerVolume;
        }

        set
        {
            double setVal = value;
            if(setVal > 1)
            {
                setVal = setVal / 100;
            }
            SetField(ref playerVolume, setVal);
        }
    }

    public delegate void ValueChangedHander(object source);
    public event ValueChangedHander? ValueChanged;

    protected void OnValueChanged()
    {
        if(ValueChanged != null)
        {
            ValueChanged.Invoke(this);
            //ValueChanged(this);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if(EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }
        field = value;
        OnValueChanged();
        OnPropertyChanged(propertyName);
        return true;
    }

    public void SaveToFile()
    {
        SettingsIoHelpers.SaveGeneralSettingsToDisk(this);
    }
}
