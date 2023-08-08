using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Serilog;
using Serilog.Events;
using Victoria.Node;
using Victoria.Node.EventArgs;
using Victoria.Player;
using IResult = Discord.Commands.IResult;
using static AndersBot.Mentorhold8;

namespace AndersBot;

public class InteractionHandler
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _handler;
    private readonly IServiceProvider _services;
    private readonly IConfiguration _configuration;
    private readonly CommandService _commands;
    private readonly ILogger<InteractionHandler> _logger;
    private readonly LavaNode _lavaNode;

    public InteractionHandler(
        DiscordSocketClient client, 
        InteractionService handler, 
        IServiceProvider services, 
        IConfiguration config, 
        CommandService commands, 
        ILogger<InteractionHandler> logger,
        LavaNode lavaNode)
    {
        _client = client;
        _handler = handler;
        _services = services;
        _configuration = config;
        _commands = commands;
        _logger = logger;
        _lavaNode = lavaNode;
    }

    public async Task InitializeAsync()
    {
        // Process when the client is ready, so we can register our commands.
        _client.Ready += ReadyAsync;
        _handler.Log += LogAsync;

        // Add the public modules that inherit InteractionModuleBase<T> to the InteractionService (for modern commands, e.g. slash/user/message)
        await _handler.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        // Register modules that are public and inherit ModuleBase<T>. (for traditional commands, e.g. !help)
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

        // Process the InteractionCreated payloads to execute Interactions commands
        // --> Interaction commands = slash commands, user commands (right click user -> apps), text commands (right click commands -> apps)
        _client.InteractionCreated += HandleInteraction;

        // Process received messages
        _client.MessageReceived += HandleMessageReceived;
        _commands.CommandExecuted += HandleCommandExecuted;

        _client.Disconnected += Disconnected;

        _lavaNode.OnTrackStart += OnTrackStart;
        _lavaNode.OnTrackStuck += OnTrackStuck;
        _lavaNode.OnTrackEnd += OnTrackEnd;
        _lavaNode.OnTrackException += OnTrackException;

        //_client.UserVoiceStateUpdated += UserVoiceStateUpdated;

    }

    //public Task UserVoiceStateUpdated()

    public Task OnTrackException(TrackExceptionEventArg<LavaPlayer<LavaTrack>, LavaTrack> arg)
    {
        arg.Player.TextChannel.SendMessageAsync($"LavaNode encountered an error with ({arg.Track.Title}) {peepoDown} Skipping...");
        arg.Player.SkipAsync();
        return Task.CompletedTask;
    }

    public Task OnTrackEnd(TrackEndEventArg<LavaPlayer<LavaTrack>, LavaTrack> arg)
    {
        switch (arg.Reason)
        {
            case TrackEndReason.Finished:
                arg.Player.SkipAsync();
                if (arg.Player.Vueue.Count == 0)
                {
                    arg.Player.TextChannel.SendMessageAsync($"No more tracks in queue {peepoDown}");
                }
                break;
            case TrackEndReason.LoadFailed:
                throw new NotImplementedException();
            case TrackEndReason.Stopped:
                break;
            case TrackEndReason.Replaced:
                break;
            case TrackEndReason.Cleanup:
                throw new NotImplementedException();
            default:
                throw new ArgumentOutOfRangeException();
        }
        return Task.CompletedTask;
    }

    public Task OnTrackStuck(TrackStuckEventArg<LavaPlayer<LavaTrack>, LavaTrack> arg)
    {
        arg.Player.TextChannel.SendMessageAsync($"Track got stuck ({arg.Track.Title}) {peepoDown} Skipping...");
        arg.Player.SkipAsync();
        return Task.CompletedTask;
    }

    public Task OnUpdateReceived(UpdateEventArg<LavaPlayer<LavaTrack>, LavaTrack> arg)
    {
        //arg.Player.TextChannel.SendMessageAsync($"Enqueued {arg.Track.Title} <:pepeJAM:1095043579635826840>");
        return Task.CompletedTask;
    }

    public Task OnTrackStart(TrackStartEventArg<LavaPlayer<LavaTrack>, LavaTrack> arg)
    {
        arg.Player.TextChannel.SendMessageAsync($"Playing {arg.Track.Title} {pepejam}\n{arg.Track.Url}");
        return Task.CompletedTask;
    }

    public Task Disconnected(Exception e)
    {
        _lavaNode.DisconnectAsync();
        return Task.CompletedTask;
    }

    private Task LogAsync(LogMessage message)
    {
        var severity = message.Severity switch
        {
            LogSeverity.Critical => LogEventLevel.Fatal,
            LogSeverity.Error => LogEventLevel.Error,
            LogSeverity.Warning => LogEventLevel.Warning,
            LogSeverity.Info => LogEventLevel.Information,
            LogSeverity.Verbose => LogEventLevel.Verbose,
            LogSeverity.Debug => LogEventLevel.Debug,
            _ => LogEventLevel.Information
        };

        Log.Write(severity, message.Exception, "[{Source}] {Message}", message.Source, message.Message);

        return Task.CompletedTask;
    }

    private async Task ReadyAsync()
    {
        // Context & Slash commands can be automatically registered, but this process needs to happen after the client enters the READY state.
        // Since Global Commands take around 1 hour to register, we should use a test guild to instantly update and test our commands.
        if (Program.IsDebug())
            foreach (var guild in _client.Guilds)
            {
                await _handler.RegisterCommandsToGuildAsync(guild.Id, true);
            }
        else
            await _handler.RegisterCommandsGloballyAsync(true);

        await _lavaNode.ConnectAsync();
    }

    private async Task HandleInteraction(SocketInteraction interaction)
    {
        try
        {
            // Create an execution context that matches the generic type parameter of your InteractionModuleBase<T> modules.
            var context = new SocketInteractionContext(_client, interaction);

            // Execute the incoming command.
            var result = await _handler.ExecuteCommandAsync(context, _services);

            if (!result.IsSuccess)
                switch (result.Error)
                {
                    case InteractionCommandError.UnmetPrecondition: throw new NotImplementedException();
                    case InteractionCommandError.UnknownCommand: throw new NotImplementedException();
                    case InteractionCommandError.ConvertFailed: throw new NotImplementedException();
                    case InteractionCommandError.BadArgs: throw new NotImplementedException();
                    case InteractionCommandError.Exception: throw new NotImplementedException();
                    case InteractionCommandError.Unsuccessful: throw new NotImplementedException();
                    case InteractionCommandError.ParseFailed: throw new NotImplementedException();
                    case null: throw new NotImplementedException();
                    default: throw new NotImplementedException();
                }
        }
        catch
        {
            // If Slash Command execution fails it is most likely that the original interaction acknowledgement will persist. It is a good idea to delete the original
            // response, or at least let the user know that something went wrong during the command execution.
            if (interaction.Type is InteractionType.ApplicationCommand)
                await interaction.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
        }
    }

    private async Task HandleMessageReceived(SocketMessage rawMessage)
    {
        // Ignore system messages, or messages from other bots
        if (!(rawMessage is SocketUserMessage message))
            return;
        if (message.Source != MessageSource.User)
            return;

        var argPos = 0;

        var context = new SocketCommandContext(_client, message);

        if (!message.HasCharPrefix('.', ref argPos))
            return;

        // Perform the execution of the command. In this method,
        // the command service will perform precondition and parsing check
        // then execute the command if one is matched.
        await _commands.ExecuteAsync(context, argPos, _services);
        // Note that normally a result will be returned by this format, but here
        // we will handle the result in CommandExecutedAsync,
    }

    public async Task HandleCommandExecuted(Optional<CommandInfo> command, ICommandContext context, IResult result)
    {
        // command is unspecified when there was a search failure (command not found); we don't care about these errors
        if (!command.IsSpecified)
            return;

        // the command was successful, we don't care about this result, unless we want to log that a command succeeded.
        if (result.IsSuccess)
            return;

        // the command failed, let's notify the user that something happened.
        await context.Channel.SendMessageAsync($"error: {result}");
    }
}