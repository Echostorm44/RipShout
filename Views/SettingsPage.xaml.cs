using RipShout.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
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
using Wpf.Ui.Common;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Interfaces;
using Wpf.Ui.Mvvm.Services;

namespace RipShout.Views;

/// <summary>
/// Interaction logic for SettingsPage.xaml
/// </summary>
public partial class SettingsPage
{
    private readonly ISnackbarService SnackbarService;
    public SettingsViewModel ViewModel { get; set; }

    public SettingsPage(ISnackbarService snackbarService, SettingsViewModel vm)
    {
        ViewModel = vm;
        this.DataContext = ViewModel;
        InitializeComponent();
        SnackbarService = snackbarService;
    }

    private void UiPage_Loaded(object sender, RoutedEventArgs e)
    {
    }

    private void UiPage_Unloaded(object sender, RoutedEventArgs e)
    {
    }

    private void SetFinalMusicPath_Click(object sender, RoutedEventArgs e)
    {
        var foo = new System.Windows.Forms.FolderBrowserDialog();
        var symFolderDialog = foo.ShowDialog();
        if(symFolderDialog == System.Windows.Forms.DialogResult.OK)
        {
            ViewModel.MySettings.SaveFinalMusicToFolder = foo.SelectedPath;
            SnackbarService.Show("Saved!", "Settings Updated", SymbolRegular.Save24);
        }
    }

    private void SetTempMusicPath_Click(object sender, RoutedEventArgs e)
    {
        var foo = new System.Windows.Forms.FolderBrowserDialog();
        var symFolderDialog = foo.ShowDialog();
        if(symFolderDialog == System.Windows.Forms.DialogResult.OK)
        {
            ViewModel.MySettings.SaveTempMusicToFolder = foo.SelectedPath;
            SnackbarService.Show("Saved!", "Settings Updated", SymbolRegular.Save24);
        }
    }

    private void SetAlbumImageCachePath_Click(object sender, RoutedEventArgs e)
    {
        var foo = new System.Windows.Forms.FolderBrowserDialog();
        var symFolderDialog = foo.ShowDialog();
        if(symFolderDialog == System.Windows.Forms.DialogResult.OK)
        {
            ViewModel.MySettings.AlbumImageCacheFolder = foo.SelectedPath;
            SnackbarService.Show("Saved!", "Settings Updated", SymbolRegular.Save24);
        }
    }

    private void SetArtistImageCachePath_Click(object sender, RoutedEventArgs e)
    {
        var foo = new System.Windows.Forms.FolderBrowserDialog();
        var symFolderDialog = foo.ShowDialog();
        if(symFolderDialog == System.Windows.Forms.DialogResult.OK)
        {
            ViewModel.MySettings.ArtistImageCacheFolder = foo.SelectedPath;
            SnackbarService.Show("Saved!", "Settings Updated", SymbolRegular.Save24);
        }
    }
}
