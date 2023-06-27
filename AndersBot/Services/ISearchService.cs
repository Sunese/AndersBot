using Discord.Interactions;
using Victoria.Responses.Search;

namespace AndersBot.Services;

public interface ISearchService
{
    public Task<SearchResponse> Search(SocketInteractionContext ctx, string query);
}