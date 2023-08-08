using System.Net.Sockets;
using Discord;
using Discord.Interactions;
using Victoria.Node;
using Victoria.Player;
using Victoria.Responses.Search;
using static AndersBot.Mentorhold8;


namespace AndersBot.Services;

public class SearchService : ISearchService
{
    private readonly LavaNode _lavaNode;
    private readonly ILogger<ISearchService> _logger;
    private readonly ISpotifySearcher _spotifySearcher;

    public SearchService(
        LavaNode lavaNode, 
        ILogger<ISearchService> logger,
        ISpotifySearcher spotifySearcher)
    {
        _lavaNode = lavaNode;
        _logger = logger;
        _spotifySearcher = spotifySearcher;
    }

    public async Task<SearchResponse> Search(SocketInteractionContext ctx, string query)
    {
        // Default to YT
        SearchType searchType = SearchType.YouTube;

        // If spotify link, get track name and search on YT
        if (IsSpotifyUrl(query))
        {
            query = await _spotifySearcher.GetTrackNameAndArtistFromSpotifyUrl(query);
            searchType = SearchType.YouTube;
        }
        else if (QueryIsUrl(query)) searchType = SearchType.Direct;

        var searchResult = await _lavaNode.SearchAsync(searchType, query);

        return searchResult;
    }

    private bool QueryIsUrl(string query)
    {
        return 
            query.Length >= 8 
            &&
            query[..8].Equals("https://");
    }

    private bool IsSpotifyUrl(string url)
    {
        return 
            url.Length >= 25 
            &&
            url[..25].Equals("https://open.spotify.com/");
    }

}