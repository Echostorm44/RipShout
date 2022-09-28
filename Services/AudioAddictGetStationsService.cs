using Newtonsoft.Json;
using RipShout.Helpers;
using RipShout.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RipShout.AudioAddictChannelServices;

public static class AudioAddictGetChannelsService
{
    static async Task<(string Primary, string Backup)> GetStreamingPrefixesAsync()
    {// it will  return different subdomains per geographic region
        string primary = "prem1";
        string backup = "prem4";

        HttpClient client = new HttpClient();
        var request = new HttpRequestMessage()
        {
            RequestUri = new Uri("http://listen.di.fm/premium/trance.pls"),
            Method = HttpMethod.Get,
        };

        await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ContinueWith(async (tm) =>
        {
            var response = await tm;
            if(response.IsSuccessStatusCode)
            {
                var raw = await response.Content.ReadAsStringAsync();
                var regPrefs = new Regex(@"http://(.+)?.di.fm", RegexOptions.Multiline);
                var prefMatchs = regPrefs.Matches(raw);
                var primaryDone = false;
                foreach(Match prefMatch in prefMatchs)
                {
                    if(prefMatch.Groups != null && prefMatch.Groups.Count > 1)
                    {
                        if(!primaryDone)
                        {
                            primary = prefMatch.Groups[1].Value;
                            primaryDone = true;
                            continue;
                        }
                        backup = primary = prefMatch.Groups[1].Value;
                    }
                }
            }
        });

        return (primary, backup);
    }


    public static async Task<List<ChannelModel>> GetChannelsAsync(string listenKey, bool getDI, bool getRt, bool getJazz, bool getRock, bool getZen, bool getClassical, List<string> favs)
    {
        // prem 1 3 4 USA prem2 Europe
        //https://api.audioaddict.com/v1/di/channels
        var results = new List<ChannelModel>();

        var prefixes = await GetStreamingPrefixesAsync();
        var stationsToGet = new List<string>();
        StationFamily fam = StationFamily.DI;
        if(getDI)
        {
            stationsToGet.Add("di");
            fam = StationFamily.DI;
        }
        if(getRt)
        {
            stationsToGet.Add("radiotunes");
            fam = StationFamily.RadioTunes;
        }
        if(getJazz)
        {
            stationsToGet.Add("jazzradio");
            fam = StationFamily.JazzRadio;
        }
        if(getRock)
        {
            stationsToGet.Add("rockradio");
            fam = StationFamily.RockRadio;
        }
        if(getZen)
        {
            stationsToGet.Add("zenradio");
            fam = StationFamily.ZenRadio;
        }
        if(getClassical)
        {
            stationsToGet.Add("classicalradio");
            fam = StationFamily.ClassicalRadio;
        }
        using(HttpClient client = new HttpClient())
        {
            foreach(var stat in stationsToGet)
            {
                using(var request = new HttpRequestMessage())
                {
                    request.RequestUri = new Uri($@"https://api.audioaddict.com/v1/{stat}/channels");
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
                                chan.Description = item.description;
                                string topDomain = "com";
                                if(stat == "di")
                                {
                                    topDomain = "fm";
                                }
                                chan.PrimaryURL = $@"http://{prefixes.Primary}.{stat}.{topDomain}:80/{item.key}?{listenKey}";
                                chan.BackupURL = $@"http://{prefixes.Backup}.{stat}.{topDomain}:80/{item.key}?{listenKey}";
                                chan.Family = fam;
                                chan.ID = GeneralHelpers.SetChannelID(chan);
                                chan.IsFavorite = favs.Contains(chan.ID);
                                chan.IsVisible = true;
                                chan.ImageURL = SettingsIoHelpers.GetChannelCover(chan.ID, fam);
                                if(string.IsNullOrEmpty(chan.ImageURL))
                                {
                                    SettingsIoHelpers.SaveURLChannelCover(chan.ID, "http:" + item.asset_url);
                                    chan.ImageURL = SettingsIoHelpers.GetChannelCover(chan.ID, fam);
                                }
                                results.Add(chan);
                            }
                        }
                    });
                }
            }
        }
        return results;
    }
}

// Root myDeserializedClass = JsonConvert.DeserializeObject<List<Root>>(myJsonResponse);
public class Artist
{
    public int id { get; set; }
    public string name { get; set; }
    public string asset_url { get; set; }
    public Images images { get; set; }
}

public class Images
{
    public string @default { get; set; }
    public string horizontal_banner { get; set; }
    public string tall_banner { get; set; }
    public string vertical { get; set; }
    public string square { get; set; }
    public string compact { get; set; }
}

public class Root
{
    public string ad_channels { get; set; }
    public string ad_dfp_unit_id { get; set; }
    public string channel_director { get; set; }
    public DateTime created_at { get; set; }
    public string description_long { get; set; }
    public string description_short { get; set; }
    public int id { get; set; }
    public string key { get; set; }
    public string name { get; set; }
    public int network_id { get; set; }
    public int? premium_id { get; set; }
    public bool @public { get; set; }
    public int? tracklist_server_id { get; set; }
    public DateTime updated_at { get; set; }
    public int? asset_id { get; set; }
    public string asset_url { get; set; }
    public string banner_url { get; set; }
    public string description { get; set; }
    public List<SimilarChannel> similar_channels { get; set; }
    public List<Artist> artists { get; set; }
    public Images images { get; set; }
}

public class SimilarChannel
{
    public int id { get; set; }
    public int similar_channel_id { get; set; }
}


