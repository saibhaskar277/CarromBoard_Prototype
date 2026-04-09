using UnityEngine;
using UnityEngine.UI;

public class StrikerMoveHandler : MonoBehaviour
{
    public Slider strikerSlider;

    private Vector3 strikerPosLeftWorld;
    private Vector3 strikerPosRightWorld;

    private void Awake()
    {
        EventManager.AddListner<SetStrikerPositionOffsetEvent>(OnBaselineSet);
        EventManager.AddListner<TurnStartedEvent>(OnTurnStarted);
    }

    private void OnBaselineSet(SetStrikerPositionOffsetEvent offsets)
    {
        strikerPosLeftWorld = offsets.leftWorld;
        strikerPosRightWorld = offsets.rightWorld;
        if (strikerSlider != null)
            ApplySliderToWorldPosition(strikerSlider.value);
    }

    public void SetStrikerBaselineWorld(Vector3 leftWorld, Vector3 rightWorld)
    {
        strikerPosLeftWorld = leftWorld;
        strikerPosRightWorld = rightWorld;
    }

    private void OnEnable()
    {
        strikerSlider.onValueChanged.AddListener(OnSliderChanged);
    }

    private void OnDisable()
    {
        if (strikerSlider != null)
            strikerSlider.onValueChanged.RemoveListener(OnSliderChanged);
    }

    private void OnTurnStarted(TurnStartedEvent e)
    {
        if (strikerSlider != null)
        {
            strikerSlider.interactable = e.activePlayer == PlayerSide.Human;
            if (e.activePlayer == PlayerSide.Human)
            {
                ApplySliderToWorldPosition(strikerSlider.value);
            }
        }
    }

    private void OnDestroy()
    {
        EventManager.RemoveListner<SetStrikerPositionOffsetEvent>(OnBaselineSet);
        EventManager.RemoveListner<TurnStartedEvent>(OnTurnStarted);
    }

    private void OnSliderChanged(float value)
    {
        ApplySliderToWorldPosition(value);
    }

    private void ApplySliderToWorldPosition(float slider01)
    {
        Vector3 world = Vector3.Lerp(strikerPosLeftWorld, strikerPosRightWorld, slider01);
        EventManager.RaiseEvent(new StrikerPositionChangedEvent(world));
    }
}
