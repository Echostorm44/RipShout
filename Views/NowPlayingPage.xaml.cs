using RipShout.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
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

/// <summary>
/// Interaction logic for NowPlayingPage.xaml
/// </summary>
public partial class NowPlayingPage : Page
{
    public NowPlayingViewModel ViewModel
    {
        get;
    }
    public NowPlayingPage()
    {
        InitializeComponent();
        ViewModel = new NowPlayingViewModel();
        this.DataContext = ViewModel;
    }
}
