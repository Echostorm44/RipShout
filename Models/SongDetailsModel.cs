using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TagLib.Mpeg;

namespace RipShout.Models;

public class SongDetailsModel
{
    public int ID { get; set; }
    public string SongName { get; set; }
    public string ArtistName { get; set; }
    public string AlbumName { get; set; }
    public string Genre { get; set; }
    public int BytesRead { get; set; }
    public string Bitrate { get; set; }
    public bool HasArtistImagesInLocalFolder { get; set; }
    public string Lyrics { get; set; }
    public string PathToAlbumArt { get; set; }
    public bool AlbumArtLoaded { get; set; }
    public string ReleaseYear { get; set; }
    public string TrackNumber { get; set; }
    public string PathToBackdrops { get; set; }

    public string SongArtistCombined
    { 
        get =>  ArtistName + " - " + SongName;
    }

    public SongDetailsModel()
    {
        ID = 0;
        SongName = "";
        ArtistName = "";
        AlbumName = "";
        Genre = "";
        BytesRead = 0;
        Bitrate = "";
        HasArtistImagesInLocalFolder = false;
        Lyrics = "";
        PathToAlbumArt = "";
        ReleaseYear = "";
        TrackNumber = "";
        PathToBackdrops = "";
    }

    public SongDetailsModel DeepCopy()
    {
        SongDetailsModel other = (SongDetailsModel)this.MemberwiseClone();
        //other.IdInfo = new IdInfo(IdInfo.IdNumber);
        //other.Name = String.Copy(Name);
        return other;
    }
}

public class CurrentStreamStatsChangedMessage : ValueChangedMessage<SongDetailsModel>
{
    public CurrentStreamStatsChangedMessage(SongDetailsModel stats) : base(stats)
    {
    }
}

