using CommunityToolkit.Mvvm.Messaging;
using NAudio.Wave;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RipShout.Helpers;
using RipShout.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Wpf.Ui.Controls;

namespace RipShout.Services;

public class RadioService : IDisposable
{
    public ShoutCastStream CurrentShoutCastStream { get; set; }
    public bool Running { get; set; }
    public ObservableCollection<string> SongHistory { get; set; }
    public ConcurrentBag<SongDetailsModel> SongDataHistory { get; set; }
    public MediaPlayer MediaPlaya { get; set; }
    string storePath = @"D:\RipShoutMusic";
    string imageCachePath = @"D:\RipShoutMusic\Images\";
    int currentSongID = 0;
    Stream? byteOut;
    byte[] buffer = new byte[512];
    bool isFrontCut = true;
    string workingPath = "";
    double totByteCount;
    string lastTitle = "";
    bool workSwitch = true;
    SongDetailsModel currentSongDetails;

    // TODO add a way to adjust media player volume

    public RadioService()
    {
        SongHistory = new ObservableCollection<string>();
        CurrentShoutCastStream = new ShoutCastStream();
        currentSongDetails = new SongDetailsModel();
        SongDataHistory = new ConcurrentBag<SongDetailsModel>();
        MediaPlaya = new MediaPlayer();
        MediaPlaya.MediaEnded += Player_MediaEnded;
        if(!Directory.Exists(storePath))
        {
            Directory.CreateDirectory(storePath);
        }
    }

    private void Player_MediaEnded(object? sender, EventArgs e)
    {
    }

    public void StopStreaming()
    {
        workSwitch = false;
    }

