using RestSharp;
using RipShout.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RipShout.Helpers;

public static class SettingsIoHelpers
{
    public static Bitmap ResizeImage(Image image, int width, int height)
    {
        var destRect = new Rectangle(0, 0, width, height);
        var destImage = new Bitmap(width, height);

        try
        {
            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using(var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using(var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
        }
        catch(Exception ex)
        {
            GeneralHelpers.WriteLogEntry(ex.ToString(), GeneralHelpers.LogFileType.Exception);
        }

        return destImage;
    }

    public static string SaveURLChannelCover(string id, string url)
    {
        string localFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Ripshout\\ChannelCovers\\";
        if(!Directory.Exists(localFolder))
        {
            Directory.CreateDirectory(localFolder);
        }
        var finalFileName = localFolder + id + ".jpg";
        var client = new RestClient(url);
        var request = new RestRequest();
        request.Method = Method.Get;
        request.Timeout = -1;
        var response = client.DownloadData(request);
        if(response != null && response.Length > 0)
        {
            var bit = Bitmap.FromStream(new MemoryStream(response));
            var fixedBit = ResizeImage(bit, 190, 190);
            fixedBit.Save(finalFileName);
            //File.WriteAllBytes(finalFileName, response);
        }
        else
        {
            return null;
        }
        return finalFileName;
    }

    public static string GetChannelCover(string id, StationFamily fam)
    {
        string localFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Ripshout\\ChannelCovers\\";
        if(!Directory.Exists(localFolder))
        {
            Directory.CreateDirectory(localFolder);
        }
        var finalFileName = localFolder + id + ".jpg";
        if(fam == StationFamily.OneFM)
        {
            var assemblyPath = Assembly.GetExecutingAssembly().Location;
            var root = Path.GetDirectoryName(assemblyPath);
            finalFileName = root + "/Images/OneFmChanCovers/OneFM" + id + ".png";
        }
        if(!File.Exists(finalFileName))
        {
            return null;
        }
        return finalFileName;
    }

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
