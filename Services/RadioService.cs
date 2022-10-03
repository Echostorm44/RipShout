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
using System.Windows.Threading;
using Wpf.Ui.Controls;

namespace RipShout.Services;

public class RadioService : IDisposable
{
    public ShoutCastStream CurrentShoutCastStream { get; set; }
    public bool Running { get; set; }
    public bool SaveTrackRunning { get; set; }
    public bool GetTrackDataRunning { get; set; }
    public MediaPlayer MediaPlaya { get; set; }
    int currentSongID = 0;
    Stream? byteOut;
    byte[] buffer = new byte[512];
    bool isFrontCut = true;
    string workingPath = "";
    double totByteCount;
    string lastTitle = "";
    bool workSwitch = true;
    SongDetailsModel currentSongDetails;
    Progress<bool> prog = new Progress<bool>();

    public RadioService()
    {
        CurrentShoutCastStream = new ShoutCastStream();
        currentSongDetails = new SongDetailsModel();
        MediaPlaya = new MediaPlayer();
        MediaPlaya.Volume = App.MySettings.PlayerVolume;
        Running = false;
    }

    public void StopStreaming()
    {
        workSwitch = false;
    }

    public void StartStreamFromURL(string url, string backupURL)
    {
        StopStreaming();
        while(Running || GetTrackDataRunning || SaveTrackRunning)
        {
            Task.Delay(100).Wait();
        }
        CurrentShoutCastStream = new ShoutCastStream();
        MediaPlaya.Stop();
        MediaPlaya.Close();
        MediaPlaya.Volume = App.MySettings.PlayerVolume;
        MediaPlaya.Open(new Uri(url));
        MediaPlaya.Play();
        workSwitch = true;

        Task.Factory.StartNew(() =>
        {
            try
            {
                Running = true;
                Task<bool> startUp = CurrentShoutCastStream.StartUp(url, backupURL);
                var foo = startUp.Result;
                CurrentShoutCastStream.StreamTitleChanged += new StreamTitleChangedHandler(scS_StreamTitleChanged);
                if(!Directory.Exists(App.MySettings.SaveTempMusicToFolder + @"\" + Regex.Replace(CurrentShoutCastStream.StreamGenre, @"[^A-Za-z0-9 -]", "") + @"\"))
                {
                    Directory.CreateDirectory(App.MySettings.SaveTempMusicToFolder + @"\" + Regex.Replace(CurrentShoutCastStream.StreamGenre, @"[^A-Za-z0-9 -]", "") + @"\");
                }
                byteOut = new FileStream(App.MySettings.SaveTempMusicToFolder + @"\" +
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
                        return;
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
                GeneralHelpers.WriteLogEntry(ex.ToString(), GeneralHelpers.LogFileType.Exception);
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
                Running = false;
            }
        });
    }

    private void SetCurrentSongDataToStopped()
    {
        currentSongDetails.ArtistName = "NA";
        currentSongDetails.SongName = "Stopped";
        currentSongDetails.BytesRead = 0;
    }

    void SaveFinishedTrack(string filePath, SongDetailsModel trackData)
    {
        SaveTrackRunning = true;
        try
        {
            var folderPath = Path.Combine(App.MySettings.SaveFinalMusicToFolder, trackData.Genre);
            if(!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            string mp3Path = CheckPathForDupesAndIncIfNeeded(folderPath + "\\" + trackData.SongName + ".mp3");

            using(var reader = new MediaFoundationReader(filePath))
            {
                MediaFoundationEncoder.EncodeToMp3(reader, mp3Path);
                // If this turns into a weak link we can do something with ffmpeg and lame
                /* convert:
                    command: ffmpeg -i $source -y -vn -aq 2 $dest
                    extension: mp3 */
                var finalMP3 = TagLib.File.Create(mp3Path);
                finalMP3.Tag.AlbumArtists = new string[] { trackData.ArtistName };
                finalMP3.Tag.Performers = new string[] { trackData.ArtistName };
                finalMP3.Tag.Title = trackData.SongName;
                finalMP3.Tag.Album = trackData.AlbumName;
                finalMP3.Tag.Genres = new string[] { trackData.Genre };
                if(!string.IsNullOrEmpty(trackData.PathToAlbumArt))
                {
                    var picture = new TagLib.Picture(trackData.PathToAlbumArt);
                    picture.Type = TagLib.PictureType.FrontCover;
                    picture.MimeType = "image/jpeg";
                    finalMP3.Tag.Pictures = new TagLib.IPicture[] { picture };
                }
                if(uint.TryParse(trackData.ReleaseYear, out var trackYear))
                {
                    finalMP3.Tag.Year = trackYear;
                }
                if(uint.TryParse(trackData.TrackNumber, out var trackNumber))
                {
                    finalMP3.Tag.Track = trackNumber;
                }

                finalMP3.Save();
                int deleteAttempts = 0;
                while(deleteAttempts <= 10)
                {
                    try
                    {
                        System.IO.File.Delete(filePath);
                    }
                    catch(Exception ex)
                    {
                        GeneralHelpers.WriteLogEntry(ex.ToString(), GeneralHelpers.LogFileType.Exception);
                    }
                    Thread.Sleep(100);
                    deleteAttempts++;
                }
            }
        }
        catch(Exception ex)
        {
            GeneralHelpers.WriteLogEntry(ex.ToString(), GeneralHelpers.LogFileType.Exception);
        }
        finally
        {
            SaveTrackRunning = false;
        }
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
                workingPath = GetSongSavePath(App.MySettings.SaveTempMusicToFolder, cleanTitle, cleanGenre, true);
                byteOut = new FileStream(workingPath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
                isFrontCut = false;
                if(cleanTitle.Length > 0)
                {
                    lastTitle = cleanTitle;
                }
            }
            else
            {
                string newPath = GetSongSavePath(App.MySettings.SaveTempMusicToFolder, cleanTitle, cleanGenre, false);
                byteOut = new FileStream(newPath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
                workingPath = newPath;
                lastTitle = cleanTitle;
            }

            currentSongID++;
            currentSongDetails = new SongDetailsModel()
            {
                ID = currentSongID,
                Genre = genre
            };
            var splitTitle = cleanTitle.Split('-', StringSplitOptions.TrimEntries);
            if(splitTitle.Length > 1)
            {
                var workingDeetz = splitTitle.ToList();
                currentSongDetails.ArtistName = workingDeetz.First();
                workingDeetz.RemoveAt(0);
                currentSongDetails.SongName = String.Join("-", workingDeetz);// In case the song is called 867-5309 Jenny or the like
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
            GeneralHelpers.WriteLogEntry(ex.ToString(), GeneralHelpers.LogFileType.Exception);
        }
        finally
        {
            totByteCount = 0;
        }
    }

    void GrabExtraSongData(ref SongDetailsModel currentModel)
    {
        // Reach out for fan art, album art, alb name, lyrics, etc
        GetTrackDataRunning = true;
        try
        {
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
            if(string.IsNullOrEmpty(artistID))
            {
                GeneralHelpers.WriteLogEntry(currentModel.ArtistName + "\r\n" + currentModel.SongName, GeneralHelpers.LogFileType.AlbumLookupFailure);
                artistID = TrackInfoHelpers.GetArtistIdFromMusicBrainz(currentModel.ArtistName).Result;
            }
            currentModel.HasArtistImagesInLocalFolder = false;
            if(!string.IsNullOrEmpty(artistID))
            {
                var fanArt = TrackInfoHelpers.GetFanArtFromFanArt(artistID, "a1da18ae7b743cf897c170678b58d746");
                if(fanArt != null && fanArt.artistbackground.Count > 0)
                {
                    string backdropPath = App.MySettings.ArtistImageCacheFolder + "\\" + artistID;
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
            else
            {
                GeneralHelpers.WriteLogEntry(currentModel.ArtistName, GeneralHelpers.LogFileType.ArtistLookupFailure);
            }
            currentModel.ArtLoaded = true;
        }
        catch(Exception ex)
        {
            GeneralHelpers.WriteLogEntry(ex.ToString(), GeneralHelpers.LogFileType.Exception);
        }
        GetTrackDataRunning = false;
    }

    void SaveAlbumArt(string downloadUrl, ref SongDetailsModel currentModel)
    {
        try
        {
            string albumArtPath = App.MySettings.AlbumImageCacheFolder + "\\"
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
        catch(Exception ex)
        {
            GeneralHelpers.WriteLogEntry(ex.ToString(), GeneralHelpers.LogFileType.Exception);
        }
    }


    private string GetSongSavePath(string rootPath, string cleanTitle, string cleanGenre, bool isFrontCut)
    {
        var tempPath = rootPath + @"\" + cleanGenre + @"\" + (isFrontCut ? @"[Front-Cut]" : "") + cleanTitle + ".mp3";
        return CheckPathForDupesAndIncIfNeeded(tempPath);
    }

    public static string CheckPathForDupesAndIncIfNeeded(string filePath)
    {
        try
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
        }
        catch(Exception ex)
        {
            GeneralHelpers.WriteLogEntry(ex.ToString(), GeneralHelpers.LogFileType.Exception);
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