    public void StartStreamFromURL(string url)
    {
        MediaPlaya.Volume = 1;
        MediaPlaya.Open(new Uri(url));
        MediaPlaya.Play();
        workSwitch = true;
        Task.Run(() =>
        {
            try
            {
                Task<bool> startUp = CurrentShoutCastStream.StartUp(url);
                var foo = startUp.Result;
                CurrentShoutCastStream.StreamTitleChanged += new StreamTitleChangedHandler(scS_StreamTitleChanged);
                if(!Directory.Exists(storePath + @"\" + Regex.Replace(CurrentShoutCastStream.StreamGenre, @"[^A-Za-z0-9 -]", "") + @"\"))
                {
                    Directory.CreateDirectory(storePath + @"\" + Regex.Replace(CurrentShoutCastStream.StreamGenre, @"[^A-Za-z0-9 -]", "") + @"\");
                }
                byteOut = new FileStream(storePath + @"\" +
                    Regex.Replace(CurrentShoutCastStream.StreamGenre, @"[^A-Za-z0-9 -]", "") + @"\" + @"[Front-Cut]" +
                    CurrentShoutCastStream.StreamTitle + ".mp3", FileMode.Create, FileAccess.ReadWrite, FileShare.Read);

                SetCurrentSongDataToStopped();

                while(workSwitch)
                {
                    int bytes = CurrentShoutCastStream.Read(buffer, 0, buffer.Length);

                    if(bytes <= 0)
                    {
                        SetCurrentSongDataToStopped();
                        WeakReferenceMessenger.Default.Send(new CurrentStreamStatsChangedMessage(currentSongDetails));
                        isFrontCut = false;
                        return false;
                    }

                    for(int i = 0; i < bytes; i++)
                    {
                        byteOut.Write(buffer, i, 1);
                    }
                    totByteCount += bytes;
                    currentSongDetails.BytesRead = (int)totByteCount;
                    WeakReferenceMessenger.Default.Send(new CurrentStreamStatsChangedMessage(currentSongDetails));
                }
            }
            catch(Exception ex)
            {
                System.Windows.MessageBox.Show("Error:" + ex.ToString());
                workSwitch = false;
            }
            finally
            {
                if(byteOut != null)
                {
                    byteOut.Flush();
                    byteOut.Close();
                }
                isFrontCut = true;

                CurrentShoutCastStream.StreamTitleChanged -= new StreamTitleChangedHandler(scS_StreamTitleChanged);
            }
            return true;
        }).ContinueWith(a =>
        {
            MediaPlaya.Stop();
        }, TaskScheduler.FromCurrentSynchronizationContext());
    }

    private void SetCurrentSongDataToStopped()
    {
        currentSongDetails.ArtistName = "NA";
        currentSongDetails.SongName = "Stopped";
        currentSongDetails.BytesRead = 0;
    }

    void SaveFinishedTrack(string filePath, SongDetailsModel trackData)
    {
        var folderPath = storePath + @"\Final\" + trackData.Genre + @"\";
        if(!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        string mp3Path = CheckPathForDupesAndIncIfNeeded(storePath + @"\Final\" + trackData.Genre + @"\" + trackData.SongName + ".mp3");

        using(var reader = new MediaFoundationReader(filePath))
        {
            MediaFoundationEncoder.EncodeToMp3(reader, mp3Path);
            // If this turns into a weak link we can do something with ffmpeg and lame
            /* convert:
                command: ffmpeg -i $source -y -vn -aq 2 $dest
                extension: mp3 */
            var tag = TagLib.File.Create(mp3Path);
            tag.Tag.AlbumArtists = new string[] { trackData.ArtistName };
            tag.Tag.Performers = new string[] { trackData.ArtistName };
            tag.Tag.Title = trackData.SongName;
            tag.Tag.Genres = new string[] { trackData.Genre };
            tag.Save();
        }
        // TODO turn this back on but make it safer, could still be in use.
        //File.Delete(filePath);
    }

    void scS_StreamTitleChanged(object source, string title, string genre, string bitrate, string audioEncodeType)
    {
        string cleanTitle = "";
        try
        {
            // Close the last song
            if(byteOut != null)
            {
                byteOut.Flush();
                byteOut.Close();
            }

            if(!string.IsNullOrEmpty(workingPath))
            {   // Convert if it needs it   
                var lastTrack = currentSongDetails.DeepCopy();
                SongDataHistory.Add(lastTrack);
                if(!isFrontCut)
                {
                    ThreadPool.QueueUserWorkItem(o => SaveFinishedTrack(workingPath, lastTrack));
                }
            }
            string cleanGenre = Regex.Replace(genre, @"[^A-Za-z0-9 -]", "");
            if(string.IsNullOrEmpty(cleanGenre))
            {
                cleanGenre = "Unknown";
            }
            cleanTitle = Regex.Replace(title, @"[^A-Za-z0-9 -\]]", "", RegexOptions.CultureInvariant);
            if(string.IsNullOrEmpty(cleanTitle))
            {
                cleanTitle = "Unknown";
            }
            foreach(char c in Path.GetInvalidFileNameChars())
            {
                cleanTitle = cleanTitle.Replace(c, ' ');
            }

            if(isFrontCut)
            {
                workingPath = GetSongSavePath(storePath, cleanTitle, cleanGenre, true);
                byteOut = new FileStream(workingPath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
                isFrontCut = false;
                if(cleanTitle.Length > 0)
                {
                    lastTitle = cleanTitle;
                }
            }
            else
            {
                string newPath = GetSongSavePath(storePath, cleanTitle, cleanGenre, false);
                byteOut = new FileStream(newPath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
                workingPath = newPath;
                lastTitle = cleanTitle;
            }

            currentSongID++;
            SongHistory.Add(cleanTitle);// TODO refactor this out
            currentSongDetails = new SongDetailsModel()
            {
                ID = currentSongID,
                Genre = genre
            };
            string[] splitTitle = cleanTitle.Split('-');
            if(splitTitle.Length > 1)
            {
                currentSongDetails.SongName = splitTitle[1];
                currentSongDetails.ArtistName = splitTitle[0];
            }
            else
            {
                currentSongDetails.SongName = cleanTitle;
            }
            currentSongDetails.Bitrate = bitrate;
            ThreadPool.QueueUserWorkItem(a => GrabExtraSongData(ref currentSongDetails));
            WeakReferenceMessenger.Default.Send(new CurrentStreamStatsChangedMessage(currentSongDetails));
        }
        catch(Exception ex)
        {
            System.Windows.MessageBox.Show(cleanTitle + ex.ToString());
        }
        finally
        {
            totByteCount = 0;
        }
    }

    void GrabExtraSongData(ref SongDetailsModel currentModel)
    {
        // Reach out for fan art, album art, alb name, lyrics, etc

        var discogTrackInfo = TrackInfoHelpers.GetTrackInfoFromDiscogs(currentModel.ArtistName,
                currentModel.SongName, "dzlteADaCwkHvvgoxQKhfIlXujJIZJuFxeaWselC");
        if(discogTrackInfo != null)
        {
            var splitTitle = discogTrackInfo.title.Split('-');// TODO take all after first -
            if(splitTitle.Length > 1)
            {
                currentModel.AlbumName = splitTitle.LastOrDefault()?.Trim();
                currentModel.ReleaseYear = discogTrackInfo.year;
                SaveAlbumArt(discogTrackInfo.cover_image, ref currentModel);
            }
        }
        if(string.IsNullOrEmpty(currentModel.AlbumName))
        {
            var iTunesTrackInfo = TrackInfoHelpers.GetTrackInfoFromItunes(currentModel.ArtistName, currentModel.SongName);
            if(iTunesTrackInfo != null)
            {
                currentModel.AlbumName = iTunesTrackInfo.collectionName;
                currentModel.TrackNumber = iTunesTrackInfo.trackNumber.ToString();
                SaveAlbumArt(iTunesTrackInfo.artworkUrl100, ref currentModel);
            }
        }
        var artistID = "";
        if(!string.IsNullOrEmpty(currentModel.AlbumName))
        {
            artistID = TrackInfoHelpers.GetArtistIdFromMusicBrainz(currentModel.ArtistName, currentModel.AlbumName).Result;
        }
        else
        {
            artistID = TrackInfoHelpers.GetArtistIdFromMusicBrainz(currentModel.ArtistName).Result;
        }
        currentModel.HasArtistImagesInLocalFolder = false;
        if(!string.IsNullOrEmpty(artistID))
        {
            var fanArt = TrackInfoHelpers.GetFanArtFromFanArt(artistID, "a1da18ae7b743cf897c170678b58d746");
            if(fanArt != null && fanArt.artistbackground.Count > 0)
            {
                string backdropPath = imageCachePath + artistID;
                if(!Directory.Exists(backdropPath))
                {
                    Directory.CreateDirectory(backdropPath);
                }
                foreach(var art in fanArt.artistbackground)
                {
                    if(string.IsNullOrEmpty(art.url))
                    {
                        continue;
                    }

                    var fileName = art.url?.Split('/')?.LastOrDefault();
                    if(!string.IsNullOrEmpty(fileName) && !File.Exists(backdropPath + "\\" + fileName))
                    {
                        var backdropClient = new RestClient(art.url);
                        var request = new RestRequest();
                        request.Method = Method.Get;
                        request.Timeout = -1;
                        var response = backdropClient.DownloadData(request);
                        if(response != null && response.Length > 0)
                        {
                            File.WriteAllBytes(backdropPath + "\\" + fileName, response);
                        }
                    }
                }
                if(fanArt.artistbackground.Count > 0 || Directory.GetFiles(backdropPath).Length > 0)
                {
                    currentModel.PathToBackdrops = backdropPath;
                    currentModel.HasArtistImagesInLocalFolder = true;
                }
            }
        }
        currentModel.ArtLoaded = true;
    }

    void SaveAlbumArt(string downloadUrl, ref SongDetailsModel currentModel)
    {
        string albumArtPath = imageCachePath + "AlbumArt\\"
                + GeneralHelpers.GetSHA256HashOfString(currentModel.ArtistName + currentModel.AlbumName);
        string albumArtFilePath = albumArtPath + "\\albumArt.jpg";

        if(File.Exists(albumArtFilePath))
        {
            currentModel.PathToAlbumArt = albumArtFilePath;
            return;
        }
        Directory.CreateDirectory(albumArtPath);
        var client = new RestClient(downloadUrl);
        var request = new RestRequest();
        request.Method = Method.Get;
        request.Timeout = -1;
        var response = client.DownloadData(request);
        if(response != null && response.Length > 0)
        {
            File.WriteAllBytes(albumArtFilePath, response);
        }
        currentModel.PathToAlbumArt = albumArtFilePath;
    }


    private string GetSongSavePath(string rootPath, string cleanTitle, string cleanGenre, bool isFrontCut)
    {
        var tempPath = rootPath + @"\" + cleanGenre + @"\" + (isFrontCut ? @"[Front-Cut]" : "") + cleanTitle + ".mp3";
        return CheckPathForDupesAndIncIfNeeded(tempPath);
    }

    public static string CheckPathForDupesAndIncIfNeeded(string filePath)
    {
        if(File.Exists(filePath))
        {
            string folderPath = Path.GetDirectoryName(filePath);
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string fileExtension = Path.GetExtension(filePath);
            int number = 1;

            Match regex = Regex.Match(fileName, @"^(.+)\((\d+)\)$");

            if(regex.Success)
            {
                fileName = regex.Groups[1].Value;
                number = int.Parse(regex.Groups[2].Value);
            }

            do
            {
                number++;
                string newFileName = $"{fileName}({number}){fileExtension}";
                filePath = Path.Combine(folderPath, newFileName);
            }
            while (File.Exists(filePath));
        }
        return filePath;
    }

    public void Dispose()
    {
        workSwitch = false;
        if(byteOut != null)
        {
            byteOut.Dispose();
        }
        if(CurrentShoutCastStream != null)
        {
            CurrentShoutCastStream.Dispose();
        }
    }
}
