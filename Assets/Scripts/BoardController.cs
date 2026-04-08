using System.Collections.Generic;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    public Transform strikerLeftPos;
    public Transform strikerRightPos;
    public Transform strikerCenter;
    public Transform coinParent;

    private readonly List<CustomDiscPhysics2D> allDiscs = new();

    private CustomDiscPhysics2D striker;
    private bool wasMoving;
    private bool canCheckCoinMovement;

    private void Awake()
    {
        EventManager.RaiseEvent(
            new SetStrikerPositionOffsetEvent(
                strikerLeftPos.position.x,
                strikerRightPos.position.x
            )
        );

        EventManager.AddListner<OnCoinSpawnedEvent>(OnCoinsSpawned);
        EventManager.AddListner<OnStrikerSpawnedEvent>(OnStrikerSpawned);
        EventManager.AddListner<OnStrikerHitEvent>(OnStrikerHit);
    }

    void OnStrikerHit(OnStrikerHitEvent e)
    {
        canCheckCoinMovement = true;
        wasMoving = true;
    }

    public Transform GetCoinParent() => coinParent;

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
        striker.transform.position = strikerCenter.position;
        striker.SetVelocity(Vector2.zero);

        canCheckCoinMovement = false;

        EventManager.RaiseEvent(new OnStrikeEndEvent());
    }
}