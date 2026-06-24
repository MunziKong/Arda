using UnityEngine;

public class GameManager : MonoBehaviour
{
    public bool IsGameStarted { get; private set; }
    public bool IsFullScreen { get; private set; } = true;
    public int ResolutionWidth { get; private set; } = 1920;
    public int ResolutionHeight { get; private set; } = 1080;

    public void SetGameStarted()
    {
        IsGameStarted = true;
    }

    public void ResetState()
    {
        IsGameStarted = false;
    }

    public void SetWindowMode(bool isFullScreen)
    {
        IsFullScreen = isFullScreen;

        if (isFullScreen)
        {
            Resolution native = Screen.currentResolution;
            int width = native.width;
            int height = Mathf.RoundToInt(width * 9f / 16f);
            Screen.SetResolution(width, height, FullScreenMode.FullScreenWindow);
        }
        else
        {
            Screen.SetResolution(ResolutionWidth, ResolutionHeight, FullScreenMode.Windowed);
        }
    }

    public void SetResolution(int width, int height)
    {
        ResolutionWidth = width;
        ResolutionHeight = height;

        if (IsFullScreen) return;

        Screen.SetResolution(width, height, FullScreenMode.Windowed);
    }
    public void ExitGame()
    {
        SaveManager.Instance.Save();
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}