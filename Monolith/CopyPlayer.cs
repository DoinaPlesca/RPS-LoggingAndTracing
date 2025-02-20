using Events;

namespace Monolith;

public class CopyPlayer : IPlayer
{
    private const string PlayerId = "The Copy Cat";
    private readonly Queue<Move> _previousMoves = new Queue<Move>();

    public PlayerMovedEvent MakeMove(GameStartedEvent e)
    {
        Move move = Move.Paper;
        if (_previousMoves.Count > 2)
        {
            move = _previousMoves.Dequeue();
        }

        var moveEvent = new PlayerMovedEvent
        {
            GameId = e.GameId,
            PlayerId = PlayerId,
            Move = move
        };
        return moveEvent;
    }

    public void ReceiveResult(GameFinishedEvent e)
    {
        var otherMove = e.Moves.SingleOrDefault(m => m.Key != PlayerId).Value;
        _previousMoves.Enqueue(otherMove);
    }
}