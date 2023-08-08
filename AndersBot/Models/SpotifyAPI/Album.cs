namespace AndersBot.Models.SpotifyAPI
{

    public class Album : ISpotifyResult
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
        public Artist1[] artists { get; set; }
        public Tracks tracks { get; set; }

        public string GetTrackName()
        {
            return name;
        }

        public string GetTrackArtists()
        {
            throw new NotImplementedException();
        }
    }

    public class AlbumItem
    {
        public Artist[] artists { get; set; }
        public string[] available_markets { get; set; }
        public int disc_number { get; set; }
        public int duration_ms { get; set; }
        public bool _explicit { get; set; }
        public External_Urls1 external_urls { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public bool is_playable { get; set; }
        public Album_Linked_From linked_from { get; set; }
        public Restrictions1 restrictions { get; set; }
        public string name { get; set; }
        public string preview_url { get; set; }
        public int track_number { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
        public bool is_local { get; set; }
    }

    public class Album_Linked_From
    {
        public External_Urls2 external_urls { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
    }

}
