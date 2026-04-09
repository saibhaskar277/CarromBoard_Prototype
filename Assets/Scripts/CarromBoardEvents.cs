
using System.Collections.Generic;
using UnityEngine;


public class OnBoardSpawnedEvent : IGameEvent
{
    public BoardController BoardController;
    public OnBoardSpawnedEvent(BoardController boardController)
    {
        this.BoardController = boardController;
    }
}




public class StrikerPositionChangedEvent : IGameEvent
{
    public Vector3 worldPosition;

    public StrikerPositionChangedEvent(Vector3 worldPosition)
    {
        this.worldPosition = worldPosition;
    }
}

public class SetStrikerPositionOffsetEvent : IGameEvent
{
    public Vector3 leftWorld;
    public Vector3 rightWorld;

    public SetStrikerPositionOffsetEvent(Vector3 leftWorld, Vector3 rightWorld)
    {
        this.leftWorld = leftWorld;
        this.rightWorld = rightWorld;
    }
}

public class SetOpponentStrikerPositionOffsetEvent : IGameEvent
{
    public float leftPos;
    public float rightPos;
    public float yPos;

    public SetOpponentStrikerPositionOffsetEvent(float leftPos, float rightPos, float yPos)
    {
        this.leftPos = leftPos;
        this.rightPos = rightPos;
        this.yPos = yPos;
    }
}

public class OnCoinSpawnedEvent: IGameEvent
{

    public List<CustomDiscPhysics2D> spawnedCoins;


    public OnCoinSpawnedEvent(List<CustomDiscPhysics2D> coins)
    {
        spawnedCoins = coins;
    }

}


public class OnStrikerSpawnedEvent : IGameEvent
{

    public  CustomDiscPhysics2D striker;


    public OnStrikerSpawnedEvent(CustomDiscPhysics2D striker)
    {
        this.striker = striker;
    }

}


public class OnStrikerHitEvent : IGameEvent
{

}


public class OnStrikeEndEvent : IGameEvent
{

}

public class DiscPocketedEvent : IGameEvent
{
    public CustomDiscPhysics2D disc;
    public DiscType discType;
    public Vector2 spawnPosition;
    public Vector2 holePosition;

    public DiscPocketedEvent(CustomDiscPhysics2D disc, DiscType discType, Vector2 spawnPosition, Vector2 holePosition)
    {
        this.disc = disc;
        this.discType = discType;
        this.spawnPosition = spawnPosition;
        this.holePosition = holePosition;
    }
}

public class TurnStartedEvent : IGameEvent
{
    public PlayerSide activePlayer;

    public TurnStartedEvent(PlayerSide activePlayer)
    {
        this.activePlayer = activePlayer;
    }
}

public class RequestStrikerShotEvent : IGameEvent
{
    public Vector2 direction;
    public float power;

    public RequestStrikerShotEvent(Vector2 direction, float power)
    {
        this.direction = direction;
        this.power = power;
    }
}

public class RequestStrikerWorldPositionEvent : IGameEvent
{
    public Vector2 worldPosition;

    public RequestStrikerWorldPositionEvent(Vector2 worldPosition)
    {
        this.worldPosition = worldPosition;
    }
}

public class RequestStrikerAimDirectionEvent : IGameEvent
{
    public Vector2 direction;

    public RequestStrikerAimDirectionEvent(Vector2 direction)
    {
        this.direction = direction;
    }
}

public class ScoreChangedEvent : IGameEvent
{
    public int humanScore;
    public int aiScore;

    public ScoreChangedEvent(int humanScore, int aiScore)
    {
        this.humanScore = humanScore;
        this.aiScore = aiScore;
    }
}

public class ShotResolvedEvent : IGameEvent
{
    public PlayerSide activePlayer;
    public bool getsExtraTurn;
    public bool turnSwitched;

    public ShotResolvedEvent(PlayerSide activePlayer, bool getsExtraTurn, bool turnSwitched)
    {
        this.activePlayer = activePlayer;
        this.getsExtraTurn = getsExtraTurn;
        this.turnSwitched = turnSwitched;
    }
}

public class PlayerDataChangedEvent : IGameEvent
{
    public int humanScore;
    public int aiScore;
    public PlayerSide activePlayer;

    public PlayerDataChangedEvent(int humanScore, int aiScore, PlayerSide activePlayer)
    {
        this.humanScore = humanScore;
        this.aiScore = aiScore;
        this.activePlayer = activePlayer;
    }
}





