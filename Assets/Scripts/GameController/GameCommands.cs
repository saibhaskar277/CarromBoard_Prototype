using UnityEngine;

public interface IGameCommand
{
    void Execute();
}

public class CommandExecutor
{
    public void Execute(IGameCommand command)
    {
        if (command == null) return;
        command.Execute();
    }
}

public class PositionStrikerCommand : IGameCommand
{
    private readonly Vector2 worldPosition;

    public PositionStrikerCommand(Vector2 worldPosition)
    {
        this.worldPosition = worldPosition;
    }

    public void Execute()
    {
        EventManager.RaiseEvent(new RequestStrikerWorldPositionEvent(worldPosition));
    }
}

public class ShootStrikerCommand : IGameCommand
{
    private readonly Vector2 direction;
    private readonly float power;

    public ShootStrikerCommand(Vector2 direction, float power)
    {
        this.direction = direction;
        this.power = power;
    }

    public void Execute()
    {
        EventManager.RaiseEvent(new RequestStrikerShotEvent(direction, power));
    }
}
