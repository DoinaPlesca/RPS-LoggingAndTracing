using System.Diagnostics;
using Events;
using Serilog;

namespace Monolith;

public class Game
{
    private readonly Dictionary<Guid, GameModel?> _games = new();
    private readonly IPlayer _player1 = new RandomPlayer();
    private readonly IPlayer _player2 = new CopyPlayer();
    private static readonly ActivitySource  activitySource = new("Monolith");

    public void Start()
    {
        Guid gameId = Guid.NewGuid();
        _games.Add(gameId, new GameModel { GameId = gameId });

        using (var activity = activitySource.StartActivity("Game Started", ActivityKind.Server))
        {
            Log.Information("Game Started with ID: {GameId}", gameId);
            activity?.SetTag("game.id", gameId.ToString());

            var startEvent = new GameStartedEvent { GameId = gameId };

            var p1Event = _player1.MakeMove(startEvent);
            Log.Information("Player 1: {PlayerId} made move: {Move}", p1Event.PlayerId, p1Event.Move);
            activity?.AddEvent(new ActivityEvent($"Player 1: {p1Event.PlayerId} made move: {p1Event.Move}"));
            ReceivePlayerEvent(p1Event);

            var p2Event = _player2.MakeMove(startEvent);
            Log.Information("Player 2: {PlayerId} made move: {Move}", p2Event.PlayerId, p2Event.Move);
            activity?.AddEvent(new ActivityEvent($"Player 2: {p2Event.PlayerId} made move: {p2Event.Move}"));
            ReceivePlayerEvent(p2Event);
        }
    }

    public string DeclareWinner(KeyValuePair<string, Move> p1, KeyValuePair<string, Move> p2)
    {
        string? winner = null;

        switch (p1.Value)
        {
            case Move.Rock:
                winner = p2.Value switch
                {
                    Move.Paper => p2.Key,
                    Move.Scissor => p1.Key,
                    _ => winner
                };
                break;
            case Move.Paper:
                winner = p2.Value switch
                {
                    Move.Rock => p1.Key,
                    Move.Scissor => p2.Key,
                    _ => winner
                };
                break;
            case Move.Scissor:
                winner = p2.Value switch
                {
                    Move.Rock => p2.Key,
                    Move.Paper => p1.Key,
                    _ => winner
                };
                break;
        }

        return winner ?? "Tie";
    }

    public void ReceivePlayerEvent(PlayerMovedEvent e)
    {
        if (_games.TryGetValue(e.GameId, out var game))
        {
            lock (game)
            {
                game.Moves.Add(e.PlayerId, e.Move);
                if (game.Moves.Values.Count == 2)
                {
                    KeyValuePair<string?, Move> p1 = game.Moves.First()!;
                    KeyValuePair<string?, Move> p2 = game.Moves.Skip(1).First()!;

                    var finishedEvent = PrepareWinnerAnnouncement(game, p1, p2);

                    using (var activity = activitySource.StartActivity("Game Finished", ActivityKind.Server))
                    {
                        Log.Information("Game {GameId} finished. Winner: {Winner}", game.GameId, finishedEvent.WinnerId);
                        activity?.SetTag("game.id", game.GameId.ToString());
                        activity?.SetTag("winner", finishedEvent.WinnerId);
                        activity?.AddEvent(new ActivityEvent($"Game {game.GameId} finished. Winner: {finishedEvent.WinnerId}"));
                    }
                    _player1.ReceiveResult(finishedEvent);
                    _player2.ReceiveResult(finishedEvent);

                    _games.Remove(game.GameId);
                }
            }
        }
    }

    public GameFinishedEvent PrepareWinnerAnnouncement(GameModel game, KeyValuePair<string?, Move> p1, KeyValuePair<string?, Move> p2)
    {
        var finishedEvent = new GameFinishedEvent
        {
            GameId = game.GameId,
            Moves = game.Moves,
            WinnerId = DeclareWinner(p1!, p2!)
        };
        return finishedEvent;
    }
}

public class GameModel
{
    public Guid GameId { get; set; }
    public Dictionary<string, Move> Moves { get; set; } = new();
}