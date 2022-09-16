using Hqub.MusicBrainz.API;
using Hqub.MusicBrainz.API.Entities;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;

namespace RipShout.Helpers;
public static class TrackInfoHelpers
{
    /*
     http://musicbrainz.org/ws/2/recording/?query=artist:%22John%20Travolta%2C%20Olivia%20Newton-John%22+and+recording:%22Summer%20Nights%22&limit=30
     https://itunes.apple.com/search?media=music&entity=song&term=nirvana-polly
    http://webservice.fanart.tv/v3/music/b60527cc-54f3-4bbe-a01b-dcf34c95ae14?api_key=a1da18ae7b743cf897c170678b58d746
    http://musicbrainz.org/ws/2/artist/?query=artist:nirvana&limit=1 
    // fanart uses musicbrains ID
    https://api.discogs.com/database/search?query=nirvana+-+polly&token=dzlteADaCwkHvvgoxQKhfIlXujJIZJuFxeaWselC&per_page=3&format=album&type=master
    lastFM?     */


    public static ScrapeDataDiscogsAlbum? GetTrackInfoFromDiscogs(string artist, string track, string token)
    {
        try
        {
            var result = new ScrapeDataDiscogsAlbum();
            var codedTerm = HttpUtility.UrlEncode(artist + "+-+" + track);
            var client = new RestClient(@"https://api.discogs.com/database/search?query=" + codedTerm + 
                "&token=" + token + "&per_page=3&format=album&type=master");
            var request = new RestRequest();
            request.Method = Method.Get;
            request.Timeout = -1;
            var response = client.Execute(request);
            if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
            {
                var myDeserializedClass = JsonConvert.DeserializeObject<ScrapeDataDiscogsTrackServerResponse>(response.Content);
                if (myDeserializedClass == null || myDeserializedClass.results == null || myDeserializedClass.results.Count == 0)
                {
                    return null;
                }
                return myDeserializedClass.results.First();
            }
        }
        catch (Exception)
        {
            // TODO do something with this
        }
        return null;
    }

    public static ScrapDataItunesTrack? GetTrackInfoFromItunes(string artist, string track)
    {
        try
        {
            var result = new ScrapDataItunesTrack();
            var codedTerm = HttpUtility.UrlEncode(artist + "-" + track);
            var client = new RestClient("https://itunes.apple.com/search?media=music&entity=song&limit=10&term=" + codedTerm);
            var request = new RestRequest();
            request.Method = Method.Get;
            request.Timeout = -1;
            var response = client.Execute(request);
            if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
            {
                var myDeserializedClass = JsonConvert.DeserializeObject<ScrapeDataItuneTrackServerResponse>(response.Content);
                if (myDeserializedClass == null)
                {
                    return null;
                }
                foreach (var item in myDeserializedClass.results.OrderBy(a => a.releaseDate))
                {// TODO we're going to have to fuzzy match this, it's too much for it to be dead on.
                    if (Levenshtein.GetRatioPercent(item.artistName, artist) > 30 &&
                        Levenshtein.GetRatioPercent(item.trackName, track) > 30 &&
                        Levenshtein.GetRatioPercent(item.trackCensoredName, track) > 30)
                    {
                        return item;
                    }

                    //if (item.artistName.JustNumbersAndLettersAndLower() == artist.JustNumbersAndLettersAndLower() && 
                    //    item.trackName.JustNumbersAndLettersAndLower() == track.JustNumbersAndLettersAndLower() && 
                    //    item.trackCensoredName.JustNumbersAndLettersAndLower() == track.JustNumbersAndLettersAndLower())
                    //{
                    //    return item;
                    //}
                }
            }
        }
        catch (Exception)
        {
            // TODO do something with this
        }
        return null;
    }

