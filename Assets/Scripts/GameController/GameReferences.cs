using System.Collections.Generic;
using UnityEngine;

public struct DiscRuntimeData
{
    public DiscType discType;
    public Vector2 spawnPosition;
}

public static class GameReferences
{
    private static readonly Dictionary<int, DiscRuntimeData> discDataById = new Dictionary<int, DiscRuntimeData>();

    public static BoardController Board { get; private set; }
    public static CustomDiscPhysics2D Striker { get; private set; }

    public static void Reset()
    {
        Board = null;
        Striker = null;
        discDataById.Clear();
    }

    public static void RegisterBoard(BoardController board)
    {
        Board = board;
    }

    public static void RegisterStriker(CustomDiscPhysics2D striker)
    {
        Striker = striker;
        if (striker != null)
        {
            RegisterDisc(striker, DiscType.Striker, striker.transform.position);
        }
    }

    public static void RegisterDisc(CustomDiscPhysics2D disc, DiscType discType, Vector2 spawnPosition)
    {
        if (disc == null) return;
        discDataById[disc.GetInstanceID()] = new DiscRuntimeData
        {
            discType = discType,
            spawnPosition = spawnPosition
        };
    }

    public static bool TryGetDiscData(CustomDiscPhysics2D disc, out DiscRuntimeData data)
    {
        data = default;
        if (disc == null) return false;
        return discDataById.TryGetValue(disc.GetInstanceID(), out data);
    }
}
