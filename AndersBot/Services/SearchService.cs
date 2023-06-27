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

    public SearchService(LavaNode lavaNode, ILogger<ISearchService> logger)
    {
        _lavaNode = lavaNode;
        _logger = logger;
    }

    public async Task<SearchResponse> Search(SocketInteractionContext ctx, string query)
    {
        var searchType = query.Substring(0, 4).Equals("http") ? SearchType.Direct : SearchType.YouTube;

        if (query.Contains("spotify"))
        {
            await ctx.Interaction.ModifyOriginalResponseAsync(msg =>
                msg.Content = $"Looks like you tried to queue a Spotify track or playlist. This is not supported right neow sowwy {peepoDown}");
        }

        var searchResult = await _lavaNode.SearchAsync(searchType, query);

        return searchResult;
    }

}