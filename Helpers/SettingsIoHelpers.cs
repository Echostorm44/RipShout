using RipShout.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RipShout.Helpers;

public static class SettingsIoHelpers
{
    public static void SaveGeneralSettingsToDisk(SettingsModel settings)
    {
        string fileName = "generalsettings.dat";
        var serialA = JsonSerializer.Serialize<SettingsModel>(settings);
        WriteFile(fileName, serialA);
    }

    public static SettingsModel LoadGeneralSettingsFromDisk()
    {
        string fileName = "generalsettings.dat";
        var settings = GetFileContents(fileName);
        SettingsModel mySettings = new SettingsModel();
        if(string.IsNullOrEmpty(settings))
        {
            string artistImageCache = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Ripshout\\ImageCache\\Artists\\";
            if(!Directory.Exists(artistImageCache))
            {
                Directory.CreateDirectory(artistImageCache);
            }
            string albumImageCache = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Ripshout\\ImageCache\\AlbumCovers\\";
            if(!Directory.Exists(albumImageCache))
            {
                Directory.CreateDirectory(albumImageCache);
            }
            string musicTempFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Ripshout\\Music\\Temp\\";
            if(!Directory.Exists(musicTempFolder))
            {
                Directory.CreateDirectory(musicTempFolder);
            }
            string musicFinalFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Ripshout\\Music\\Finished\\";
            if(!Directory.Exists(musicFinalFolder))
            {
                Directory.CreateDirectory(musicFinalFolder);
            }
            mySettings = new SettingsModel() { SaveFinalMusicToFolder = musicFinalFolder, SaveTempMusicToFolder = musicTempFolder,
                AlbumImageCacheFolder = albumImageCache, ArtistImageCacheFolder = artistImageCache };
            var serialGS = JsonSerializer.Serialize<SettingsModel>(mySettings);
            WriteFile(fileName, serialGS);
        }
        else
        {
            mySettings = JsonSerializer.Deserialize<SettingsModel>(settings) ?? new SettingsModel();
        }
        return mySettings;
    }

    static string GetFileContents(string fileName)
    {
        string basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Ripshout\\";
        if(!File.Exists(basePath + fileName))
        {
            return "";
        }
        else
        {
            var fileText = File.ReadAllText(basePath + fileName);
            return fileText;
        }
    }

    static void WriteFile(string filename, string contents)
    {
        string basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Ripshout\\";
        if(!Directory.Exists(basePath))
        {
            Directory.CreateDirectory(basePath);
        }
        File.WriteAllText(basePath + filename, contents);
    }
}
