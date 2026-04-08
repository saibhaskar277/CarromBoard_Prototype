
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
    public float Position;

    public StrikerPositionChangedEvent(float position)
    {
        Position = position;
    }
}



public class SetStrikerPositionOffsetEvent : IGameEvent
{
    public float leftPos;
    public float rightPos;

    public SetStrikerPositionOffsetEvent(float leftPos,float rightPos)
    {
        this.leftPos = leftPos;
        this.rightPos = rightPos;
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






