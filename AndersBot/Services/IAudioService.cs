using System.Net.Sockets;
using Discord;
using Discord.Interactions;
using Victoria.Node;
using Victoria.Player;
using Victoria.Responses.Search;
using static AndersBot.DiscordEmotes;

namespace AndersBot.Services;

public interface IAudioService
{
    public Task Join(SocketInteractionContext ctx);

    public Task Play(SocketInteractionContext ctx, string query);

    public Task Skip(SocketInteractionContext ctx);

    public Task Stop(SocketInteractionContext ctx);

    public Task CurrentlyPlaying(SocketInteractionContext ctx);

    public Task Queue(SocketInteractionContext ctx);

    public Task Leave(SocketInteractionContext ctx);
}