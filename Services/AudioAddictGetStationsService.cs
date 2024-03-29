﻿using Newtonsoft.Json;
using RipShout.Helpers;
using RipShout.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
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

        if(getDI)
        {
            stationsToGet.Add("di");
        }
        if(getRt)
        {
            stationsToGet.Add("radiotunes");
        }
        if(getJazz)
        {
            stationsToGet.Add("jazzradio");
        }
        if(getRock)
        {
            stationsToGet.Add("rockradio");
        }
        if(getZen)
        {
            stationsToGet.Add("zenradio");
        }
        if(getClassical)
        {
            stationsToGet.Add("classicalradio");
        }
        using(HttpClient client = new HttpClient())
        {
            foreach(var stat in stationsToGet)
            {
                StationFamily fam = StationFamily.DI;
                if(stat == "di")
                {
                    fam = StationFamily.DI;
                }
                else if(stat == "radiotunes")
                {
                    fam = StationFamily.RadioTunes;
                }
                else if(stat == "jazzradio")
                {
                    fam = StationFamily.JazzRadio;
                }
                else if(stat == "rockradio")
                {
                    fam = StationFamily.RockRadio;
                }
                else if(stat == "zenradio")
                {
                    fam = StationFamily.ZenRadio;
                }
                else if(stat == "classicalradio")
                {
                    fam = StationFamily.ClassicalRadio;
                }

                var statChansFromDiskCache = SettingsIoHelpers.LoadStationChannelCacheFromDisk(stat);
                if(statChansFromDiskCache.Count > 0)
                {
                    results.AddRange(statChansFromDiskCache);
                    continue;
                }

                using(var request = new HttpRequestMessage())
                {
                    request.RequestUri = new Uri($@"https://api.audioaddict.com/v1/{stat}/channels");
                    var singleStatChanList = new List<ChannelModel>();
                    request.Method = HttpMethod.Get;
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/109.0");
                    await client.SendAsync(request, HttpCompletionOption.ResponseContentRead).ContinueWith(async (tm) =>
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
                                    if(chan.ImageURL == null)
                                    {
                                        chan.ImageURL = System.IO.Path.Combine(Assembly.GetExecutingAssembly().Location, "/Images/DefaultChannelCover.png");
                                    }
                                }
                                results.Add(chan);
                                singleStatChanList.Add(chan);
                            }
                            if(singleStatChanList.Count > 0)
                            {
                                SettingsIoHelpers.SaveStationChannelCacheToDisk(stat, singleStatChanList);
                            }
                        }
                        else
                        {
                            results.AddRange(SettingsIoHelpers.LoadStationChannelCacheFromDisk(stat));
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


