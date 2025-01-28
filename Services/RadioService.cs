using CommunityToolkit.Mvvm.Messaging;
using ExCSS;
using Fizzler;
using Hqub.MusicBrainz.API.Entities;
using LibVLCSharp.Shared;
using NAudio.Gui;
using NAudio.Wave;
using RestSharp;
using RipShout.Helpers;
using RipShout.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace RipShout.Services;

public class RadioService : IDisposable
{
	public bool Running { get; set; }
	public bool SaveTrackRunning { get; set; }
	public bool GetTrackDataRunning { get; set; }
	string workingPath = "";
	bool workSwitch = true;
	SongDetailsModel currentSongDetails;

	public LibVLCSharp.Shared.MediaPlayer VlcMediaPlayerShell { get; set; }
	public LibVLC libVLC { get; set; }
	public WaveOutEvent WaveOutPlayer { get; set; }

	public RadioService()
	{
		currentSongDetails = new SongDetailsModel();
		Running = false;
		Core.Initialize();
		libVLC = new LibVLC(enableDebugLogs: true, "--no-video", "--network-caching=4000");
		VlcMediaPlayerShell = new LibVLCSharp.Shared.MediaPlayer(libVLC);
		WaveOutPlayer = new WaveOutEvent();
		VlcMediaPlayerShell.Volume = 1;
		WaveOutPlayer.Volume = 1;
	}

	public void StopStreaming()
	{
		workSwitch = false;
	}

