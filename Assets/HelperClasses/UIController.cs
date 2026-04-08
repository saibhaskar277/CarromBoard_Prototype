using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public static UIController Instance;

    public List<UIScreen> screens;

    private Dictionary<ScreenType, UIScreen> screenMap = new();
    private Stack<UIScreen> screenStack = new();

    private void Awake()
    {
        Instance = this;

        foreach (var screen in screens)
        {
            screenMap[screen.screenType] = screen;
            if (screen.isDefaultScreen)
            {
                screenStack.Push(screen);
                screen.Open();
            }
            else
            {
                screen.Close();
            }
        }
    }

    public void Open(ScreenType type)
    {
        UIScreen screen = screenMap[type];

        if (!screen.isOverlay && screenStack.Count > 0)
            screenStack.Peek().Close();

        screen.Open();
        screenStack.Push(screen);
    }

    public void Back()
    {
        if (screenStack.Count <= 1)
            return;

        UIScreen current = screenStack.Pop();
        current.Close();

        screenStack.Peek().Open();
    }
}


public enum ScreenType
{
    MainMenu,
    Settings,
    Pause,
    GameOver
}