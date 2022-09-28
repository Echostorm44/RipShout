using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RipShout.Models;


public class ChannelModel
{
    public string ID { get; set; }//Name+;Family; Gen Helper method to set this which should probably be here now that I think about it
    public string Name { get; set; }
    public string Description { get; set; }
    public string ImageURL { get; set; }
    public string PrimaryURL { get; set; }
    public string BackupURL { get; set; }
    public StationFamily Family { get; set; }
    public bool IsFavorite { get; set; }
    public bool IsVisible { get; set; }
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