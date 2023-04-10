using Discord;
using Discord.Interactions;
using System;
using System.Linq;
using System.Threading.Tasks;
using AndersBot.Services;
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

    public GeneralInteractionModule(
        ILogger<GeneralInteractionModule> logger, 
        AudioService audioService,
        LavaNode lavaNode)
    {
        _logger = logger;
        _audioService = audioService;
        _lavaNode = lavaNode;
    }

    [SlashCommand("hello", "hello")]
    public async Task Hello()
    {
        await RespondAsync("hiii :3");
    }

    [SlashCommand("play", "Play a song")]
    public async Task Play(string query)
    {
        await DeferAsync();
        await ModifyOriginalResponseAsync(msg => msg.Content = "Looking for track...");
        LavaPlayer<LavaTrack> player;
        if (!_lavaNode.HasPlayer(Context.Guild))
        {
            try
            {
                if ((Context.User as IGuildUser).VoiceChannel is null)
                {
                    return;
                }

                await _lavaNode.ConnectAsync();
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

        // TODO: uuuuh what does SearchType.Direct do?
        var search = await _lavaNode.SearchAsync(SearchType.YouTube, query);

        if (search.Status is SearchStatus.LoadFailed or SearchStatus.NoMatches)
        {
            await Context.Channel.SendMessageAsync($"{search.Exception}");
            return;
        }

        await ModifyOriginalResponseAsync(msg => msg.Content = $"Playing {search.Tracks.First().Title}");
        await _audioService.QueueTrackToPlayer(player, search);

    }
}
