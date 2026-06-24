using UnityEngine;
using UnityEngine.SceneManagement;

public class BootLoader : MonoBehaviour
{
    private void Start()
    {
        GameCore.SceneTransitionManager.MoveToSceneDirect(
            SceneNames.GetSceneName(SceneType.Main),
            "main_start"
        );
    }
}