	public void StartStreamFromURL(string url, string backupURL)
	{
		StopStreaming();
		Stopwatch sw = new Stopwatch();
		sw.Start();
		while(Running || GetTrackDataRunning || SaveTrackRunning)
		{
			Task.Delay(100).Wait();
			if(sw.Elapsed.TotalMinutes == 1)
			{// Something has gone very wrong.  Break out && let them start over if possible
				Running = false;
				GetTrackDataRunning = false;
				SaveTrackRunning = false;
				sw.Stop();
				return;
			}
		}

		workSwitch = true;

		Task.Factory.StartNew(async () =>
		{
			try
			{
				Running = true;
				var media = new Media(libVLC, new Uri(url));
				media.AddOption(":no-video");
				media.AddOption(":network-caching=5000");
				VlcMediaPlayerShell.Media = media;
				var outputFolder = App.MySettings.SaveTempMusicToFolder;
				if(!Directory.Exists(outputFolder))
				{
					Directory.CreateDirectory(outputFolder);
				}
				var outputFilePath = System.IO.Path.Combine(outputFolder, "frontCut.wav");
				var waveFormat = new WaveFormat(8000, 16, 1);
				var writer = new WaveFileWriter(outputFilePath, waveFormat);
				var waveProvider = new BufferedWaveProvider(waveFormat);
				WaveOutPlayer.Init(waveProvider);
				VlcMediaPlayerShell.SetAudioFormatCallback(AudioSetup, AudioCleanup);
				VlcMediaPlayerShell.SetAudioCallbacks(PlayAudio, PauseAudio, ResumeAudio, FlushAudio, DrainAudio);

				VlcMediaPlayerShell.Play();
				WaveOutPlayer.Play();
				// Wait a bit then set volume again, this doesn't seem to take until it is playing.
				await Task.Delay(500);

				WaveOutPlayer.Volume = (float)App.MySettings.PlayerVolume;
				var initVol = (int)(App.MySettings.PlayerVolume * 100);
				VlcMediaPlayerShell.Volume = initVol;

				var currentTitle = "frontCut";
				string playingTitle = "";
				string savingTitle = "";
				int trackCounter = 0;
				while(workSwitch)
				{
					await Task.Delay(500);
					var newMetaTitle = media.Meta(MetadataType.NowPlaying);
					var genre = media.Meta(MetadataType.Genre);
					if(!string.IsNullOrEmpty(newMetaTitle))
					{
						if(newMetaTitle != currentTitle)
						{
							var saveSongDetails = currentSongDetails.DeepCopy();
							currentSongDetails = new SongDetailsModel();
							currentSongDetails.ArtLoaded = false;
							// Clean up the title so it isn't invalid to use in the save path
							playingTitle = Regex.Replace(newMetaTitle, @"[^A-Za-z0-9 -\]]", "", 
                                RegexOptions.CultureInvariant);
							if(string.IsNullOrEmpty(newMetaTitle))
							{
								playingTitle = "Unknown";
							}
							foreach(char c in Path.GetInvalidFileNameChars())
							{
								playingTitle = playingTitle.Replace(c, ' ');
							}
							genre = Regex.Replace(genre, @"[^A-Za-z0-9 -]", "");
							if(string.IsNullOrEmpty(genre))
							{
								genre = "Unknown";
							}
							currentSongDetails.Genre = genre;
							var splitTitle = newMetaTitle.Split('-', StringSplitOptions.TrimEntries);
							if(splitTitle.Length > 1)
							{
								var workingDeetz = splitTitle.ToList();
								currentSongDetails.ArtistName = workingDeetz.First();
								workingDeetz.RemoveAt(0);
								currentSongDetails.SongName = String.Join("-", workingDeetz);
								// In case the song is called 867-5309 Jenny or the like
							}
							else
							{
								currentSongDetails.SongName = newMetaTitle;
							}
							ThreadPool.QueueUserWorkItem(a =>
							{
								GrabExtraSongData(ref currentSongDetails);
							});
							writer.Flush();
							waveProvider.ClearBuffer();
							writer.Dispose();
							outputFilePath = System.IO.Path.Combine(outputFolder, playingTitle + ".wav");
							writer = new WaveFileWriter(outputFilePath, waveFormat);

							if(trackCounter > 1)// This means we have a full track to save as it has passed twice, once
							{// for the init && again for the front cut.
								var lastWaveTrack = System.IO.Path.Combine(outputFolder,
                                    savingTitle + ".wav");
								ThreadPool.QueueUserWorkItem(o => 
                                    SaveFinishedTrack(lastWaveTrack, saveSongDetails));
							}
							currentTitle = newMetaTitle;
							savingTitle = playingTitle;
							trackCounter++;
						}
					}
					if(!GetTrackDataRunning && currentSongDetails.ArtLoaded)
					{// Don't mess with it while we're getting new data || we get a race condition on art.
						WeakReferenceMessenger.Default.Send(new CurrentStreamStatsChangedMessage(currentSongDetails));
					}
				}
				// outside work loop
				WaveOutPlayer.Stop();
				VlcMediaPlayerShell.Stop();

                #region Audio Callbacks

				void PlayAudio(IntPtr data, IntPtr samples, uint count, long pts)
				{
					try
					{
						int bytes = (int)count * 2; // (16 bit, 1 channel)
						var buffer = new byte[bytes];
						Marshal.Copy(samples, buffer, 0, bytes);
						currentSongDetails.BytesRead += bytes;
						waveProvider.AddSamples(buffer, 0, bytes);
						writer.Write(buffer, 0, bytes);
					}
					catch(Exception ex)
					{
						GeneralHelpers.WriteLogEntry(ex.ToString(), GeneralHelpers.LogFileType.Exception);
					}
				}

				int AudioSetup(ref IntPtr opaque, ref IntPtr format, ref uint rate, ref uint channels)
				{
					channels = (uint)waveFormat.Channels;
					rate = (uint)waveFormat.SampleRate;
					return 0;
				}

				void DrainAudio(IntPtr data)
				{
					writer.Flush();
				}

				void FlushAudio(IntPtr data, long pts)
				{
					writer.Flush();
					waveProvider.ClearBuffer();
				}

				void ResumeAudio(IntPtr data, long pts)
				{
					WaveOutPlayer.Play();
				}

				void PauseAudio(IntPtr data, long pts)
				{
					WaveOutPlayer.Pause();
				}

				void AudioCleanup(IntPtr opaque)
				{
				}

				#endregion
			}
			catch(Exception ex)
			{
				Running = false;
				GeneralHelpers.WriteLogEntry(ex.ToString(), GeneralHelpers.LogFileType.Exception);
				workSwitch = false;
			}
			finally
			{
				Running = false;
				SetCurrentSongDataToStopped();
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
			var tempSongName = trackData.ArtistName + " - " + trackData.SongName;
			tempSongName = Regex.Replace(tempSongName, @"[^A-Za-z0-9 -\]]", "",
								RegexOptions.CultureInvariant);
			foreach(char c in Path.GetInvalidFileNameChars())
			{
				tempSongName = tempSongName.Replace(c, ' ');
			}

			string mp3Path = folderPath + "\\" + tempSongName + ".mp3";

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
			GeneralHelpers.WriteLogEntry(ex.ToString() + 
                    $"\r\nFilePath:{filePath} trackArtist:{trackData.ArtistName} songname: {trackData.SongName} genere: {trackData.Genre}", 
                    GeneralHelpers.LogFileType.Exception);
		}
		finally
		{
			SaveTrackRunning = false;
		}
	}

	void GrabExtraSongData(ref SongDetailsModel currentModel)
	{
		// Reach out for fan art, album art, alb name, lyrics, etc        
		try
		{
			if(GetTrackDataRunning)
			{
				return;
			}
			GetTrackDataRunning = true;
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
				string backdropPath = App.MySettings.ArtistImageCacheFolder + "\\" + artistID;
				if(!Directory.Exists(backdropPath))
				{
					Directory.CreateDirectory(backdropPath);
				}

				if(Directory.GetFiles(backdropPath).Length > 0)
				{
					currentModel.PathToBackdrops = backdropPath;
					currentModel.HasArtistImagesInLocalFolder = true;
				}
				else
				{
					var fanArt = TrackInfoHelpers.GetFanArtFromFanArt(artistID, "f70d3e1719d159ecc4819187ab742521");
					if(fanArt != null && fanArt.artistbackground.Count > 0)
					{
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
			}
			else
			{
				GeneralHelpers.WriteLogEntry(currentModel.ArtistName, GeneralHelpers.LogFileType.ArtistLookupFailure);
			}
			currentModel.ArtLoaded = true;
			WeakReferenceMessenger.Default.Send(new CurrentStreamStatsChangedMessage(currentModel));
		}
		catch(Exception ex)
		{
			GeneralHelpers.WriteLogEntry(ex.ToString(), GeneralHelpers.LogFileType.Exception);
		}
		finally
		{
			GetTrackDataRunning = false;
		}
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
		return tempPath;
	}

	public void Dispose()
	{
		workSwitch = false;
		Thread.Sleep(1000);// Give it a sec to stop gracefully
		if(WaveOutPlayer != null)
		{
			WaveOutPlayer.Stop();
		}
		if(VlcMediaPlayerShell != null)
		{
			VlcMediaPlayerShell.Stop();
		}
		WaveOutPlayer.Dispose();
		VlcMediaPlayerShell.Dispose();
		Running = false;
	}
}