    public static async Task<string?> GetArtistIdFromMusicBrainz(string band, string album)
    {
        try
        {
            MusicBrainzClient client = new MusicBrainzClient();
            var query = new QueryParameters<ReleaseGroup>()
            {
                { "artist", band },
                { "releasegroup", album },
                { "type", "album" },
                { "status", "official" }
            };
            // Search for an release-group by title.
            var groups = await client.ReleaseGroups.SearchAsync(query);
            if (groups.Items.Count == 0)
            {
                return await GetArtistIdFromMusicBrainz(band);// backup plan
            }
            var releaseGroup = groups.Items.FirstOrDefault(a => Levenshtein.GetRatioPercent(a.Title, album) > 30);
            if (releaseGroup == null)
            {
                return await GetArtistIdFromMusicBrainz(band);// backup plan
            }
            var artistID = releaseGroup.Credits?.FirstOrDefault()?.Artist?.Id;
            return artistID;
        }
        catch (Exception ex)
        {
            // TODO
        }
        return null;
    }

    public static async Task<string?> GetArtistIdFromMusicBrainz(string name)
    {
        try
        {
            MusicBrainzClient client = new MusicBrainzClient();
            var artists = await client.Artists.SearchAsync(name, 20);
            if (artists != null && artists.Count > 0)
            {
                return artists.First().Id;
                //var artist = artists.Items.OrderByDescending(a => Levenshtein.GetRatio(a.Name, name)).First();
                //artist = await client.Artists.GetAsync(artist.Id, "artist-rels", "url-rels");
            }
        }
        catch (Exception ex)
        {
            // TODO
        }
        return null;
    }

    public static ScrapeDataFanArt? GetFanArtFromFanArt(string artistID, string token)
    {
        try
        {
            var result = new ScrapeDataFanArt();
            var client = new RestClient($@"http://webservice.fanart.tv/v3/music/{artistID}?api_key={token}");
            var request = new RestRequest();
            request.Method = Method.Get;
            request.Timeout = -1;
            var response = client.Execute(request);
            if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
            {
                var myDeserializedClass = JsonConvert.DeserializeObject<ScrapeDataFanArt>(response.Content);
                if (myDeserializedClass == null || myDeserializedClass.artistbackground == null || myDeserializedClass.artistbackground.Count == 0)
                {
                    return null;
                }
                return myDeserializedClass;
            }
        }
        catch (Exception)
        {
            // TODO do something with this
        }
        return null;
    }



    #region MusicBrainzDetail
    //public static async Task<string?> GetAlbumAndSongDataFromMusicBrainz(string artist, string album, string song)
    //{
    //    MusicBrainzClient client = new MusicBrainzClient();
    //    // Build an advanced query to search for the recording.
    //    var query = new QueryParameters<Recording>()
    //    {
    //        { "artist", artist },
    //        { "release", album },
    //        { "recording", song }
    //    };

    //    // Search for a recording by title.
    //    var recordings = await client.Recordings.SearchAsync(query);

    //    Console.WriteLine("Total matches for '{0} ({1}) {2}': {3}", artist, album, song, recordings.Count);

    //    // Get exact matches.
    //    var matches = recordings.Items.Where(r => r.Title == song && r.Releases.Any(s => s.Title == album));

    //    // Get the best match (in this case, we use the recording that has the most releases associated).
    //    var recording = matches.OrderByDescending(r => r.Releases.Count).First();

    //    // Get the first official release.
    //    var release = recording.Releases.Where(r => r.Title == album && IsOfficial(r)).OrderBy(r => r.Date).First();

    //    // Get detailed information of the recording, including related works.
    //    recording = await client.Recordings.GetAsync(recording.Id, "work-rels");

    //    if (recording.Relations.Count == 0)
    //    {
    //        return null;
    //    }

    //    // Expect only a single work related to recording.
    //    var work = recording.Relations.First().Work;

    //    // Get detailed information of the work, including related urls.
    //    work = await client.Work.GetAsync(work.Id, "url-rels");

    //    return null;
    //}
    #endregion

    static bool IsOfficial(Release r)
    {
        return !string.IsNullOrEmpty(r.Date) && !string.IsNullOrEmpty(r.Status)
             && r.Status.Equals("Official", StringComparison.OrdinalIgnoreCase);
    }
}

public sealed class ScrapeDataItuneTrackServerResponse
{
    public int resultCount { get; set; }
    public List<ScrapDataItunesTrack> results { get; set; }
}

