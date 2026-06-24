using UnityEngine;

public class MainUIManager : MonoBehaviour
{
    public static MainUIManager Instance { get; private set; }
    [SerializeField] private GameObject _mainUI;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void ActiveMainUI()
    {
        _mainUI.SetActive(true);
        GameCore.GameController.InactiveCoreUI();
    }

    public void InActiveMainUI()
    {
        _mainUI.SetActive(false);
    }
}