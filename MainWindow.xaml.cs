using CommunityToolkit.Mvvm.Messaging;
using LibVLCSharp.Shared;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using RipShout.Helpers;
using RipShout.Models;
using RipShout.Services;
using RipShout.ViewModels;
using RipShout.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Interop.WinDef;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;
using Wpf.Ui.TaskBar;

namespace RipShout;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : INavigationWindow
{
    public ViewModels.MainViewModel ViewModel { get; set; }
    private bool initialized = false;

    public MainWindow(MainViewModel viewModel, INavigationService navigationService,
        IPageService pageService, ISnackbarService snackbarService)
    {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
        navigationService.SetNavigationControl(RootNavigation);
        SetPageService(pageService);
        snackbarService.SetSnackbarControl(RootSnackbar);
    }

    private void InvokeSplashScreen()
    {
        if(initialized)
        {
            return;
        }
        initialized = true;

        // Location has to happen outside constructor
        //if(App.MySettings.LastWindowY >= 0 && App.MySettings.LastWindowX >= 0)
        //{
        //    this.Left = App.MySettings.LastWindowX;
        //    this.Top = App.MySettings.LastWindowY;
        //}

        RootMainGrid.Visibility = Visibility.Collapsed;
        RootWelcomeGrid.Visibility = Visibility.Visible;
        Wpf.Ui.TaskBar.TaskBarProgress.SetValue(this, Wpf.Ui.TaskBar.TaskBarProgressState.Indeterminate, 0);

        Task.Run(async () =>
        {
            // Remember to always include Delays and Sleeps in
            // your applications to be able to charge the client for optimizations later. ;)
            try
            {
                await App.LoadChannels();
                await Dispatcher.InvokeAsync(() =>
                {
                    RootWelcomeGrid.Visibility = Visibility.Hidden;
                    RootMainGrid.Visibility = Visibility.Visible;
                    RootNavigation.Navigate(typeof(StationsPage));
                    Wpf.Ui.TaskBar.TaskBarProgress.SetValue(this, Wpf.Ui.TaskBar.TaskBarProgressState.None, 0);
                });
            }
            catch(Exception ex)
            {
                GeneralHelpers.WriteLogEntry(ex.ToString(), GeneralHelpers.LogFileType.Exception);
            }

            return true;
        });
    }

    public Frame GetFrame()
    {
        return RootFrame;
    }

    public INavigation GetNavigation()
    {
        return RootNavigation;
    }

    public bool Navigate(Type pageType)
    {
        return RootNavigation.Navigate(pageType);
    }

    public void SetPageService(IPageService pageService)
    {
        RootNavigation.PageService = pageService;
    }

    public void ShowWindow()
    {
        Show();
    }

    public void CloseWindow()
    {
        Close();
    }