public sealed class ScrapDataItunesTrack
{
    public string wrapperType { get; set; }
    public string kind { get; set; }
    public int artistId { get; set; }
    public int collectionId { get; set; }
    public int trackId { get; set; }
    public string artistName { get; set; }
    public string collectionName { get; set; }
    public string trackName { get; set; }
    public string collectionCensoredName { get; set; }
    public string trackCensoredName { get; set; }
    public string artistViewUrl { get; set; }
    public string collectionViewUrl { get; set; }
    public string trackViewUrl { get; set; }
    public string previewUrl { get; set; }
    public string artworkUrl30 { get; set; }
    public string artworkUrl60 { get; set; }
    public string artworkUrl100 { get; set; }
    public double collectionPrice { get; set; }
    public double trackPrice { get; set; }
    public DateTime releaseDate { get; set; }
    public string collectionExplicitness { get; set; }
    public string trackExplicitness { get; set; }
    public int discCount { get; set; }
    public int discNumber { get; set; }
    public int trackCount { get; set; }
    public int trackNumber { get; set; }
    public int trackTimeMillis { get; set; }
    public string country { get; set; }
    public string currency { get; set; }
    public string primaryGenreName { get; set; }
    public bool isStreamable { get; set; }

    public ScrapDataItunesTrack()
    {
        
    }
}

public sealed class DiscogsCommunity
{
    public int want { get; set; }
    public int have { get; set; }
}

public sealed class DiscogsPagination
{
    public int page { get; set; }
    public int pages { get; set; }
    public int per_page { get; set; }
    public int items { get; set; }
    public DiscogsPagingUrls urls { get; set; }
}

public sealed class ScrapeDataDiscogsAlbum
{
    public string country { get; set; }
    public string year { get; set; }
    public List<string> format { get; set; }
    public List<string> label { get; set; }
    public string type { get; set; }
    public List<string> genre { get; set; }
    public List<string> style { get; set; }
    public int id { get; set; }
    public List<string> barcode { get; set; }
    public DiscogsUserData user_data { get; set; }
    public int master_id { get; set; }
    public string master_url { get; set; }
    public string uri { get; set; }
    public string catno { get; set; }
    public string title { get; set; }
    public string thumb { get; set; }
    public string cover_image { get; set; }
    public string resource_url { get; set; }
    public DiscogsCommunity community { get; set; }
}

public sealed class ScrapeDataDiscogsTrackServerResponse
{
    public DiscogsPagination pagination { get; set; }
    public List<ScrapeDataDiscogsAlbum> results { get; set; }
}

public sealed class DiscogsPagingUrls
{
    public string last { get; set; }
    public string next { get; set; }
}

public sealed class DiscogsUserData
{
    public bool in_wantlist { get; set; }
    public bool in_collection { get; set; }
}

//public class F7568cc74c093a47Abbc593e7aaac7d6
//{
//    public List<Albumcover> albumcover { get; set; }
//    public List<Cdart> cdart { get; set; }
//}

public class FanArtHdmusiclogo
{
    public string id { get; set; }
    public string url { get; set; }
    public string likes { get; set; }
}

public class FanArtMusicbanner
{
    public string id { get; set; }
    public string url { get; set; }
    public string likes { get; set; }
}

public class FanArtArtistbackground
{
    public string id { get; set; }
    public string url { get; set; }
    public string likes { get; set; }
}

public class FanArtArtistthumb
{
    public string id { get; set; }
    public string url { get; set; }
    public string likes { get; set; }
}

public class Albumcover
{
    public string id { get; set; }
    public string url { get; set; }
    public string likes { get; set; }
}

public class ScrapeDataFanArt
{
    public string name { get; set; }
    public string mbid_id { get; set; }
    public dynamic albums { get; set; }
    public List<FanArtArtistbackground> artistbackground { get; set; }
    public List<FanArtArtistthumb> artistthumb { get; set; }
    public List<FanArtMusicbanner> musicbanner { get; set; }
    public List<FanArtHdmusiclogo> hdmusiclogo { get; set; }
}