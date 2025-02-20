using Events;

namespace Monolith;

public interface IPlayer
{
    PlayerMovedEvent MakeMove(GameStartedEvent e);
    void ReceiveResult(GameFinishedEvent e);
}