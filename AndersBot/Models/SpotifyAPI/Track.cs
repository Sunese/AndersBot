namespace AndersBot.Models.SpotifyAPI
{
    public class Track : ISpotifyResult
    {
        public Album_tracksearch AlbumTracksearch { get; set; }
        public Artist1[] artists { get; set; }
        public string[] available_markets { get; set; }
        public int disc_number { get; set; }
        public int duration_ms { get; set; }
        public bool _explicit { get; set; }
        public External_Ids1 external_ids { get; set; }
        public External_Urls2 external_urls { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public bool is_playable { get; set; }
        public Linked_From linked_from { get; set; }
        public Restrictions1 restrictions { get; set; }
        public string name { get; set; }
        public int popularity { get; set; }
        public string preview_url { get; set; }
        public int track_number { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
        public bool is_local { get; set; }
        
        public string GetTrackName()
        {
            return name;
        }

        public string GetTrackArtists()
        {
            var artistsString = "";
            foreach (var artist in artists)
            {
                artistsString = artistsString + " " + artist.name;
            }
            return artistsString;
        }
    }

    public class Album_tracksearch
    {
        public string album_type { get; set; }
        public int total_tracks { get; set; }
        public string[] available_markets { get; set; }
        public External_Urls external_urls { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public Image[] images { get; set; }
        public string name { get; set; }
        public string release_date { get; set; }
        public string release_date_precision { get; set; }
        public Restrictions restrictions { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
        public Copyright[] copyrights { get; set; }
        public External_Ids external_ids { get; set; }
        public string[] genres { get; set; }
        public string label { get; set; }
        public int popularity { get; set; }
        public string album_group { get; set; }
        public Artist[] artists { get; set; }
    }

    public class External_Urls
    {
        public string spotify { get; set; }
    }

    public class Restrictions
    {
        public string reason { get; set; }
    }

    public class External_Ids
    {
        public string isrc { get; set; }
        public string ean { get; set; }
        public string upc { get; set; }
    }

    public class Image
    {
        public string url { get; set; }
        public int height { get; set; }
        public int width { get; set; }
    }

    public class Copyright
    {
        public string text { get; set; }
        public string type { get; set; }
    }

    public class Artist
    {
        public External_Urls1 external_urls { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
    }

    public class External_Urls1
    {
        public string spotify { get; set; }
    }

    public class External_Ids1
    {
        public string isrc { get; set; }
        public string ean { get; set; }
        public string upc { get; set; }
    }

    public class External_Urls2
    {
        public string spotify { get; set; }
    }

    public class Linked_From
    {
    }

    public class Restrictions1
    {
        public string reason { get; set; }
    }

    public class Artist1
    {
        public External_Urls3 external_urls { get; set; }
        public Followers followers { get; set; }
        public string[] genres { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public Image1[] images { get; set; }
        public string name { get; set; }
        public int popularity { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
    }

    public class External_Urls3
    {
        public string spotify { get; set; }
    }

    public class Followers
    {
        public string href { get; set; }
        public int total { get; set; }
    }

    public class Image1
    {
        public string url { get; set; }
        public int height { get; set; }
        public int width { get; set; }
    }
}
