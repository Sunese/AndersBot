using Discord;
using Discord.Interactions;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using AndersBot.Attributes;
using AndersBot.Services;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Victoria.Node;
using Victoria.Player;
using Victoria.Responses.Search;


namespace AndersBot.Modules;

public class GeneralInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<GeneralInteractionModule> _logger;
    private readonly AudioService _audioService;
    private readonly LavaNode _lavaNode;
    private readonly DiscordSocketClient _client;

    public GeneralInteractionModule(
        ILogger<GeneralInteractionModule> logger, 
        AudioService audioService,
        LavaNode lavaNode,
        DiscordSocketClient client)
    {
        _logger = logger;
        _audioService = audioService;
        _lavaNode = lavaNode;
        _client = client;
    }
    
    [SlashCommand("hello", "hello")]
    public async Task Hello()
    {
        await RespondAsync("hiii :3");
    }

    //[UserInVoiceChannelCheck]
    [SlashCommand("queue", "Show queue")]
    public async Task Queue()
    {
        await DeferAsync();
        if ((Context.User as IGuildUser)?.VoiceChannel is null)
        {
            await ModifyOriginalResponseAsync(msg =>
                msg.Content = "You are not in a voice channel you fucking retard <:Madge:1095001223171674273>");
            return;
        }

        if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
        {
            await ModifyOriginalResponseAsync(msg => msg.Content = "I am currently not playing anything <:peepoDown:1095001205278773339>");
            return;
        }

        var queueList = player.Vueue.ToList();
        if (!(queueList.Count > 0))
        {
            await ModifyOriginalResponseAsync(msg => msg.Content = "Queue is empty <:peepoDown:1095001205278773339>");
            return;
        }

        string result = "<:pepeJAM:1095043579635826840> currently in queue:\n";
        
        for (int i = 0; i <= queueList.Count - 1; i++)
        {
            result += $"[{i}] {queueList[i]}\n";
        }

        await ModifyOriginalResponseAsync(msg => msg.Content = result);
        return;
    }

    [SlashCommand("play", "Play a song")]
    public async Task Play([Summary(description: "Search query")] string query)
    {
        await DeferAsync();
        if ((Context.User as IGuildUser)?.VoiceChannel is null)
        {
            await ModifyOriginalResponseAsync(msg =>
                msg.Content = "You are not in a voice channel you fucking retard <:Madge:1095001223171674273>");
            return;
        }
        await ModifyOriginalResponseAsync(msg => msg.Content = "Looking for track... <a:HACKERMANS:587667462670123060>");
        LavaPlayer<LavaTrack> player;
        if (!_lavaNode.HasPlayer(Context.Guild))
        {
            try
            {
                if (!_lavaNode.IsConnected)
                {
                    _logger.LogError($"User tried playing but LavaLink is not connected");
                    await ModifyOriginalResponseAsync(msg => msg.Content = "Could not connect to LavaLink <:peepoDown:1095001205278773339>");
                    return;
                }
                player = await _lavaNode.JoinAsync((Context.User as IGuildUser)?.VoiceChannel, Context.Channel as ITextChannel);
                await player.SetVolumeAsync(0);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create player");
            }
        }

        if (!_lavaNode.TryGetPlayer(Context.Guild, out player))
        {
            _logger.LogError("Could not get player.");
            return;
        }

        var searchType = query.Substring(0,4).Equals("http") ? SearchType.Direct : SearchType.YouTube;

        var search = await _lavaNode.SearchAsync(searchType, query);

        if (search.Status is SearchStatus.LoadFailed)
        {
            _logger.LogError($"SearchStatus LoadFailed for query: '{query}' and SearchType: '{searchType.ToString()}'");
            await ModifyOriginalResponseAsync(msg => msg.Content = $"An error occurred <:peepoDown:1095001205278773339> Try again!");
            return;
        }

        if (search.Status is SearchStatus.NoMatches)
        {
            await ModifyOriginalResponseAsync(msg => msg.Content = $"No match found for {query} <:peepoDown:1095001205278773339>");
            // TODO: ask user if they want bot to search on SC because SC is very weird D:
            return;
        }

        await _audioService.QueueTracksToPlayer(player, search);

        if (player.PlayerState is PlayerState.Playing)
        {
            await ModifyOriginalResponseAsync(msg => msg.Content = $"Queued {search.Tracks.First().Title} <:pepeJAM:1095043579635826840>");
        }
        else
        {
            //await ModifyOriginalResponseAsync(msg => msg.Content = $"Playing {search.Tracks.First().Title} <:pepeJAM:1095043579635826840>");
        }
    }

    [SlashCommand("skip", "Skip the song being played")]
    public async Task Skip()
    {
        await DeferAsync();

        if ((Context.User as IGuildUser)?.VoiceChannel is null)
        {
            await ModifyOriginalResponseAsync(msg =>
                msg.Content = "You are not in a voice channel you fucking retard <:Madge:1095001223171674273>");
            return;
        }
        
        if (!_lavaNode.HasPlayer(Context.Guild))
        {
            await ModifyOriginalResponseAsync(msg => msg.Content = "I am currently not playing anything <:peepoDown:1095001205278773339>");
            return;
        }

        if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
        {
            await ModifyOriginalResponseAsync(msg => msg.Content = "An error occurred <:peepoDown:1095001205278773339> Try again!");
            _logger.LogError("Could not get player.");
            return;
        }

        if (player.Vueue.Count <= 0)
        {
            await player.StopAsync();
            await ModifyOriginalResponseAsync(msg => msg.Content = "No tracks left in queue. Stopped playing<:hmmNice:1095001212245520444>");
            return;
        }

        var tracks = await player.SkipAsync();

        await ModifyOriginalResponseAsync(msg => msg.Content =
            $"Skipped {tracks.Skipped.Title} <:hmmNice:1095001212245520444>");
    }

    [SlashCommand("leave", "Make Anders leave")]
    public async Task Leave()
    {
        // TODO: only allow if 
        // 1) is in channel with bot
        // 2) bot is alone in a channel
        await DeferAsync();

        if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
        {
            await ModifyOriginalResponseAsync(msg =>
                msg.Content = "Not in a voice channel <:peepoPoo:1095001226426449980>");
            return;
        }

        await player.StopAsync();
        player.Vueue.Clear();
        await _lavaNode.LeaveAsync(player.VoiceChannel);
        await ModifyOriginalResponseAsync(msg => msg.Content = "Bye! <:peepoPoo:1095001226426449980>");

    }
}
