using AndersBot.Services;
using Discord.Interactions;
using Discord.WebSocket;
using Victoria.Node;
using static AndersBot.DiscordEmotes;


namespace AndersBot.InteractionModules;

public class GeneralInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<GeneralInteractionModule> _logger;
    private readonly IAudioService _audioService;
    private readonly LavaNode _lavaNode;
    private readonly DiscordSocketClient _client;

    public GeneralInteractionModule(
        ILogger<GeneralInteractionModule> logger, 
        IAudioService audioService,
        LavaNode lavaNode,
        DiscordSocketClient client)
    {
        _logger = logger;
        _audioService = audioService;
        _lavaNode = lavaNode;
        _client = client;
    }

    // NOTE: these were set up to test wether emotes actually
    //       show up. I have experienced that they sometimes don't.
    //       Perhaps set up proper tests for this instead.

    //[SlashCommand("respondhello", "hello")]
    //public async Task Hello()
    //{
    //    await RespondAsync($"hiii {pepejam}");
    //}

    //[SlashCommand("deferhello", "hello")]
    //public async Task Hello2()
    //{
    //    await DeferAsync();
    //    await ModifyOriginalResponseAsync(msg => msg.Content = $"hiii {pepejam}");
    //}

    //[SlashCommand("deferhackermans", "hello")]
    //public async Task Hello4()
    //{
    //    await DeferAsync();
    //    await ModifyOriginalResponseAsync(msg => msg.Content = $"hiii {a_hackermans}");
    //}

    //[SlashCommand("randomemotehello", "hello")]
    //public async Task Hello3()
    //{
    //    await DeferAsync();
    //    var emotes = await Context.Guild.GetEmotesAsync();
    //    Random rnd = new Random();
    //    var number = rnd.Next(emotes.Count);
    //    await ModifyOriginalResponseAsync(msg => msg.Content = $"hiii {emotes.ElementAt(number)}");
    //}
}
