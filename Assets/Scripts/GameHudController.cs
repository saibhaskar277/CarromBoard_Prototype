using UnityEngine;
using TMPro;

public class GameHudController : MonoBehaviour
{
    public TMP_Text humanScoreText;
    public TMP_Text aiScoreText;
    public TMP_Text turnText;
    public TMP_Text statusText;

    private void OnEnable()
    {
        EventManager.AddListner<PlayerDataChangedEvent>(OnPlayerDataChanged);
        EventManager.AddListner<ShotResolvedEvent>(OnShotResolved);
    }

    private void OnDisable()
    {
        EventManager.RemoveListner<PlayerDataChangedEvent>(OnPlayerDataChanged);
        EventManager.RemoveListner<ShotResolvedEvent>(OnShotResolved);
    }

    private void Start()
    {
        SetDefaultLabels();
    }

    private void SetDefaultLabels()
    {
        if (humanScoreText != null) humanScoreText.text = "Player: 0";
        if (aiScoreText != null) aiScoreText.text = "AI: 0";
        if (turnText != null) turnText.text = "Turn: Player";
        if (statusText != null) statusText.text = "Pocket your coin for extra turn";
    }

    private void OnPlayerDataChanged(PlayerDataChangedEvent e)
    {
        if (humanScoreText != null)
        {
            humanScoreText.text = "Player: " + e.humanScore;
        }

        if (aiScoreText != null)
        {
            aiScoreText.text = "AI: " + e.aiScore;
        }
    }

    private void OnTurnStarted(TurnStartedEvent e)
    {
        if (turnText != null)
        {
            turnText.text = e.activePlayer == PlayerSide.Human ? "Turn: Player" : "Turn: AI";
        }

        if (statusText != null)
        {
            statusText.text = e.activePlayer == PlayerSide.Human ? "Your move" : "AI is thinking...";
        }
    }

    private void OnShotResolved(ShotResolvedEvent e)
    {
        if (statusText == null)
        {
            return;
        }

        if (e.getsExtraTurn)
        {
            statusText.text = e.activePlayer == PlayerSide.Human ? "Great shot! Bonus turn" : "AI earned bonus turn";
            return;
        }

        statusText.text = e.activePlayer == PlayerSide.Human ? "Turn changed to Player" : "Turn changed to AI";
    }
}
