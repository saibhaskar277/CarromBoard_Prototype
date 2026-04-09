using System.Collections.Generic;
using UnityEngine;

public class CarromBoardSetup : MonoBehaviour
{
    Transform coinsParent;

    [Header("Layout")]
    public float spacing = 0.35f;

    private readonly List<CustomDiscPhysics2D> spawnedCoins = new();
    private CarromBoardData boardData;
    private BoardController spawnedBoard;
    private CustomDiscPhysics2D spawnedStriker;

    private void Start()
    {
        boardData = ConfigManager.GetConfig<CarromBoardData>();
        SetupBoard();
    }

    public void SetupBoard()
    {
        ClearBoard();

        SpawnBoard();
        SpawnStriker();
        SpawnCoins();

        EventManager.RaiseEvent(new OnCoinSpawnedEvent(spawnedCoins));
    }

    void SpawnBoard()
    {
        if (boardData.boardPRefab == null)
        {
            Debug.LogError("Board prefab missing in CarromBoardData");
            return;
        }

        spawnedBoard = Instantiate(
            boardData.boardPRefab,
            transform.position,
            Quaternion.identity,
            transform
        );

        GameReferences.RegisterBoard(spawnedBoard);
        EventManager.RaiseEvent(new OnBoardSpawnedEvent(spawnedBoard));

        coinsParent = spawnedBoard.GetComponent<BoardController>().GetCoinParent();
    }

    void SpawnCoins()
    {
        Vector2 center = transform.position;

        // Queen in center
        SpawnDisc(DiscType.Red, center);

        Vector2[] directions =
        {
            new Vector2(1, 0),
            new Vector2(0.5f, 0.866f),
            new Vector2(-0.5f, 0.866f),
            new Vector2(-1, 0),
            new Vector2(-0.5f, -0.866f),
            new Vector2(0.5f, -0.866f)
        };

        int colorIndex = 0;

        // First ring
        for (int i = 0; i < 6; i++)
        {
            Vector2 pos = center + directions[i] * spacing;
            SpawnAlternatingDisc(pos, ref colorIndex);
        }

        // Outer ring
        for (int i = 0; i < 6; i++)
        {
            Vector2 start = center + directions[i] * spacing * 2;
            Vector2 nextDir = directions[(i + 1) % 6];

            for (int j = 0; j < 2; j++)
            {
                Vector2 pos = Vector2.Lerp(start, center + nextDir * spacing * 2, j / 2f);
                SpawnAlternatingDisc(pos, ref colorIndex);
            }
        }
    }

    void SpawnStriker()
    {
        DiscData strikerData = boardData.GetDiscByType(DiscType.Striker);

        if (strikerData.prefab == null)
        {
            Debug.LogError("Striker prefab missing in CarromBoardData");
            return;
        }

        spawnedStriker = Instantiate(
            strikerData.prefab,
            transform.position,
            Quaternion.identity,
            transform
        );
        GameReferences.RegisterStriker(spawnedStriker);

        EventManager.RaiseEvent(new OnStrikerSpawnedEvent(spawnedStriker));


    }

    void SpawnAlternatingDisc(Vector2 position, ref int index)
    {
        DiscType type = (index % 2 == 0) ? DiscType.White : DiscType.Black;
        SpawnDisc(type, position);
        index++;
    }

    void SpawnDisc(DiscType type, Vector2 position)
    {
        DiscData data = boardData.GetDiscByType(type);

        if (data.prefab == null)
        {
            Debug.LogError($"Missing prefab for {type}");
            return;
        }

        CustomDiscPhysics2D disc = Instantiate(data.prefab, position, Quaternion.identity, coinsParent);
        GameReferences.RegisterDisc(disc, type, position);
        spawnedCoins.Add(disc);
    }

    void ClearBoard()
    {
        GameReferences.Reset();

        foreach (var coin in spawnedCoins)
        {
            if (coin != null)
                Destroy(coin.gameObject);
        }

        spawnedCoins.Clear();

        if (spawnedBoard != null)
            Destroy(spawnedBoard.gameObject);

        if (spawnedStriker != null)
            Destroy(spawnedStriker.gameObject);
    }
}
