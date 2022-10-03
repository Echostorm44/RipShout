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
using System.Reflection;
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
                                imageURL = System.IO.Path.Combine(Assembly.GetExecutingAssembly().Location, "/Images/OneFMDefault.png");
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