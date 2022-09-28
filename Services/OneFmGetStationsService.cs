using NAudio.Utils;
using Newtonsoft.Json;
using RipShout.Helpers;
using RipShout.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using static System.Windows.Forms.AxHost;

namespace RipShout.OneFmChannelServices;

public static class OneFmGetStationsService
{
    public static async Task<List<ChannelModel>> GetChannelsAsync(List<string> favs)
    {
        var channels = new List<ChannelModel>();

        using(HttpClient client = new HttpClient())
        {
            using(var request = new HttpRequestMessage())
            {
                request.RequestUri = new Uri(@"https://www.1.fm/mainstations");
                request.Method = HttpMethod.Get;
                await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ContinueWith(async (tm) =>
                {
                    var response = await tm;
                    if(response.IsSuccessStatusCode)
                    {
                        var rawJson = await response.Content.ReadAsStringAsync();
                        var chans = JsonConvert.DeserializeObject<List<Root>>(rawJson);
                        foreach(var item in chans)
                        {
                            var chan = new ChannelModel();
                            chan.Name = item.name;
                            chan.Description = item.long_desc;
                            chan.PrimaryURL = $@"http://{item.strm128kmp3}";
                            chan.BackupURL = $@"http://{item.webstream}";
                            chan.Family = StationFamily.OneFM;
                            chan.ID = GeneralHelpers.SetChannelID(chan);
                            chan.IsFavorite = favs.Contains(chan.ID);
                            chan.IsVisible = true;

                            var imageURL = SettingsIoHelpers.GetChannelCover(item.stid, StationFamily.OneFM);
                            if(string.IsNullOrEmpty(imageURL))
                            {
                                // reach out && download the covers
                                var gotEm = DownloadChannelImagesToCache().Result;
                                imageURL = SettingsIoHelpers.GetChannelCover(item.stid, StationFamily.OneFM);
                            }
                            chan.ImageURL = imageURL;
                            channels.Add(chan);
                        }
                    }
                });
            }
        }
        return channels;
    }

    static async Task<bool> DownloadChannelImagesToCache()
    {
        HttpClient client = new HttpClient();
        var request = new HttpRequestMessage()
        {
            RequestUri = new Uri("https://www.1.fm/less/style.css"), //new Uri("https://www.radiotunes.com/"),
            Method = HttpMethod.Get,
        };

        await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ContinueWith((tm) =>
        {
            var response = tm.Result;
            var rooo = response.Content.ReadAsStringAsync().Result;
            var breakUp = rooo.Split(".svg-");
            foreach(var line in breakUp)
            {
                if(line.Contains(";base64,"))
                {
                    var splitLine = line.Split(@"background-image:url(data:image/svg+xml;base64,");
                    if(splitLine.Length == 2)
                    {
                        var tempkey = splitLine[0];
                        var splitOnForKey = "{height:";
                        if(!tempkey.Contains(splitOnForKey))
                        {
                            splitOnForKey = "{";
                        }
                        var splitTempKey = tempkey.Split(splitOnForKey);
                        var key = splitTempKey[0];
                        var tempSvg = "";
                        int indexOfEnd = splitLine[1].IndexOf(")}");
                        if(indexOfEnd >= 0)
                        {
                            tempSvg = splitLine[1].Remove(indexOfEnd);
                        }
                        var svg = Encoding.UTF8.GetString(Convert.FromBase64String(tempSvg));
                        var xml = new System.Xml.XmlDocument();
                        xml.LoadXml(svg);
                        var moo = Svg.SvgDocument.Open(xml);
                        var biggest = Math.Max(moo.Width, moo.Height);
                        moo.Width = biggest;
                        moo.Height = biggest;
                        var loo = moo.Draw();
                        string localFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Ripshout\\ChannelCovers\\";
                        if(!Directory.Exists(localFolder))
                        {
                            Directory.CreateDirectory(localFolder);
                        }
                        if(loo == null)
                        {
                            var defImg = Bitmap.FromFile(System.AppDomain.CurrentDomain.BaseDirectory + "/Images/DefaultBackdrop.png");
                            defImg.Save(localFolder + "OneFM" + key + ".jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                        }
                        else
                        {
                            var fixedBit = SettingsIoHelpers.ResizeImage(loo, 190, 190);
                            fixedBit.Save(localFolder + "OneFM" + key + ".jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                        }
                    }
                }
                else
                {
                    var splitLine = line.Split(@"background-image:url(data:image/svg+xml,");
                    if(splitLine.Length == 2)
                    {
                        var tempkey = splitLine[0];
                        var splitTempKey = tempkey.Split("{height:");
                        var key = splitTempKey[0];
                        var svg = WebUtility.UrlDecode(splitLine[1]);
                        int indexOfEnd = svg.IndexOf(")}");
                        if(indexOfEnd >= 0)
                        {
                            svg = svg.Remove(indexOfEnd);
                        }
                        var xml = new System.Xml.XmlDocument();
                        xml.LoadXml(svg);
                        var moo = Svg.SvgDocument.Open(xml);
                        var biggest = Math.Max(moo.Width, moo.Height);
                        moo.Width = biggest;
                        moo.Height = biggest;
                        var loo = moo.Draw();
                        string localFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Ripshout\\ChannelCovers\\";
                        if(!Directory.Exists(localFolder))
                        {
                            Directory.CreateDirectory(localFolder);
                        }
                        var fixedBit = SettingsIoHelpers.ResizeImage(loo, 190, 190);

                        fixedBit.Save(localFolder + "OneFM" + key + ".jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                    }
                }
            }
        });
        return true;
    }
}

// Root myDeserializedClass = JsonConvert.DeserializeObject<List<Root>>(myJsonResponse);
public class Root
{
    public string stid { get; set; }//id
    public bool active { get; set; }
    public string srvtype { get; set; }
    public string name { get; set; }// name
    public string webstream { get; set; }
    public string strm32kmp3 { get; set; }
    public string strm64kmp3 { get; set; }
    public string strm128kmp3 { get; set; }// this one
    public string _32kmp3 { get; set; }
    public string _64kmp3 { get; set; }
    public string _128kmp3 { get; set; }
    public string _56kmp3 { get; set; }
    public string long_desc { get; set; }// this. short is empty
    public string short_desc { get; set; }
    public string relatedstations { get; set; }
    public string id { get; set; }
}