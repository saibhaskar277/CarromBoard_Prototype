using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

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



    bool canAim= true;

    void Start()
    {
        CreateDots();
        HideDots();

        EventManager.AddListner<OnStrikeEndEvent>(OnStrikeEndEventListner);

    }

    private void OnEnable()
    {
        EventManager.AddListner<StrikerPositionChangedEvent>(MoveStriker);
    }

    private void OnDisable()
    {
        EventManager.RemoveListner<StrikerPositionChangedEvent>(MoveStriker);
    }

    void MoveStriker(StrikerPositionChangedEvent data)
    {
        if(canAim)
            strikerVisual.position = new Vector3(data.Position, transform.position.y, transform.position.z);
    }



    void OnStrikeEndEventListner(OnStrikeEndEvent e)
    {
        canAim = true;
    }

    void Update()
    {
        if(canAim)
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
        strikerPhysics.SetVelocity(Vector2.zero);
        strikerPhysics.AddImpulse(shootDirection * shootPower);
        canAim = false;
        EventManager.RaiseEvent(new OnStrikerHitEvent());

    }
}
