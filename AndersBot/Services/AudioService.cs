using System.Net.Sockets;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Victoria.Node;
using Victoria.Player;
using Victoria.Responses.Search;
using static AndersBot.DiscordEmotes;

namespace AndersBot.Services;

public class AudioService : IAudioService
{
    private readonly ISearchService _searchService;
    private readonly LavaNode _lavaNode;

    public AudioService(ISearchService searchService, LavaNode lavaNode)
    {
        _searchService = searchService;
        _lavaNode = lavaNode;
    }

    /*
     * Public portion
     */
    public async Task Join(SocketInteractionContext ctx)
    {
        await ctx.Interaction.DeferAsync();

        if (!await UserIsInChannel(ctx))
        {
            return;
        }

        var voicechannel = (ctx.User as IGuildUser)?.VoiceChannel;
        await JoinVoice(ctx, voicechannel);
        await ModifyResponseAsync(ctx, $"Haiii {peepoShy}");
    }

    public async Task Play(SocketInteractionContext ctx, string query)
    {
        await ctx.Interaction.DeferAsync();
        
        if (!await UserIsInChannel(ctx))
        {
            await ModifyResponseAsync(ctx, $"You are not in a voice channel {madge}");
            return;
        }

        if (!_lavaNode.IsConnected)
        {
            await ModifyResponseAsync(ctx, $"Lost connection to LavaLink {sadge} Trying to re-connect... {animated_hackermans}");

            // NOTE: blocking
            // TODO: perhaps have an event trigger another response if connection is never succesful
            await ReconnectToLavaLink();

            await ModifyResponseAsync(ctx, $"Re-established LavaLink connection {hmmNice}");
        }

        var userVoiceChannel = (ctx.User as IGuildUser)?.VoiceChannel;

        if (!_lavaNode.HasPlayer(ctx.Guild))
        {
            await JoinVoice(ctx, userVoiceChannel);
        }

        var searchResponse = await _searchService.Search(ctx, query);

        switch (searchResponse.Status)
        {
            case SearchStatus.LoadFailed:
                await ModifyResponseAsync(ctx, $"An error occurred {peepoDown} Try again!");
                return;
            case SearchStatus.NoMatches:
                await ModifyResponseAsync(ctx, $"No match found for {query} {peepoDown}");
                // TODO: ask user if they want bot to search on SC because SC is very weird D:
                return;
            default:
                _lavaNode.TryGetPlayer(ctx.Guild, out var player);
                await QueueTracksToPlayer(ctx, player, searchResponse);
                break;
        }
    }

    public async Task Skip(SocketInteractionContext ctx)
    {
        await ctx.Interaction.DeferAsync();

        if (!await UserIsInChannel(ctx) || !await IsPlaying(ctx))
        {
            return;
        }

        _lavaNode.TryGetPlayer(ctx.Guild, out var player);

        var currentlyPlaying = player.Track.Title;

        if (player.Vueue.Count < 1) await player.StopAsync();
        else await player.SkipAsync();

        await ctx.Interaction.ModifyOriginalResponseAsync(msg => msg.Content =
            $"Skipped {currentlyPlaying} {hmmNice}");
    }

    public async Task Stop(SocketInteractionContext ctx)
    {
        await ctx.Interaction.DeferAsync();

        if (!await UserIsInChannel(ctx) || !await IsPlaying(ctx))
        {
            return;
        }

        _lavaNode.TryGetPlayer(ctx.Guild, out var player);

        await player.StopAsync();

        await ctx.Interaction.ModifyOriginalResponseAsync(msg =>
            msg.Content = $"Stopped playing {hmmNice}");
    }

    public async Task CurrentlyPlaying(SocketInteractionContext ctx)
    {
        await ctx.Interaction.DeferAsync();
        if (!await UserIsInChannel(ctx) || !await IsPlaying(ctx))
        {
            return;
        }
        _lavaNode.TryGetPlayer(ctx.Guild, out var player);
        await ctx.Interaction.ModifyOriginalResponseAsync(
            msg => msg.Content = $"Currently playing {player.Track.Title} {pepejam}\n{player.Track.Url}");
    }

