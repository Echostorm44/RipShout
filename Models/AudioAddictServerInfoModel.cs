using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RipShout.AudioAddictServerInfo;

// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
public class AdblockWall
{
    public bool? passthru_enabled { get; set; }
    public string? passthru_user_agent_regex { get; set; }
}

public class Advertising
{
    public double web_session_lifetime_minutes { get; set; }
    public string? gpt_midroll_slot_id { get; set; }
    public double midroll_banner_continue_delay { get; set; }
    public double interruptible_track_grace_period { get; set; }
    public double interruptible_track_length { get; set; }
    public double max_ad_frequency { get; set; }
    public double ad_repeat_cap { get; set; }
}

public class AdwordsConversion
{
}

public class Api
{
    public string? urlRoot { get; set; }
    public string? url { get; set; }
}

public class Apps
{
    public string? android_app_id { get; set; }
}

public class AssetPaths
{
    [JsonProperty("fastclick.js")]
    public string? FastclickJs { get; set; }

    [JsonProperty("howler.js")]
    public string? HowlerJs { get; set; }

    [JsonProperty("branch.js")]
    public string? BranchJs { get; set; }
}

public class Base
{
    public string? color { get; set; }
    public string? lineHeight { get; set; }
    public string? padding { get; set; }
    public string? fontSize { get; set; }
    public string? iconColor { get; set; }

    [JsonProperty("::placeholder")]
    public Placeholder Placeholder { get; set; }
}

public class Calendar
{
    public bool? enabled { get; set; }
}

public class Channel
{
    public string? ad_channels { get; set; }
    public string? ad_dfp_unit_id { get; set; }
    public string? channel_director { get; set; }
    public DateTime created_at { get; set; }
    public string? description_long { get; set; }
    public string? description_short { get; set; }
    public int id { get; set; }
    public string? key { get; set; }
    public string? name { get; set; }
    public int network_id { get; set; }
    public int? premium_id { get; set; }
    public bool? @public { get; set; }
    public int? tracklist_server_id { get; set; }
    public DateTime updated_at { get; set; }
    public int asset_id { get; set; }
    public string? asset_url { get; set; }
    public object banner_url { get; set; }
    public string? description { get; set; }
    public List<SimilarChannel> similar_channels { get; set; }
    public List<object> artists { get; set; }
    public Images images { get; set; }
    public bool? favorite { get; set; }
    public int? favorite_position { get; set; }
}

public class ChannelFavorites
{
    public string? favoriteText { get; set; }
    public string? unfavoriteText { get; set; }
    public string? addFavoriteText { get; set; }
    public string? removeFavoriteText { get; set; }
    public string? announceFavoriteTpl { get; set; }
    public string? announceUnfavoriteTpl { get; set; }
}

public class ChannelFilter
{
    public object created_at { get; set; }
    public string? description_text { get; set; }
    public string? description_title { get; set; }
    public bool? display { get; set; }
    public bool? display_description { get; set; }
    public bool? genre { get; set; }
    public int id { get; set; }
    public string? key { get; set; }
    public bool? meta { get; set; }
    public string? name { get; set; }
    public int network_id { get; set; }
    public int position { get; set; }
    public object updated_at { get; set; }
    public List<int> channels { get; set; }
    public Images images { get; set; }
}

public class ChromeExtension
{
    public bool? enabled { get; set; }
    public object store_url { get; set; }
    public object is_desktop_chrome { get; set; }
}

public class Clevertap
{
    public string? accountId { get; set; }
}

public class ContentFormat
{
    public string? extension { get; set; }
    public int id { get; set; }
    public string? key { get; set; }
    public string? mime_type { get; set; }
    public string? name { get; set; }
}

public class ContentQuality
{
    public int id { get; set; }
    public string? key { get; set; }
    public int kilo_bitrate { get; set; }
    public string? name { get; set; }
}

public class CustomMap
{
    public string? dimension1 { get; set; }
}

