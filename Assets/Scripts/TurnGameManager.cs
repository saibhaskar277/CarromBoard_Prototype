using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnGameManager : MonoBehaviour
{
    [Header("AI")]
    [Tooltip("Fallback delay if CarromAIConfig is missing or turnDelay is zero.")]
    public float aiTurnDelay = 0.5f;
    public CarromAIConfig aiConfig;

    [Header("Rules")]
    public int queenScore = 3;

    private readonly CommandExecutor commandExecutor = new CommandExecutor();
    private readonly EnemyController enemyController = new EnemyController();
    private readonly List<DiscPocketedEvent> pocketedThisShot = new List<DiscPocketedEvent>();
    private readonly List<CustomDiscPhysics2D> allCoins = new List<CustomDiscPhysics2D>();

    private CustomDiscPhysics2D strikerDisc;

    private PlayerSide activePlayer = PlayerSide.Human;
    private bool shotInProgress;

    private int humanScore;
    private int aiScore;

    private Vector3 playerStrikerLeftWorld;
    private Vector3 playerStrikerRightWorld;
    private float strikerLeftX;
    private float strikerRightX;
    private float opponentStrikerLeftX;
    private float opponentStrikerRightX;
    private float opponentStrikerY;
    private float playerBaseY;
    private bool hasBoardRange;
    private bool hasOpponentBoardRange;
    private bool hasPlayerBaseY;

    private float lastHumanSliderValue = 0.5f;
    private float lastAiStrikerT = 0.5f;

    private bool queenPocketedNeedsCover;
    private PlayerSide queenPocketedBy;
    private CustomDiscPhysics2D queenDisc;
    private Vector2 queenSpawnPosition;
    private float boardCenterY;
    private bool hasBoardCenter;

    private void Awake()
    {
        EventManager.AddListner<OnCoinSpawnedEvent>(OnCoinsSpawned);
        EventManager.AddListner<OnBoardSpawnedEvent>(OnBoardSpawned);
        EventManager.AddListner<OnStrikerSpawnedEvent>(OnStrikerSpawned);
        EventManager.AddListner<SetStrikerPositionOffsetEvent>(OnStrikerRangeSet);
        EventManager.AddListner<SetOpponentStrikerPositionOffsetEvent>(OnOpponentStrikerRangeSet);
        EventManager.AddListner<StrikerPositionChangedEvent>(OnHumanStrikerMoved);
        EventManager.AddListner<OnStrikerHitEvent>(OnStrikerHit);
        EventManager.AddListner<DiscPocketedEvent>(OnDiscPocketed);
        EventManager.AddListner<OnStrikeEndEvent>(OnStrikeEnd);
    }

    private void Start()
    {
        BroadcastScore();
        BeginTurn(PlayerSide.Human);
    }

    private void OnDestroy()
    {
        EventManager.RemoveListner<OnCoinSpawnedEvent>(OnCoinsSpawned);
        EventManager.RemoveListner<OnBoardSpawnedEvent>(OnBoardSpawned);
        EventManager.RemoveListner<OnStrikerSpawnedEvent>(OnStrikerSpawned);
        EventManager.RemoveListner<SetStrikerPositionOffsetEvent>(OnStrikerRangeSet);
        EventManager.RemoveListner<SetOpponentStrikerPositionOffsetEvent>(OnOpponentStrikerRangeSet);
        EventManager.RemoveListner<StrikerPositionChangedEvent>(OnHumanStrikerMoved);
        EventManager.RemoveListner<OnStrikerHitEvent>(OnStrikerHit);
        EventManager.RemoveListner<DiscPocketedEvent>(OnDiscPocketed);
        EventManager.RemoveListner<OnStrikeEndEvent>(OnStrikeEnd);
    }

    private void OnCoinsSpawned(OnCoinSpawnedEvent e)
    {
        allCoins.Clear();
        for (int i = 0; i < e.spawnedCoins.Count; i++)
        {
            CustomDiscPhysics2D coin = e.spawnedCoins[i];
            allCoins.Add(coin);
            if (GameReferences.TryGetDiscData(coin, out DiscRuntimeData data) && data.discType == DiscType.Red)
            {
                queenDisc = coin;
                queenSpawnPosition = data.spawnPosition;
            }
        }
    }

    private void OnBoardSpawned(OnBoardSpawnedEvent e)
    {
        if (e == null || e.BoardController == null)
        {
            return;
        }

        boardCenterY = e.BoardController.transform.position.y;
        hasBoardCenter = true;
    }

    private void OnStrikerSpawned(OnStrikerSpawnedEvent e)
    {
        strikerDisc = e.striker;
        if (strikerDisc != null)
        {
            playerBaseY = strikerDisc.transform.position.y;
            hasPlayerBaseY = true;
            PlaceStrikerForTurn(activePlayer);
        }
    }

    private void OnStrikerRangeSet(SetStrikerPositionOffsetEvent e)
    {
        playerStrikerLeftWorld = e.leftWorld;
        playerStrikerRightWorld = e.rightWorld;
        strikerLeftX = e.leftWorld.x;
        strikerRightX = e.rightWorld.x;
        hasBoardRange = true;
    }

    private void OnOpponentStrikerRangeSet(SetOpponentStrikerPositionOffsetEvent e)
    {
        opponentStrikerLeftX = e.leftPos;
        opponentStrikerRightX = e.rightPos;
        opponentStrikerY = e.yPos;
        hasOpponentBoardRange = true;
    }

    private void OnHumanStrikerMoved(StrikerPositionChangedEvent e)
    {
        if (activePlayer != PlayerSide.Human || !hasBoardRange)
        {
            return;
        }

        lastHumanSliderValue = ProjectOntoSegmentT(
            playerStrikerLeftWorld,
            playerStrikerRightWorld,
            e.worldPosition);
    }

    private static float ProjectOntoSegmentT(Vector3 a, Vector3 b, Vector3 p)
    {
        Vector3 ab = b - a;
        float lenSq = ab.sqrMagnitude;
        if (lenSq <= 1e-8f)
        {
            return 0.5f;
        }

        return Mathf.Clamp01(Vector3.Dot(p - a, ab) / lenSq);
    }

    private void OnStrikerHit(OnStrikerHitEvent e)
    {
        shotInProgress = true;
        pocketedThisShot.Clear();
    }

    private void OnDiscPocketed(DiscPocketedEvent e)
    {
        if (!shotInProgress)
        {
            return;
        }

        pocketedThisShot.Add(e);
    }

    private void OnStrikeEnd(OnStrikeEndEvent e)
    {
        if (!shotInProgress)
        {
            PlaceStrikerForTurn(activePlayer);
            if (activePlayer == PlayerSide.AI)
            {
                StartCoroutine(RunAiTurn());
            }
            return;
        }

        shotInProgress = false;
        bool getsExtraTurn = ResolveShotOutcome();
        bool turnSwitched = !getsExtraTurn;

        if (turnSwitched)
        {
            activePlayer = GetOpponent(activePlayer);
        }

        EventManager.RaiseEvent(new ShotResolvedEvent(activePlayer, getsExtraTurn, turnSwitched));
        BeginTurn(activePlayer);
    }

    private void BeginTurn(PlayerSide side)
    {
        activePlayer = side;
        PlaceStrikerForTurn(activePlayer);
        EventManager.RaiseEvent(new TurnStartedEvent(activePlayer));
        BroadcastPlayerData();

        if (activePlayer == PlayerSide.AI)
        {
            StartCoroutine(RunAiTurn());
        }
    }

    private void PlaceStrikerForTurn(PlayerSide side)
    {
        if (strikerDisc == null || !hasBoardRange)
        {
            return;
        }

        Vector2 target;

        if (side == PlayerSide.Human)
        {
            Vector3 onBaseline = Vector3.Lerp(
                playerStrikerLeftWorld,
                playerStrikerRightWorld,
                lastHumanSliderValue);
            target = new Vector2(onBaseline.x, onBaseline.y);
        }
        else
        {
            if (!hasPlayerBaseY)
            {
                return;
            }

            float left = hasOpponentBoardRange ? opponentStrikerLeftX : strikerLeftX;
            float right = hasOpponentBoardRange ? opponentStrikerRightX : strikerRightX;
            float x = Mathf.Lerp(left, right, lastAiStrikerT);
            float y = hasOpponentBoardRange ? opponentStrikerY : GetOppositeBaselineY();
            target = new Vector2(x, y);
        }

        commandExecutor.Execute(new PositionStrikerCommand(target));
        strikerDisc.SetVelocity(Vector2.zero);
    }

    private float GetOppositeBaselineY()
    {
        float centerY = hasBoardCenter ? boardCenterY : 0f;
        return centerY - (playerBaseY - centerY);
    }

    private IEnumerator RunAiTurn()
    {
        if (shotInProgress || activePlayer != PlayerSide.AI || strikerDisc == null)
        {
            yield break;
        }

        if (aiConfig == null)
        {
            yield break;
        }

        CustomDiscPhysics2D[] aiTargets = GetTargetsFor(PlayerSide.AI);
        Vector2[] holes = GameReferences.Board != null ? GameReferences.Board.GetHolePositions() : new Vector2[0];

        float left = hasOpponentBoardRange ? opponentStrikerLeftX : strikerLeftX;
        float right = hasOpponentBoardRange ? opponentStrikerRightX : strikerRightX;
        float y = hasOpponentBoardRange ? opponentStrikerY : GetOppositeBaselineY();
        float width = right - left;

        bool hasPlan = enemyController.TryGetShotPlan(
            aiConfig,
            strikerDisc,
            new Vector2(left, right),
            y,
            aiTargets,
            holes,
            out Vector2 plannedStrikerPos,
            out Vector2 plannedDir,
            out float plannedPower
        );

        if (!hasPlan)
        {
            activePlayer = PlayerSide.Human;
            BeginTurn(activePlayer);
            yield break;
        }

        if (Mathf.Abs(width) > 0.0001f)
        {
            lastAiStrikerT = Mathf.Clamp01(Mathf.InverseLerp(left, right, plannedStrikerPos.x));
        }

        yield return AiThinkingRoamOnBaseline(left, right, y);

        if (shotInProgress || activePlayer != PlayerSide.AI || strikerDisc == null)
        {
            yield break;
        }

        float aimLerp = Mathf.Max(0.02f, aiConfig.aimLerpDuration);
        yield return SmoothLerpStrikerTo(plannedStrikerPos, aimLerp);

        Vector2 shotDir = plannedDir.sqrMagnitude > 0.0001f ? plannedDir.normalized : Vector2.down;
        EventManager.RaiseEvent(new RequestStrikerAimDirectionEvent(shotDir));
        yield return null;

        commandExecutor.Execute(new ShootStrikerCommand(plannedDir, plannedPower));
    }

    private IEnumerator AiThinkingRoamOnBaseline(float left, float right, float baselineY)
    {
        float thinkingSeconds = aiConfig != null && aiConfig.turnDelay > 0f
            ? aiConfig.turnDelay
            : aiTurnDelay;

        float interval = aiConfig != null
            ? Mathf.Max(0.04f, aiConfig.jitterRetargetInterval)
            : 0.1f;
        float moveSpeed = aiConfig != null ? aiConfig.aiThinkingMoveSpeed : 2.5f;
        float wobble = aiConfig != null ? aiConfig.aiThinkingAimWobbleDegrees : 35f;

        float elapsed = 0f;
        float nextPick = 0f;
        Vector2 roamTarget = strikerDisc != null
            ? (Vector2)strikerDisc.transform.position
            : new Vector2(Mathf.Lerp(left, right, 0.5f), baselineY);

        while (elapsed < thinkingSeconds && activePlayer == PlayerSide.AI && strikerDisc != null)
        {
            if (elapsed >= nextPick)
            {
                float t = Random.value;
                roamTarget = new Vector2(Mathf.Lerp(left, right, t), baselineY);
                float ang = Random.Range(-wobble, wobble);
                Vector2 wobbleDir = (Vector2)(Quaternion.Euler(0f, 0f, ang) * Vector2.down);
                if (wobbleDir.sqrMagnitude < 0.0001f)
                {
                    wobbleDir = Vector2.down;
                }

                EventManager.RaiseEvent(new RequestStrikerAimDirectionEvent(wobbleDir.normalized));
                nextPick += interval;
            }

            Vector2 cur = strikerDisc.transform.position;
            Vector2 next = Vector2.MoveTowards(cur, roamTarget, moveSpeed * Time.deltaTime);
            EventManager.RaiseEvent(new RequestStrikerWorldPositionEvent(next));

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator SmoothLerpStrikerTo(Vector2 target, float duration)
    {
        if (strikerDisc == null)
        {
            yield break;
        }

        Vector2 start = strikerDisc.transform.position;
        float dur = Mathf.Max(0.02f, duration);
        float elapsed = 0f;

        while (elapsed < dur && strikerDisc != null && activePlayer == PlayerSide.AI)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / dur);
            t = t * t * (3f - 2f * t);
            Vector2 pos = Vector2.Lerp(start, target, t);
            EventManager.RaiseEvent(new RequestStrikerWorldPositionEvent(pos));
            yield return null;
        }

        if (strikerDisc != null)
        {
            EventManager.RaiseEvent(new RequestStrikerWorldPositionEvent(target));
        }
    }

    private CustomDiscPhysics2D[] GetTargetsFor(PlayerSide player)
    {
        DiscType desiredType = GetPlayerDiscType(player);
        List<CustomDiscPhysics2D> targets = new List<CustomDiscPhysics2D>();

        for (int i = 0; i < allCoins.Count; i++)
        {
            CustomDiscPhysics2D coin = allCoins[i];
            if (coin == null || !coin.gameObject.activeSelf)
            {
                continue;
            }

            if (!GameReferences.TryGetDiscData(coin, out DiscRuntimeData data) || data.discType != desiredType)
            {
                continue;
            }

            targets.Add(coin);
        }

        if (targets.Count == 0 && queenDisc != null && queenDisc.gameObject.activeSelf)
        {
            targets.Add(queenDisc);
        }

        return targets.ToArray();
    }

    private bool ResolveShotOutcome()
    {
        int ownPocketed = 0;
        int opponentPocketed = 0;
        bool strikerPocketed = false;
        bool queenPocketedNow = false;

        DiscType ownDiscType = GetPlayerDiscType(activePlayer);
        DiscType opponentDiscType = GetPlayerDiscType(GetOpponent(activePlayer));

        for (int i = 0; i < pocketedThisShot.Count; i++)
        {
            DiscPocketedEvent pocket = pocketedThisShot[i];
            if (pocket == null)
            {
                continue;
            }

            if (pocket.discType == DiscType.Striker)
            {
                strikerPocketed = true;
                continue;
            }

            if (pocket.discType == DiscType.Red)
            {
                queenPocketedNow = true;
                continue;
            }

            if (pocket.discType == ownDiscType)
            {
                ownPocketed++;
            }
            else if (pocket.discType == opponentDiscType)
            {
                opponentPocketed++;
            }
        }

        AddScore(activePlayer, ownPocketed);
        AddScore(GetOpponent(activePlayer), opponentPocketed);

        bool getsExtraTurn = ownPocketed > 0 && !strikerPocketed;

        if (queenPocketedNow)
        {
            queenPocketedNeedsCover = true;
            queenPocketedBy = activePlayer;
            getsExtraTurn = true;
        }

        if (queenPocketedNeedsCover && queenPocketedBy == activePlayer && !queenPocketedNow)
        {
            if (ownPocketed > 0 && !strikerPocketed)
            {
                AddScore(activePlayer, queenScore);
                queenPocketedNeedsCover = false;
            }
            else
            {
                RespotQueen();
                queenPocketedNeedsCover = false;
            }
        }

        if (strikerPocketed)
        {
            getsExtraTurn = false;
        }

        pocketedThisShot.Clear();
        return getsExtraTurn;
    }

    private void RespotQueen()
    {
        if (queenDisc == null)
        {
            return;
        }

        queenDisc.gameObject.SetActive(true);
        queenDisc.transform.position = queenSpawnPosition;

        CustomDiscPhysics2D queenPhysics = queenDisc.GetComponent<CustomDiscPhysics2D>();
        if (queenPhysics != null)
        {
            queenPhysics.SetVelocity(Vector2.zero);
        }
    }

    private void AddScore(PlayerSide player, int value)
    {
        if (value <= 0)
        {
            return;
        }

        if (player == PlayerSide.Human)
        {
            humanScore += value;
        }
        else
        {
            aiScore += value;
        }

        BroadcastScore();
    }

    private void BroadcastScore()
    {
        EventManager.RaiseEvent(new ScoreChangedEvent(humanScore, aiScore));
        BroadcastPlayerData();
    }

    private void BroadcastPlayerData()
    {
        EventManager.RaiseEvent(new PlayerDataChangedEvent(humanScore, aiScore, activePlayer));
    }

    private static PlayerSide GetOpponent(PlayerSide side)
    {
        return side == PlayerSide.Human ? PlayerSide.AI : PlayerSide.Human;
    }

    private static DiscType GetPlayerDiscType(PlayerSide player)
    {
        return player == PlayerSide.Human ? DiscType.White : DiscType.Black;
    }

}

