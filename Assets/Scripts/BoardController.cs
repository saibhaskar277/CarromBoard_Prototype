using System.Collections.Generic;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    public Transform strikerLeftPos;
    public Transform strikerRightPos;
    public Transform opponentStrikerLeftPos;
    public Transform opponentStrikerRightPos;
    public Transform opponentStrikerCenter;
    public Transform strikerCenter;
    public Transform coinParent;

    private readonly List<CustomDiscPhysics2D> allDiscs = new();

    private CustomDiscPhysics2D striker;
    private bool wasMoving;
    private bool canCheckCoinMovement;

    private void Awake()
    {
        EventManager.AddListner<OnCoinSpawnedEvent>(OnCoinsSpawned);
        EventManager.AddListner<OnStrikerSpawnedEvent>(OnStrikerSpawned);
        EventManager.AddListner<OnStrikerHitEvent>(OnStrikerHit);
    }

    private void Start()
    {
        EventManager.RaiseEvent(
            new SetStrikerPositionOffsetEvent(
                strikerLeftPos.position,
                strikerRightPos.position
            )
        );

        if (opponentStrikerLeftPos != null && opponentStrikerRightPos != null)
        {
            float y = opponentStrikerCenter != null ? opponentStrikerCenter.position.y : strikerCenter.position.y;
            EventManager.RaiseEvent(
                new SetOpponentStrikerPositionOffsetEvent(
                    opponentStrikerLeftPos.position.x,
                    opponentStrikerRightPos.position.x,
                    y
                )
            );
        }
    }

    void OnStrikerHit(OnStrikerHitEvent e)
    {
        canCheckCoinMovement = true;
        wasMoving = true;
    }

    public Transform GetCoinParent() => coinParent;

    public Vector2[] GetHolePositions()
    {
        CarromHole[] holes = GetComponentsInChildren<CarromHole>(true);
        Vector2[] result = new Vector2[holes.Length];
        for (int i = 0; i < holes.Length; i++)
        {
            result[i] = holes[i].transform.position;
        }
        return result;
    }

    void OnStrikerSpawned(OnStrikerSpawnedEvent e)
    {
        striker = e.striker;
        striker.transform.position = strikerCenter.position;

        RefreshDiscList();
    }

    void OnCoinsSpawned(OnCoinSpawnedEvent e)
    {
        allDiscs.Clear();
        allDiscs.AddRange(e.spawnedCoins);

        RefreshDiscList();
    }

    void RefreshDiscList()
    {
        if (striker != null && !allDiscs.Contains(striker))
            allDiscs.Add(striker);
    }

    void Update()
    {
        if (!canCheckCoinMovement)
            return;

        bool isMoving = CustomDiscPhysics2D.AnyMoving(allDiscs);

        if (wasMoving && !isMoving)
        {
            OnAllStopped();
        }

        wasMoving = isMoving;
    }

    void OnAllStopped()
    {
        ResetStriker();
    }

    void ResetStriker()
    {
        if (striker != null)
        {
            striker.SetVelocity(Vector2.zero);
        }

        canCheckCoinMovement = false;

        EventManager.RaiseEvent(new OnStrikeEndEvent());
    }
}