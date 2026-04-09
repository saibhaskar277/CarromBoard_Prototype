using System.Collections.Generic;
using UnityEngine;

public class EnemyController
{
    public bool TryGetShotPlan(
        CarromAIConfig cfg,
        CustomDiscPhysics2D striker,
        Vector2 strikerRangeLeftRight,
        float strikerY,
        CustomDiscPhysics2D[] candidateCoins,
        Vector2[] holePositions,
        out Vector2 strikerPosition,
        out Vector2 shotDirection,
        out float power
    )
    {
        strikerPosition = Vector2.zero;
        shotDirection = Vector2.up;
        power = cfg != null ? cfg.minPower : 3f;

        if (cfg == null || striker == null || candidateCoins == null || candidateCoins.Length == 0
            || holePositions == null || holePositions.Length == 0)
        {
            return false;
        }

        float strikerRadius = striker.GetRadius();
        float leftX = Mathf.Min(strikerRangeLeftRight.x, strikerRangeLeftRight.y);
        float rightX = Mathf.Max(strikerRangeLeftRight.x, strikerRangeLeftRight.y);
        int samples = Mathf.Max(2, cfg.strikerSampleCount);

        float bestScore = float.MinValue;
        Vector2 bestPos = Vector2.zero;
        Vector2 bestDir = Vector2.up;
        float bestPower = cfg.minPower;

        for (int s = 0; s < samples; s++)
        {
            float t = samples == 1 ? 0.5f : s / (float)(samples - 1);
            float x = Mathf.Lerp(leftX, rightX, t);
            Vector2 sampleStrikerPos = new Vector2(x, strikerY);

            for (int c = 0; c < candidateCoins.Length; c++)
            {
                CustomDiscPhysics2D coin = candidateCoins[c];
                if (coin == null || !coin.gameObject.activeSelf)
                {
                    continue;
                }

                float coinRadius = coin.GetRadius();
                Vector2 coinPos = coin.transform.position;

                for (int h = 0; h < holePositions.Length; h++)
                {
                    Vector2 hole = holePositions[h];
                    Vector2 toHole = hole - coinPos;
                    if (toHole.sqrMagnitude <= 0.0001f)
                    {
                        continue;
                    }

                    Vector2 holeDir = toHole.normalized;
                    float contactSep = strikerRadius + coinRadius + cfg.contactSeparationPadding;
                    Vector2 aimPoint = coinPos - holeDir * contactSep;

                    Vector2 rawDir = aimPoint - sampleStrikerPos;
                    if (rawDir.sqrMagnitude <= 0.0001f)
                    {
                        continue;
                    }

                    Vector2 dir = rawDir.normalized;
                    float distToAim = rawDir.magnitude;
                    float candidatePower = Mathf.Clamp(
                        distToAim * cfg.powerDistanceMultiplier,
                        cfg.minPower,
                        cfg.maxPower);

                    float errorScale = 1f - Mathf.Clamp01(cfg.accuracy);
                    float maxAngleErr = Mathf.Lerp(cfg.minAngleErrorDegrees, cfg.maxAngleErrorDegrees, errorScale);
                    float angleNoise = Random.Range(-maxAngleErr, maxAngleErr);
                    Vector2 noisyDir = (Quaternion.Euler(0f, 0f, angleNoise) * dir).normalized;

                    Vector2 startVelocity = (noisyDir * candidatePower) / striker.mass;
                    List<Vector2> path = striker.SimulatePath(
                        sampleStrikerPos,
                        startVelocity,
                        cfg.predictionSteps,
                        cfg.predictionStepTime);

                    float pocketLineScore = PocketLineAlignment(sampleStrikerPos, coinPos, hole);
                    float distScore = 1f / (1f + distToAim * 0.35f);
                    float holeProximity = 1f / (1f + toHole.magnitude * 0.08f);

                    float pathBonus = 0f;
                    if (path.Count > 0 && path.Count < cfg.predictionSteps)
                    {
                        Vector2 hitProbe = path[path.Count - 1];
                        if (TryGetCoinAt(striker, hitProbe, out CustomDiscPhysics2D hitCoin)
                            && hitCoin == coin)
                        {
                            pathBonus = cfg.pathHitsTargetWeight;
                        }
                    }

                    float endNearCoin = 0f;
                    if (path.Count > 0)
                    {
                        float d = Vector2.Distance(path[path.Count - 1], coinPos);
                        endNearCoin = 1f / (1f + d * 2f);
                    }

                    float score =
                        pocketLineScore * cfg.pocketLineWeight
                        + distScore * cfg.distanceWeight
                        + holeProximity * cfg.holeProximityWeight
                        + pathBonus
                        + endNearCoin * cfg.directHitWeight;

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestPos = sampleStrikerPos;
                        bestDir = noisyDir;
                        bestPower = candidatePower;
                    }
                }
            }
        }

        if (bestScore == float.MinValue)
        {
            return false;
        }

        strikerPosition = bestPos;
        shotDirection = bestDir;
        power = bestPower;
        return true;
    }

    private static bool TryGetCoinAt(CustomDiscPhysics2D striker, Vector2 worldPos, out CustomDiscPhysics2D coin)
    {
        coin = null;
        Collider2D col = Physics2D.OverlapCircle(worldPos, striker.GetRadius() * 1.08f, striker.GetCoinMask());
        if (col == null)
        {
            return false;
        }

        coin = col.GetComponent<CustomDiscPhysics2D>();
        return coin != null;
    }

    private static float PocketLineAlignment(Vector2 strikerPos, Vector2 coinPos, Vector2 hole)
    {
        Vector2 toCoin = coinPos - strikerPos;
        Vector2 toHole = hole - coinPos;
        if (toCoin.sqrMagnitude <= 1e-6f || toHole.sqrMagnitude <= 1e-6f)
        {
            return 0f;
        }

        float dot = Vector2.Dot(toCoin.normalized, toHole.normalized);
        return (dot + 1f) * 0.5f;
    }
}
