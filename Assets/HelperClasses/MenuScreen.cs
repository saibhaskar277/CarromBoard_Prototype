using UnityEngine;
using UnityEngine.UI;

public class MenuScreen : UIScreen
{

    public Button optionsButton;



    private void Awake()
    {
        optionsButton.onClick.AddListener(() => { UIController.Instance.Open(ScreenType.Settings); });
    }


}