public class DailyFreeChannels
{
    public object created_at { get; set; }
    public string? description_text { get; set; }
    public string? description_title { get; set; }
    public bool? display { get; set; }
    public bool? display_description { get; set; }
    public bool? genre { get; set; }
    public int id { get; set; }
    public string? key { get; set; }
    public bool? meta { get; set; }
    public string? name { get; set; }
    public int network_id { get; set; }
    public int position { get; set; }
    public object updated_at { get; set; }
    public List<int> channels { get; set; }
    public Images images { get; set; }
}

public class ElementTheme
{
    public Base @base { get; set; }
    public Invalid invalid { get; set; }
}

public class Email
{
    public string? legal { get; set; }
    public string? abuse { get; set; }
    public string? support { get; set; }
    public string? billing { get; set; }
}

public class ExternalPlayers
{
    public bool? enabled { get; set; }
}

public class Facebook
{
    public string? app_id { get; set; }
    public string? sdk_url { get; set; }
    public string? version { get; set; }
    public bool? xfbml { get; set; }
    public bool? status { get; set; }
    public bool? cookie { get; set; }
}

public class Favorite
{
    public int channel_id { get; set; }
    public int position { get; set; }
}

public class FeatureEvents
{
    public bool? first_time_skip { get; set; }
    public bool? first_time_votedown { get; set; }
    public bool? first_time_live_show_skip { get; set; }
}

public class Firebase
{
    public string? sdk { get; set; }
    public string? apiKey { get; set; }
    public string? authDomain { get; set; }
    public string? databaseURL { get; set; }
    public string? projectId { get; set; }
    public string? storageBucket { get; set; }
    public string? messagingSenderId { get; set; }
}

public class GoogleGtagConfig
{
    public bool? anonymize_ip { get; set; }
    public bool? send_page_view { get; set; }
    public CustomMap custom_map { get; set; }
    public string? member_type { get; set; }
    public string? transport { get; set; }
    public int user_id { get; set; }
}

public class HomepageMosaicFilterBar
{
    public bool? enabled { get; set; }
}

public class Images
{
    public string? @default { get; set; }
}

public class Instructions
{
    public HomepageMosaicFilterBar homepageMosaicFilterBar { get; set; }
    public WebplayerFavoriteButton webplayerFavoriteButton { get; set; }
}

public class Invalid
{
    public string? color { get; set; }
    public string? iconColor { get; set; }
}

public class Modals
{
    public string? defaultBackgroundImage { get; set; }
}

public class Network
{
    public bool? active { get; set; }
    public DateTime created_at { get; set; }
    public object description { get; set; }
    public int id { get; set; }
    public string? key { get; set; }
    public string? name { get; set; }
    public DateTime updated_at { get; set; }
    public string? url { get; set; }
    public string? listen_url { get; set; }
    public string? service_key { get; set; }
    public int active_channel_count { get; set; }
}

public class Placeholder
{
    public string? color { get; set; }
}

public class Playlists
{
    public bool? enabled { get; set; }
}

public class Recaptcha
{
    public string? sitekey { get; set; }
    public object theme { get; set; }
}

public class RequestEnv
{
    public string? ip { get; set; }
    public string? countryName { get; set; }
    public string? countryCode { get; set; }
    public string? date { get; set; }
}

public class Root
{
    public string? appVersion { get; set; }
    public DateTime appDeployTime { get; set; }
    public List<Network> networks { get; set; }
    public List<Channel> channels { get; set; }
    public ChannelFavorites channelFavorites { get; set; }
    public List<ChannelFilter> channel_filters { get; set; }
    public DailyFreeChannels daily_free_channels { get; set; }
    public List<object> banners { get; set; }
    public Calendar calendar { get; set; }
    public List<StreamSetConfig> stream_set_configs { get; set; }
    public RequestEnv requestEnv { get; set; }
    public string? currentUserType { get; set; }
    public User? user { get; set; }
    public Api api { get; set; }
    public string? network_key { get; set; }
    public string? network_name { get; set; }
    public string? network_description { get; set; }
    public string? service_key { get; set; }
    public string? service_name { get; set; }
    public string? twitterId { get; set; }
    public string? listen_url { get; set; }
    public object registration_wall { get; set; }
    public Facebook facebook { get; set; }
    public Apps apps { get; set; }
    public List<object> messages { get; set; }
    public Advertising advertising { get; set; }
    public AdblockWall adblock_wall { get; set; }
    public object crowdfunding { get; set; }
    public bool? performance_reporting { get; set; }
    public AssetPaths asset_paths { get; set; }
    public Firebase firebase { get; set; }
    public bool? mobile_webview { get; set; }
    public Playlists playlists { get; set; }
    //public Tracking tracking { get; set; }
    public GoogleGtagConfig google_gtag_config { get; set; }
    public AdwordsConversion adwords_conversion { get; set; }
    public ExternalPlayers external_players { get; set; }
    public Stripe stripe { get; set; }
    public Recaptcha recaptcha { get; set; }
    public Clevertap clevertap { get; set; }
    public Modals modals { get; set; }
    public Email email { get; set; }
    public object discord_url { get; set; }
    public Instructions instructions { get; set; }
    public ChromeExtension chrome_extension { get; set; }
}

