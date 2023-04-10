using Discord;
using Victoria;
using Victoria.Node;
using Victoria.Player;
using Victoria.Responses.Search;

namespace AndersBot.Services;

public class AudioService
{
    private LavaNode _lavaNode { get; set; }

    public AudioService(LavaNode lavaNode)
    {
        _lavaNode = lavaNode;

        _lavaNode.OnTrackStart += async (args) =>
        {
            var player = args.Player;

            //If for some reason Volume is set to 0 (100%) it will set to default volume
            //if (player.Volume == 0)
            //{
            //    await player.UpdateVolumeAsync(Program.BotConfig.Volume);
            //}

            //var content = queue switch
            //{
            //    "" => NoSongsInQueue,
            //    _ => string.Format(QueueMayHaveSongs, queue)
            //};
        };


    }

    public async Task QueueTrackToPlayer(LavaPlayer<LavaTrack> player, SearchResponse search)
    {
        List<LavaTrack> lavaTracks;
        string newQueue;
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

        if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
        {
            foreach (var track in lavaTracks)
            {
                player.Vueue.Enqueue(track);
            }
        }
        else
        {
            foreach (var track in lavaTracks)
            {
                player.Vueue.Enqueue(track);
            }

            _ = player.Vueue.TryDequeue(out var newTrack);

            await player.PlayAsync(newTrack);
        }
    }
}