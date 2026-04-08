using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName ="BoardData")]
public class CarromBoardData : ScriptableObject
{
    public BoardController boardPRefab;

    public List<DiscData> discDatas;


    public DiscData GetDiscByType(DiscType type)
    {
        return discDatas.Find(x => x.DiscType == type);
    }
}



[System.Serializable]
public struct DiscData
{
    public DiscType DiscType;
    public CustomDiscPhysics2D prefab;
}



public enum DiscType
{
    Red,
    Black,
    White,
    Striker
}
