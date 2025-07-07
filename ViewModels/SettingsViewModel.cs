using CommunityToolkit.Mvvm.ComponentModel;
using RipShout.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;

namespace RipShout.ViewModels;

public class SettingsViewModel
{
    public SettingsModel MySettings { get; set; }

    public SettingsViewModel()
    {
        MySettings = App.MySettings;
    }
}
