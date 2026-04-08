using System;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;

public class StrikerMoveHandler : MonoBehaviour
{
    public Slider strikerSlider;
    
    private float strikerPosLeftEnd, strikerPosRightEnd;


    private void Awake()
    {
        EventManager.AddListner<SetStrikerPositionOffsetEvent>(MoveStriker);
    }

    private void MoveStriker(SetStrikerPositionOffsetEvent offsets)
    {
        strikerPosLeftEnd = offsets.leftPos;
        strikerPosRightEnd = offsets.rightPos;
    }

    public void SetStrikerPositions(float leftEnd,float rightEnd)
    {
        strikerPosLeftEnd = leftEnd;
        strikerPosRightEnd = rightEnd;
    }

    private void OnEnable()
    {
        strikerSlider.onValueChanged.AddListener(OnSliderChanged);
    }

    private void OnDisable()
    {
        strikerSlider.onValueChanged.RemoveListener(OnSliderChanged);
    }

    private void OnSliderChanged(float value)
    {
        float targetX = Mathf.Lerp(
            strikerPosLeftEnd,
            strikerPosRightEnd,
            value
        );

        EventManager.RaiseEvent(new StrikerPositionChangedEvent(targetX));
    }
}