    private void UiWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        this.SavePlacement();
    }

    private void UiWindow_Loaded(object sender, RoutedEventArgs e)
    {
        InvokeSplashScreen();
        Test();
    }

    #region This was a test to get the audio from the sound card output because DI / RadioTunes doesn't

    // Let you do 2 streams at once on premium accounts && using the custom stream with the overridden Read
    // causes issues with buffering that I simply can't find a way to fix.  This was plan B but it's not
    // great beacuse it's not a direct stream from the source, it's the sound card output which is affected
    // by the volume of the sound card.  I'm leaving this here for now because it's a good example of how to
    // do this if I ever need to do it again.
    //void Test()
    //{
    //    var url = "http://prem4.radiotunes.com:80/poprock?fd1ce46ac9b3ce6b357cb5c1";
    //    Core.Initialize();

    //    using(var libVLC = new LibVLC(enableDebugLogs: true, "--no-video", "--network-caching=4000"))
    //    {
    //        libVLC.Log += (s, e) =>
    //        {
    //            Trace.WriteLine($"[{e.Level}] {e.Module}:{e.Message}");
    //        };
    //        using(var mediaPlayer = new LibVLCSharp.Shared.MediaPlayer(libVLC))
    //        {
    //            mediaPlayer.Volume = 40;
    //            using(var media = new Media(libVLC, new Uri(url)))
    //            {
    //                media.StateChanged += (s, e) =>
    //                {
    //                    Trace.WriteLine($"Media state changed to {e}");
    //                };
    //                mediaPlayer.ChapterChanged += (s, e) =>
    //                {
    //                    Trace.WriteLine($"Chapter changed to {e}");
    //                };
    //                media.MetaChanged += (s, e) =>
    //                {
    //                    Trace.WriteLine($"Meta changed to {e}");
    //                };

    //                media.AddOption(":no-video");
    //                media.AddOption(":network-caching=4000");
    //                mediaPlayer.Media = media;
    //                mediaPlayer.Play();

    //                var outputFolder = "c:\\Test\\";
    //                var outputFilePath = System.IO.Path.Combine(outputFolder, "fontCut.wav");
    //                //var waveIn = new WaveInEvent();
    //                var waveIn = new WasapiLoopbackCapture();

    //                WaveFileWriter writer = new WaveFileWriter(outputFilePath, waveIn.WaveFormat);
    //                waveIn.DataAvailable += (s, a) =>
    //                {
    //                    writer.Write(a.Buffer, 0, a.BytesRecorded);
    //                };
    //                waveIn.StartRecording();

    //                var currentTitle = "";
    //                while(true)
    //                {
    //                    var foo5 = media.Meta(MetadataType.NowPlaying);
    //                    if(!string.IsNullOrEmpty(foo5))
    //                    {
    //                        if(foo5 != currentTitle)
    //                        {
    //                            //waveIn.StopRecording();
    //                            writer.Dispose();
    //                            outputFilePath = System.IO.Path.Combine(outputFolder, foo5 + ".wav");
    //                            writer = new WaveFileWriter(outputFilePath, waveIn.WaveFormat);
    //                            if(!string.IsNullOrEmpty(currentTitle))
    //                            {
    //                                outputFilePath = System.IO.Path.Combine(outputFolder, currentTitle + ".wav");
    //                                var mp3Path = System.IO.Path.Combine(outputFolder, currentTitle + ".mp3");
    //                                try
    //                                {
    //                                    using(var reader = new WaveFileReader(outputFilePath))
    //                                    {
    //                                        MediaFoundationEncoder.EncodeToMp3(reader, mp3Path);
    //                                    }
    //                                }
    //                                catch(Exception ex)
    //                                {
    //                                    var loooo = ex;
    //                                }
    //                            }
    //                            //waveIn.StartRecording();
    //                            currentTitle = foo5;
    //                        }
    //                    }

    //                    Thread.Sleep(500);
    //                }
    //            }
    //        }
    //    }

    //}

    #endregion

    void Test()
    {
        var url = "http://prem4.radiotunes.com:80/the80s?fd1ce46ac9b3ce6b357cb5c1";
        Core.Initialize();

        using(var libVLC = new LibVLC(enableDebugLogs: true, "--no-video", "--network-caching=4000"))
        {
            libVLC.Log += (s, e) =>
            {
                Trace.WriteLine($"[{e.Level}] {e.Module}:{e.Message}");
            };
            using(var mediaPlayer = new LibVLCSharp.Shared.MediaPlayer(libVLC))
            {
                mediaPlayer.Volume = 100;
                using(var media = new Media(libVLC, new Uri(url)))
                {
                    media.AddOption(":no-video");
                    media.AddOption(":network-caching=4000");
                    mediaPlayer.Media = media;

                    var outputFolder = "c:\\Test\\";
                    var outputFilePath = System.IO.Path.Combine(outputFolder, "frontCut.wav");

                    using var outputDevice = new WaveOutEvent();
                    var waveFormat = new WaveFormat(8000, 16, 1);
                    var writer = new WaveFileWriter(outputFilePath, waveFormat);
                    var waveProvider = new BufferedWaveProvider(waveFormat);
                    outputDevice.Init(waveProvider);

                    mediaPlayer.SetAudioFormatCallback(AudioSetup, AudioCleanup);
                    mediaPlayer.SetAudioCallbacks(PlayAudio, PauseAudio, ResumeAudio, FlushAudio, DrainAudio);

                    mediaPlayer.Play();
                    outputDevice.Play();
                    var currentTitle = "frontCut";
                    bool isFrontCut = true;
                    while(true)
                    {
                        Thread.Sleep(500);
                        var foo5 = media.Meta(MetadataType.NowPlaying);
                        if(!string.IsNullOrEmpty(foo5))
                        {
                            if(foo5 != currentTitle)
                            {
                                writer.Flush();
                                waveProvider.ClearBuffer();
                                writer.Dispose();
                                outputFilePath = System.IO.Path.Combine(outputFolder, foo5 + ".wav");
                                writer = new WaveFileWriter(outputFilePath, waveFormat);

                                if(currentTitle != "frontCut")
                                {
                                    outputFilePath = System.IO.Path.Combine(outputFolder, currentTitle + ".wav");
                                    if(isFrontCut)
                                    {
                                        currentTitle = "[FrontCut]" + currentTitle;
                                    }
                                    var mp3Path = System.IO.Path.Combine(outputFolder, currentTitle + ".mp3");
                                    using(var reader = new WaveFileReader(outputFilePath))
                                    {
                                        MediaFoundationEncoder.EncodeToMp3(reader, mp3Path);
                                    }
                                    isFrontCut = false;
                                }
                                currentTitle = foo5;
                            }
                        }
                    }
                    void PlayAudio(IntPtr data, IntPtr samples, uint count, long pts)
                    {
                        int bytes = (int)count * 2; // (16 bit, 1 channel)
                        var buffer = new byte[bytes];
                        Marshal.Copy(samples, buffer, 0, bytes);

                        waveProvider.AddSamples(buffer, 0, bytes);
                        writer.Write(buffer, 0, bytes);
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
                        outputDevice.Play();
                    }

                    void PauseAudio(IntPtr data, long pts)
                    {
                        outputDevice.Pause();
                    }

                    void AudioCleanup(IntPtr opaque)
                    {
                    }
                }
            }
        }
    }

    private void UiWindow_SourceInitialized(object sender, EventArgs e)
    {
        this.ApplyPlacement();
    }
}
