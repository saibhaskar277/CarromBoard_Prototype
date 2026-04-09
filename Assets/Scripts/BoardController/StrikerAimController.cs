using System.Collections.Generic;
using UnityEngine;

public class StrikerAimController : MonoBehaviour
{
    [Header("References")]
    public CustomDiscPhysics2D strikerPhysics;
    public Collider2D aimTrigger;
    public Transform strikerVisual;

    [Header("Dots")]
    public GameObject dotPrefab;
    public int dotCount = 40;
    public Transform dotsParent;

    [Header("Aim")]
    public float maxPower = 10f;
    public float maxDragDistance = 2f;

    private readonly List<GameObject> dots = new();
    private bool isAiming;
    private Vector2 dragStart;
    private Vector2 shootDirection;
    private float shootPower;



    bool canAim = true;
    bool isHumanTurn = true;

    void Start()
    {
        CreateDots();
        HideDots();

        EventManager.AddListner<OnStrikeEndEvent>(OnStrikeEndEventListner);
        EventManager.AddListner<RequestStrikerShotEvent>(OnRequestStrikerShot);
        EventManager.AddListner<RequestStrikerWorldPositionEvent>(OnRequestStrikerWorldPosition);
        EventManager.AddListner<RequestStrikerAimDirectionEvent>(OnRequestStrikerAimDirection);
        EventManager.AddListner<TurnStartedEvent>(OnTurnStarted);

    }

    private void OnEnable()
    {
        EventManager.AddListner<StrikerPositionChangedEvent>(MoveStriker);
    }

    private void OnDisable()
    {
        EventManager.RemoveListner<OnStrikeEndEvent>(OnStrikeEndEventListner);
        EventManager.RemoveListner<StrikerPositionChangedEvent>(MoveStriker);
        EventManager.RemoveListner<RequestStrikerShotEvent>(OnRequestStrikerShot);
        EventManager.RemoveListner<RequestStrikerWorldPositionEvent>(OnRequestStrikerWorldPosition);
        EventManager.RemoveListner<RequestStrikerAimDirectionEvent>(OnRequestStrikerAimDirection);
        EventManager.RemoveListner<TurnStartedEvent>(OnTurnStarted);
    }

    void MoveStriker(StrikerPositionChangedEvent data)
    {
        if (canAim && isHumanTurn)
        {
            strikerVisual.position = data.worldPosition;
            transform.localPosition = Vector3.zero;
        }
    }



    void OnStrikeEndEventListner(OnStrikeEndEvent e)
    {
        canAim = true;
    }

    void OnTurnStarted(TurnStartedEvent e)
    {
        isHumanTurn = e.activePlayer == PlayerSide.Human;
        if (!isHumanTurn)
        {
            isAiming = false;
            HideDots();
        }
    }

    void OnRequestStrikerWorldPosition(RequestStrikerWorldPositionEvent e)
    {
        // Always apply — game/turn logic must reposition even while canAim is false.
        // OnStrikeEnd runs TurnGameManager before this script sets canAim = true, so
        // guarding here left the striker stuck on the opponent baseline after AI turns.
        Vector3 target = new Vector3(e.worldPosition.x, e.worldPosition.y, transform.position.z);
        strikerVisual.position = target;
        transform.localPosition = Vector3.zero;
    }

    void OnRequestStrikerAimDirection(RequestStrikerAimDirectionEvent e)
    {
        Vector2 d = e.direction.sqrMagnitude > 0.0001f ? e.direction.normalized : Vector2.down;
        strikerVisual.right = new Vector3(d.x, d.y, 0f);
    }

    void OnRequestStrikerShot(RequestStrikerShotEvent e)
    {
        if (!canAim)
            return;

        Vector2 direction = e.direction.sqrMagnitude > 0.0001f ? e.direction.normalized : Vector2.up;
        float power = Mathf.Clamp(e.power, 0f, maxPower);
        ShootWith(direction, power);
    }

    void Update()
    {
        if (canAim && isHumanTurn)
            HandleInput();
    }

    void CreateDots()
    {
        for (int i = 0; i < dotCount; i++)
        {
            GameObject dot = Instantiate(dotPrefab, dotsParent ? dotsParent : transform);
            dot.SetActive(false);
            dots.Add(dot);
        }
    }


    void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (aimTrigger.OverlapPoint(mouseWorld))
            {
                isAiming = true;
                dragStart = mouseWorld;
            }
        }

        if (!isAiming)
            return;

        if (Input.GetMouseButton(0))
        {
            Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 drag = mouseWorld - dragStart;

            float dragDistance = Mathf.Clamp(drag.magnitude, 0, maxDragDistance);
            float powerPercent = dragDistance / maxDragDistance;

            shootDirection = -drag.normalized;
            shootPower = powerPercent * maxPower;

            strikerVisual.right = shootDirection;

            UpdatePredictionDots();
        }

        if (Input.GetMouseButtonUp(0))
        {
            Shoot();
            isAiming = false;
            HideDots();
        }
    }


    void UpdatePredictionDots()
    {
        Vector2 startVelocity = (shootDirection * shootPower) / strikerPhysics.mass;

        List<Vector2> points = strikerPhysics.SimulatePath(
            strikerPhysics.transform.position,
            startVelocity,
            dotCount,
            Time.fixedDeltaTime
        );

        for (int i = 0; i < dots.Count; i++)
        {
            if (i < points.Count)
            {
                dots[i].transform.position = points[i];
                dots[i].SetActive(true);
            }
            else
            {
                dots[i].SetActive(false);
            }
        }
    }

    void HideDots()
    {
        foreach (var dot in dots)
            dot.SetActive(false);
    }

    void Shoot()
    {
        ShootWith(shootDirection, shootPower);
    }

    void ShootWith(Vector2 direction, float power)
    {
        strikerPhysics.SetVelocity(Vector2.zero);
        strikerPhysics.AddImpulse(direction * power);
        canAim = false;
        EventManager.RaiseEvent(new OnStrikerHitEvent());
    }
}
