using UnityEngine;

[CreateAssetMenu(fileName = "CarromAIConfig", menuName = "Carrom/AI Config")]
public class CarromAIConfig : ScriptableObject
{
    [Header("Timing")]
    [Tooltip("Seconds the AI spends \"thinking\": smooth roam + random aim wobble on the baseline before moving to the final aim.")]
    public float turnDelay = 0.45f;

    [Tooltip("How often to pick a new random roam target along the opponent line during thinking.")]
    public float jitterRetargetInterval = 0.1f;

    [Tooltip("World units per second while roaming toward random baseline points.")]
    public float aiThinkingMoveSpeed = 2.8f;

    [Tooltip("Max random aim rotation (degrees) applied each retarget during thinking.")]
    public float aiThinkingAimWobbleDegrees = 40f;

    [Tooltip("Seconds to smoothly slide the striker to the final shot position (not instant).")]
    public float aimLerpDuration = 0.4f;

    [Header("Aim")]
    [Tooltip("1 = very precise shots, 0 = large angular error.")]
    [Range(0f, 1f)]
    public float accuracy = 0.82f;

    [Tooltip("Angular error in degrees at accuracy = 1.")]
    public float minAngleErrorDegrees = 0.12f;

    [Tooltip("Angular error in degrees at accuracy = 0.")]
    public float maxAngleErrorDegrees = 14f;

    [Tooltip("Extra space along coin–hole line for contact point (tweak if clips).")]
    public float contactSeparationPadding = 0.02f;

    [Header("Power")]
    public float minPower = 3f;
    public float maxPower = 10f;
    public float powerDistanceMultiplier = 6f;

    [Header("Search")]
    [Tooltip("How many positions along opponent striker baseline to try.")]
    public int strikerSampleCount = 11;

    [Header("Prediction (matches CustomDiscPhysics2D.SimulatePath)")]
    public int predictionSteps = 48;
    public float predictionStepTime = 0.02f;

    [Header("Scoring weights")]
    [Tooltip("Striker → coin → hole alignment.")]
    public float pocketLineWeight = 1.2f;

    public float distanceWeight = 0.45f;
    public float holeProximityWeight = 0.35f;

    [Tooltip("Bonus when simulated path first hits the intended coin.")]
    public float pathHitsTargetWeight = 2f;

    public float directHitWeight = 0.55f;
}
