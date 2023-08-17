using System.Net.Http.Headers;
using System.Web;
using AndersBot.Models.SpotifyAPI;
using Microsoft.Extensions.Options;

namespace AndersBot.Services;

public class SpotifySearcher : ISpotifySearcher
{
    private readonly HttpClient _httpClient;
    private readonly IOptions<SpotifyClientOptions> _opts;

    public SpotifySearcher(IOptions<SpotifyClientOptions> opts)
    {
        _opts = opts;
        _httpClient = new HttpClient();
    }

    enum SearchType
    {
        track,
        playlist,
        album
    }

    // https://developer.spotify.com/documentation/web-api/reference/search
    public async Task<string> GetTrackNameAndArtistFromSpotifyUrl(string spotifyUrl)
    {
        var accessTokenResponse = await GetSpotifyAccessToken();
        var searchType = GetSearchType(spotifyUrl);
        var itemId = GetSpotifyItemId(spotifyUrl, searchType);
        var result = await SearchAsync(itemId, searchType, accessTokenResponse.access_token);
        return result.GetTrackArtists() + " " + result.GetTrackName();
    }

    private static SearchType GetSearchType(string url)
    {
        if (!url.StartsWith("https://open.spotify.com/"))
        {
            throw new NotSupportedException($"Not a spotify URL: {url}");
        }

        var endpoint = url.Split("https://open.spotify.com/")[1];

        if (endpoint.Contains("track"))
        {
            return SearchType.track;
        }
        else if (endpoint.Contains("playlist"))
        {
            // TODO: queue the whole thing
            throw new NotImplementedException();
        }
        else if (endpoint.Contains("album"))
        {
            // TODO: queue the whole thing
            throw new NotImplementedException();
        }
        else
        {
            throw new NotSupportedException($"Did not recognize spotify endpoint: {endpoint}");
        }
    }

    private static string GetSpotifyItemId(string query, SearchType searchType)
    {
        var queries = query.Split("https://open.spotify.com/" + searchType + "/")[1];
        // If link is copied manually from spotify app it will contain a
        // ?si= parameter that seems to be referral information
        // Filter this out. Will still filter ID out properly
        // if referral query is not present
        var id = queries.Split("?")[0];
        if (!id.All(char.IsLetterOrDigit))
        {
            throw new NotSupportedException($"Got a non-alphanumeric ID: {id}");
        }
        return id;
    }

    private async Task<ISpotifyResult> SearchAsync(string id, SearchType type, string accessToken)
    {
        var url = "https://api.spotify.com/v1/" + type + "s" + "/" + id;
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await _httpClient.GetAsync(url);

        switch (type)
        {
            case SearchType.track:
                return await response.Content.ReadFromJsonAsync<Track>();
            case SearchType.playlist:
                return await response.Content.ReadFromJsonAsync<Playlist>();
            case SearchType.album:
                return await response.Content.ReadFromJsonAsync<Album>();
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    // "Request an access token"
    // https://developer.spotify.com/documentation/web-api/tutorials/getting-started
    private async Task<AccessTokenResponse?> GetSpotifyAccessToken()
    {
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");
        httpRequestMessage.Content =
            new StringContent(
                $"grant_type=client_credentials&client_id={_opts.Value.Id}&client_secret={_opts.Value.Secret}");
        httpRequestMessage.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");
        var httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);
        var response = await httpResponseMessage.Content.ReadFromJsonAsync<AccessTokenResponse>();
        return response;
    }
}