    public async Task Queue(SocketInteractionContext ctx)
    {
        await ctx.Interaction.DeferAsync();

        if (!await UserIsInChannel(ctx) || !await IsPlaying(ctx))
        {
            return;
        }

        _lavaNode.TryGetPlayer(ctx.Guild, out var player);

        var queueList = player.Vueue.ToList();
        if (queueList.Count <= 0)
        {
            await ctx.Interaction.ModifyOriginalResponseAsync(msg => 
                msg.Content = $"Queue is empty {peepoDown}");
            return;
        }

        var queueString = CreatePlaylistString(player.Vueue.ToList());
        await ctx.Interaction.ModifyOriginalResponseAsync(msg => 
            msg.Content = $"{pepejam} currently in queue:\n{queueString}");
    }

    public async Task Leave(SocketInteractionContext ctx)
    {
        // TODO: only allow if 
        // 1) is in channel with bot
        // 2) bot is alone in a channel

        await ctx.Interaction.DeferAsync();

        if (!await UserIsInChannel(ctx))
        {
            return;
        }

        var voicechannel = (ctx.User as IGuildUser)?.VoiceChannel;

        await voicechannel.DisconnectAsync();

        await ctx.Interaction.ModifyOriginalResponseAsync(msg => 
            msg.Content = "Bye! <:peepoPoo:1095001226426449980>");

        if (await IsPlaying(ctx))
        {
            await _lavaNode.LeaveAsync(voicechannel);
        }
    }

    private async Task<LavaPlayer<LavaTrack>> JoinVoice(SocketInteractionContext ctx, IVoiceChannel voiceChannel)
    {
        var player = await _lavaNode.JoinAsync(voiceChannel, ctx.Channel as ITextChannel);
        await player.SetVolumeAsync(0); // TODO: what is the volume range?
        return player;
    }

    /*
     * Private portion
     */
    private async Task<bool> IsPlaying(SocketInteractionContext ctx)
    {
        var hasPlayer = _lavaNode.TryGetPlayer(ctx.Guild, out var player);

        var message = $"Not playing anything {peepoSit}";

        if (hasPlayer)
        {
            switch (player.PlayerState)
            {
                case PlayerState.Playing:
                    return true;
                case PlayerState.Paused:
                    message = $"Player is paused {peepoSit}";
                    break;
                case PlayerState.Stopped:
                    message = $"Player is stopped {peepoSit}";
                    break;
                case PlayerState.None:
                    message = $"Not playing anything {peepoSit}";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        await ctx.Interaction.ModifyOriginalResponseAsync(msg =>
            msg.Content = message);
        return false;
    }

    private string CreatePlaylistString(List<LavaTrack> tracklist)
    {
        var result = "";
        for (int i = 0; i <= tracklist.Count - 1; i++)
        {
            result += $"[{i}] {tracklist[i]}\n";
        }

        return result;
    }

    private async Task<bool> UserIsInChannel(SocketInteractionContext ctx)
    {
        return (ctx.User as IGuildUser)?.VoiceChannel is not null;
    }

    private async Task ReconnectToLavaLink()
    {
        while (!_lavaNode.IsConnected)
        {
            await _lavaNode.ConnectAsync();
        }
    }

    private async Task QueueTracksToPlayer(SocketInteractionContext ctx, LavaPlayer<LavaTrack> player, SearchResponse search)
    {
        lock (player.Vueue)
        {
            List<LavaTrack> lavaTracks;
            if (search.Status == SearchStatus.PlaylistLoaded)
            {
                lavaTracks = search.Tracks.ToList();
            }
            else
            {
                lavaTracks = new List<LavaTrack>
                {
                    search.Tracks.First()
                };
            }
            foreach (var track in lavaTracks)
            {
                player.Vueue.Enqueue(track);
            }
        }

        if (player.PlayerState is PlayerState.Playing)
        {
            // do nothing
        }
        else if (player.PlayerState is PlayerState.Paused or PlayerState.Stopped or PlayerState.None)
        {
            _ = player.Vueue.TryDequeue(out var newTrack);
            await player.PlayAsync(newTrack);
        }

        // Respond to user
        if (search.Status == SearchStatus.PlaylistLoaded)
        {
            var playlistString = CreatePlaylistString(search.Tracks.ToList());
            await ctx.Interaction.ModifyOriginalResponseAsync(msg =>
                msg.Content = $"Queued playlist {hmmNice}\n{playlistString}");
        }
        else
        {
            await ctx.Interaction.ModifyOriginalResponseAsync(
                msg => msg.Content = $"Queued {search.Tracks.First().Title} {hmmNice}");
        }
    }

    private async Task ModifyResponseAsync(SocketInteractionContext ctx, string newResponse)
    {
        await ctx.Interaction.ModifyOriginalResponseAsync(msg => msg.Content = newResponse);
    }
}