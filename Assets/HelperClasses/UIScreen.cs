using UnityEngine;
using UnityEngine.UI;

public abstract class UIScreen : MonoBehaviour
{
    public ScreenType screenType;
    public bool isOverlay;

    public bool isDefaultScreen;

    public Button backButton;



    private void Awake()
    {
        if(!isDefaultScreen)
            backButton.onClick.AddListener(() => { UIController.Instance.Back(); });
    }


    public virtual void Open() => gameObject.SetActive(true);
    public virtual void Close() => gameObject.SetActive(false);
}