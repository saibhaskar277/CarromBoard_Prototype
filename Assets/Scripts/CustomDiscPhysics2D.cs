using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(CircleCollider2D))]
public class CustomDiscPhysics2D : MonoBehaviour
{
    [Header("Physics")]
    public float mass = 1f;
    public float friction = 0.985f;     // per fixed step
    public float restitution = 0.92f;   // bounce energy
    public float stopThreshold = 0.03f;
    public LayerMask borderMask;
    public LayerMask coinMask;

    [Header("Runtime")]
    public Vector2 velocity;

    private CircleCollider2D circle;
    private float radius;

    private static readonly Collider2D[] hitBuffer = new Collider2D[16];

    void Awake()
    {
        circle = GetComponent<CircleCollider2D>();
        radius = circle.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y);
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        if (velocity.sqrMagnitude <= stopThreshold * stopThreshold)
        {
            velocity = Vector2.zero;
            return;
        }

        MoveWithBorderBounce(dt);
        ResolveCoinCollisions();
        ApplyFriction();
    }

    public void AddImpulse(Vector2 impulse)
    {
        velocity += impulse / mass;
    }

    public void SetVelocity(Vector2 newVelocity)
    {
        velocity = newVelocity;
    }

    void MoveWithBorderBounce(float dt)
    {
        Vector2 position = transform.position;
        Vector2 move = velocity * dt;
        float distance = move.magnitude;

        if (distance <= 0.0001f)
            return;

        RaycastHit2D hit = Physics2D.CircleCast(position, radius, move.normalized, distance, borderMask);

        if (hit.collider != null)
        {
            // Move to contact point
            Vector2 hitPos = hit.centroid;
            transform.position = hitPos;

            // Reflect velocity like real physics wall bounce
            velocity = Vector2.Reflect(velocity, hit.normal) * restitution;

            // Move remaining distance after bounce
            float remaining = distance - hit.distance;
            if (remaining > 0f)
            {
                transform.position = (Vector2)transform.position + velocity.normalized * remaining;
            }
        }
        else
        {
            transform.position = position + move;
        }
    }

    void ResolveCoinCollisions()
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(coinMask);
        filter.useTriggers = false;

        int count = Physics2D.OverlapCircle(
            transform.position,
            radius * 1.05f,
            filter,
            hitBuffer
        );

        for (int i = 0; i < count; i++)
        {
            Collider2D otherCol = hitBuffer[i];
            if (otherCol == null || otherCol.gameObject == gameObject)
                continue;

            CustomDiscPhysics2D other = otherCol.GetComponent<CustomDiscPhysics2D>();
            if (other == null)
                continue;

            Vector2 posA = transform.position;
            Vector2 posB = other.transform.position;

            Vector2 normal = posB - posA;
            float dist = normal.magnitude;
            float minDist = radius + other.radius;

            if (dist <= 0.0001f || dist >= minDist)
                continue;

            normal /= dist;

            // Separate overlap first
            float penetration = minDist - dist;
            float totalMass = mass + other.mass;

            transform.position = posA - normal * (penetration * (other.mass / totalMass));
            other.transform.position = posB + normal * (penetration * (mass / totalMass));

            // 1D elastic collision along normal
            float v1 = Vector2.Dot(velocity, normal);
            float v2 = Vector2.Dot(other.velocity, normal);

            float newV1 = (v1 * (mass - other.mass) + 2f * other.mass * v2) / totalMass;
            float newV2 = (v2 * (other.mass - mass) + 2f * mass * v1) / totalMass;

            velocity += (newV1 - v1) * normal;
            other.velocity += (newV2 - v2) * normal;

            // energy loss from hit
            velocity *= restitution;
            other.velocity *= restitution;
        }
    }

    void ApplyFriction()
    {
        velocity *= friction;

        if (velocity.magnitude < stopThreshold)
            velocity = Vector2.zero;
    }

    public bool IsMoving()
    {
        return velocity.sqrMagnitude > stopThreshold * stopThreshold;
    }

    public static bool AnyMoving(IEnumerable<CustomDiscPhysics2D> discs)
    {
        foreach (var disc in discs)
        {
            if (disc != null && disc.IsMoving())
                return true;
        }
        return false;
    }

    // Predict exact future path using the same custom physics rules
    public List<Vector2> SimulatePath(Vector2 startPosition, Vector2 startVelocity, int steps, float stepTime)
    {
        List<Vector2> points = new List<Vector2>(steps);

        Vector2 simPos = startPosition;
        Vector2 simVel = startVelocity;

        for (int i = 0; i < steps; i++)
        {
            if (simVel.sqrMagnitude <= stopThreshold * stopThreshold)
                break;

            Vector2 move = simVel * stepTime;
            float distance = move.magnitude;

            if (distance > 0.0001f)
            {
                RaycastHit2D hit = Physics2D.CircleCast(simPos, radius, move.normalized, distance, borderMask);

                if (hit.collider != null)
                {
                    simPos = hit.centroid;
                    simVel = Vector2.Reflect(simVel, hit.normal) * restitution;

                    float remaining = distance - hit.distance;
                    if (remaining > 0f)
                        simPos += simVel.normalized * remaining;
                }
                else
                {
                    simPos += move;
                }
            }

            // stop preview on first coin hit
            Collider2D coinHit = Physics2D.OverlapCircle(simPos, radius * 1.05f, coinMask);
            if (coinHit != null && coinHit.gameObject != gameObject)
            {
                points.Add(simPos);
                break;
            }

            simVel *= friction;
            points.Add(simPos);
        }

        return points;
    }
}
