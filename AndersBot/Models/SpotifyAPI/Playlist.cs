namespace AndersBot.Models.SpotifyAPI
{
    public class Playlist : ISpotifyResult
    {
        public bool collaborative { get; set; }
        public string description { get; set; }
        public External_Urls external_urls { get; set; }
        public Followers followers { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public Image[] images { get; set; }
        public string name { get; set; }
        public Owner owner { get; set; }
        public bool _public { get; set; }
        public string snapshot_id { get; set; }
        public Tracks tracks { get; set; }
        public string type { get; set; }
        public string uri { get; set; }

        public string GetTrackName()
        {
            return name;
        }

        public string GetTrackArtists()
        {
            throw new NotImplementedException();
        }
    }

    public class Owner
    {
        public External_Urls1 external_urls { get; set; }
        public Followers1 followers { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
        public string display_name { get; set; }
    }

    public class Followers1
    {
        public string href { get; set; }
        public int total { get; set; }
    }

    public class Tracks
    {
        public string href { get; set; }
        public int limit { get; set; }
        public string next { get; set; }
        public int offset { get; set; }
        public string previous { get; set; }
        public int total { get; set; }
        public Item[] items { get; set; }
    }

    public class Item
    {
        public string added_at { get; set; }
        public Added_By added_by { get; set; }
        public bool is_local { get; set; }
    }

    public class Added_By
    {
        public External_Urls2 external_urls { get; set; }
        public Followers2 followers { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
    }

    public class Followers2
    {
        public string href { get; set; }
        public int total { get; set; }
    }
}