public class SimilarChannel
{
    public int id { get; set; }
    public int similar_channel_id { get; set; }
}

public class StreamSet
{
    public string? description { get; set; }
    public int id { get; set; }
    public string? key { get; set; }
    public string? name { get; set; }
    public int network_id { get; set; }
}

public class StreamSetConfig
{
    public string? extension { get; set; }
    public string? format_label { get; set; }
    public string? format_name { get; set; }
    public int id { get; set; }
    public string? label { get; set; }
    public bool? premium { get; set; }
    public bool? @public { get; set; }
    public string? quality_name { get; set; }
    public bool? visible { get; set; }
    public StreamSet stream_set { get; set; }
    public ContentFormat content_format { get; set; }
    public ContentQuality content_quality { get; set; }
}

public class Stripe
{
    public string? public_key { get; set; }
    public string? api_version { get; set; }
    public ElementTheme element_theme { get; set; }
}

public class Tracking
{
    public string? google_conversion_id { get; set; }
    public string? google_analytics_property_id { get; set; }
    public bool? google_analytics { get; set; }
    public bool? adroll_adv_id { get; set; }
    public string? adroll_pix_id { get; set; }
    public string? adroll_conversion_id { get; set; }
    public bool? facebook_enabled { get; set; }
    public string? facebook_pixel_id { get; set; }
    public bool? adwords_enabled { get; set; }
    public string? adwords_conversion_id { get; set; }
    public string? adwords_created_account { get; set; }
    public string? adwords_premium_1_month { get; set; }
    public string? adwords_premium_1_year { get; set; }
    public string? adwords_premium_2_year { get; set; }
    public string? adwords_started_trial { get; set; }
    public bool? bing_enabled { get; set; }
    public string? bing_tracking_id { get; set; }
    public bool? error_reporting_enabled { get; set; }
    public bool? adroll_enabled { get; set; }
    public bool? adwords_remarketing_enabled { get; set; }
    public bool? ga_ecommerce_enabled { get; set; }
    public bool? triton_enabled { get; set; }
    public string? triton_id { get; set; }
    public string? adroll_registration_conversion_id { get; set; }
    public string? adroll_trial_conversion_id { get; set; }
    public bool? branch_enabled { get; set; }
    public string? branch_key { get; set; }
    public string? google_tag_manager_container_id { get; set; }
    public bool? google_tag_manager_enabled { get; set; }
}

public class User
{
    public bool? authenticated { get; set; }
    public bool? hasPremium { get; set; }
    public bool? trialAvailable { get; set; }
    public DateTime? created_at { get; set; }
    public string? audio_token { get; set; }
    public string? session_key { get; set; }
    public bool? hasPassword { get; set; }
    public FeatureEvents feature_events { get; set; }
    public string? first_name { get; set; }
    public string? last_name { get; set; }
    public string? company_name { get; set; }
    public string? vat_number { get; set; }
    public string? email { get; set; }
    public bool? mostlyPremium { get; set; }
    public int? id { get; set; }
    public string? api_key { get; set; }
    public string? listen_key { get; set; }
    public List<Favorite>? favorites { get; set; }
    public bool? isBillable { get; set; }
    public object? processingPaymentId { get; set; }
}

public class WebplayerFavoriteButton
{
    public bool? enabled { get; set; }
}


