using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RipShout.Models;


public class ChannelModel : INotifyPropertyChanged
{
    public string ID { get; set; }//Name+;Family; Gen Helper method to set this which should probably be here now that I think about it
    public string Name { get; set; }
    public string Description { get; set; }
    public string ImageURL { get; set; }
    public string PrimaryURL { get; set; }
    public string BackupURL { get; set; }
    public StationFamily Family { get; set; }
    bool isFavorite;
    public bool IsFavorite
    {
        get => isFavorite;
        set
        {
            if (isFavorite == value)
            {
                return;
            }

            isFavorite = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsFavorite)));
        }
    }
    bool isVisible;
    public bool IsVisible
    {
        get => isVisible;
        set
        {
            if (isVisible == value)
            {
                return;
            }

            isVisible = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsVisible)));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
}

public enum StationFamily
{
    DI,
    RadioTunes,
    ZenRadio,
    JazzRadio,
    ClassicalRadio,
    RockRadio,
    OneFM,
    None,
}