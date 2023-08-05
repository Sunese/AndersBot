using AndersBot.Services;
using Discord.Interactions;
using Discord.WebSocket;
using Victoria.Node;
using IResult = Discord.Interactions.IResult;


namespace AndersBot.InteractionModules;

public class MusicInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<MusicInteractionModule> _logger;
    private readonly IAudioService _audioService;
    private readonly LavaNode _lavaNode;
    private readonly DiscordSocketClient _client;

    public MusicInteractionModule(
        ILogger<MusicInteractionModule> logger, 
        IAudioService audioService,
        LavaNode lavaNode,
        DiscordSocketClient client)
    {
        _logger = logger;
        _audioService = audioService;
        _lavaNode = lavaNode;
        _client = client;
    }

    [SlashCommand("queue", "Show queue")]
    public async Task Queue()
    {
        await _audioService.Queue(Context);
    }

    [SlashCommand("currentlyplaying", "Show the song currently playing")]
    public async Task CurrentlyPlaying()
    {
        await _audioService.CurrentlyPlaying(Context);
    }

    [SlashCommand("play", "Play a song")]
    public async Task Play([Summary(description: "Search query")] string query)
    {
        await _audioService.Play(Context, query);
    }

    [SlashCommand("skip", "Skip the song being played")]
    public async Task Skip()
    {
        await _audioService.Skip(Context);
    }

    [SlashCommand("leave", "Make Anders leave")]
    public async Task Leave()
    {
        await _audioService.Leave(Context);
    }

    [SlashCommand("join", "Make Anders join your voice channel")]
    public async Task Join()
    {
        await _audioService.Join(Context);
    }

    [SlashCommand("stop", "Stop playing music")]
    public async Task Stop()
    {
        await _audioService.Stop(Context);
    }
}
