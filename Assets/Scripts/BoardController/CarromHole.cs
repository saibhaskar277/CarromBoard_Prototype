using UnityEngine;

public class CarromHole : MonoBehaviour
{
    [Header("Pocket Shape")]
    public float holeRadius = 0.28f;
    public float perfectCenterRadius = 0.12f;
    public float fastShotCenterBias = 0.65f;

    [Header("Speed Rules")]
    public float highSpeedThreshold = 12f;
    public float rimBounceMultiplier = 0.9f;

    public LayerMask discMask;

    private readonly Collider2D[] results = new Collider2D[10];

    private void FixedUpdate()
    {
        CheckHole();
    }

    void CheckHole()
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(discMask);
        filter.useTriggers = false;

        int count = Physics2D.OverlapCircle(
            transform.position,
            holeRadius,
            filter,
            results
        );

        for (int i = 0; i < count; i++)
        {
            var col = results[i];
            if (col == null) continue;

            var disc = col.GetComponent<CustomDiscPhysics2D>();
            if (disc == null || !disc.gameObject.activeSelf)
                continue;

            EvaluatePocket(disc);
        }
    }

    void EvaluatePocket(CustomDiscPhysics2D disc)
    {
        Vector2 discPos = disc.transform.position;
        Vector2 holeCenter = transform.position;

        Vector2 toCenter = (holeCenter - discPos);
        float dist = toCenter.magnitude;

        if (dist <= 0.0001f)
        {
            PocketDisc(disc);
            return;
        }

        Vector2 dirToCenter = toCenter.normalized;
        Vector2 velocityDir = disc.velocity.sqrMagnitude > 0.0001f
            ? disc.velocity.normalized
            : Vector2.zero;

        // Check if disc is actually moving into the hole center
        float centerAlignment = Vector2.Dot(velocityDir, dirToCenter);

        // Fast shots require much better center alignment
        bool isFast = disc.velocity.magnitude >= highSpeedThreshold;
        float requiredAlignment = isFast ? fastShotCenterBias : 0.15f;

        // Perfect center always pockets, even fast
        bool perfectCenter = dist <= perfectCenterRadius;

        if (perfectCenter || centerAlignment >= requiredAlignment)
        {
            PocketDisc(disc);
            return;
        }

        // Rim rejection: realistic carrom bounce-back on bad angle / too much speed
        RimBounce(disc, dirToCenter);
    }

    void RimBounce(CustomDiscPhysics2D disc, Vector2 inwardNormal)
    {
        Vector2 outwardNormal = -inwardNormal;

        disc.velocity = Vector2.Reflect(disc.velocity, outwardNormal) * rimBounceMultiplier;

        // push slightly outside hole radius so it doesn't instantly repocket
        disc.transform.position = (Vector2)transform.position + outwardNormal * (holeRadius + 0.03f);
    }

    void PocketDisc(CustomDiscPhysics2D disc)
    {
        DiscType discType = DiscType.White;
        Vector2 spawnPosition = disc.transform.position;
        if (GameReferences.TryGetDiscData(disc, out DiscRuntimeData data))
        {
            discType = data.discType;
            spawnPosition = data.spawnPosition;
        }

        EventManager.RaiseEvent(new DiscPocketedEvent(disc, discType, spawnPosition, transform.position));
        disc.SetVelocity(Vector2.zero);
        disc.gameObject.SetActive(false);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, holeRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, perfectCenterRadius);
    }
#endif
}
