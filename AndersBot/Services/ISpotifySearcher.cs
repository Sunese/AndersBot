namespace AndersBot.Services
{
    public interface ISpotifySearcher
    {
        public Task<string> GetTrackNameAndArtistFromSpotifyUrl(string spotifyUrl);
    }
}
