using RipShout.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
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

namespace RipShout.Views;

public partial class NowPlayingPage
{
    public NowPlayingViewModel ViewModel { get; }

    public NowPlayingPage(NowPlayingViewModel vm)
    {
        ViewModel = vm;
        this.DataContext = ViewModel;
        InitializeComponent();
    }

    private void UiPage_Loaded(object sender, RoutedEventArgs e)
    {
    }

    private void Mp3Folder_Clicked(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo(App.MySettings.SaveFinalMusicToFolder) { UseShellExecute = true });
    }
}
