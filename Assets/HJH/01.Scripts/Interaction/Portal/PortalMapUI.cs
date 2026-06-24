using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PortalMapUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject _mapUI;

    [Header("Map Select Panel")]
    [SerializeField] private GameObject _mapSelectPanel;

    [Header("Map Slots")]
    [SerializeField] private MapSlotUI _forestSlot;
    [SerializeField] private MapSlotUI _valleySlot;
    [SerializeField] private MapSlotUI _dungeonSlot;

    [Header("Map Area")]
    [SerializeField] private GameObject _forestMap;

    [Header("Portal Buttons")]
    [SerializeField] private PortalDestinationButton[] _forestPortalBtns;

    [Header("Result")]
    [SerializeField] private TMP_Text _selectMapTxt;
    [SerializeField] private TMP_Text _selectPortalTxt;
    [SerializeField] private Button _enterBtn;
    [SerializeField] private Button _closeBtn;

    [Header("Confirm Panel")]
    [SerializeField] private GameObject _confirmPanel;
    [SerializeField] private Button _portalSelectBtn;
    [SerializeField] private Button _returnBtn;

    [Header("Input")]
    [SerializeField] private PlayerInputs _playerInputs;

    private PortalDestinationButton _selectedPortal;
    private bool _isInitialized;

    private void Awake()
    {
        _enterBtn.onClick.AddListener(OnEnterClicked);
        _closeBtn.onClick.AddListener(() => { SoundManager.Instance?.PlayButtonClick(); CloseUI(); });

        if (_portalSelectBtn != null)
        {
            _portalSelectBtn.onClick.AddListener(OnPortalSelectClicked);
        }

        if (_returnBtn != null)
        {
            _returnBtn.onClick.AddListener(OnReturnClicked);
        }

        _mapUI.SetActive(false);
    }

    private void OnEnable()
    {
        _playerInputs.CloseEvent += CloseUI;
    }

    private void OnDisable()
    {
        _playerInputs.CloseEvent -= CloseUI;
    }

    private void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }

        InitializeSlots();

        _isInitialized = true;
    }

    private void InitializeSlots()
    {
        _forestSlot.Initialize(OnForestSlotSelected);
        _valleySlot.Initialize(null);
        _dungeonSlot.Initialize(null);
    }

    private void InitializePortalButtons()
    {
        foreach (PortalDestinationButton btn in _forestPortalBtns)
        {
            // Debug.Log($"[Portal] Initialize: {btn.name} / PortalNumber: {btn.PortalNumber}");
            btn.Initialize(OnPortalSelected);
        }
    }

    public void OpenUI()
    {
        _mapUI.SetActive(true);

        Initialize();
        InitializePortalButtons();

        GameCore.PlayerInputs.SetCursorLock(false);
        GameCore.PlayerInputs.SetUIInput();

        ResetSelection();

        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == SceneNames.GetSceneName(SceneType.Main))
        {
            OpenMapSelectPanel();
        }
        else
        {
            OpenConfirmPanel();
        }
    }

    public void ForceCloseUI()
    {
        _mapUI.SetActive(false);
    }

    public void CloseUI()
    {
        if (!_mapUI.activeSelf)
        {
            return;
        }
        GameCore.PlayerInputs.SetCursorLock(true);
        GameCore.PlayerInputs.SetPlayerInput();

        _mapUI.SetActive(false);
    }

    private void OpenMapSelectPanel()
    {
        if (_confirmPanel != null)
        {
            _confirmPanel.SetActive(false);
        }

        _mapSelectPanel.SetActive(true);
        SelectForestMap();
    }

    public void OpenConfirmPanel()
    {
        _mapSelectPanel.SetActive(false);

        if (_confirmPanel != null)
        {
            _confirmPanel.SetActive(true);
        }
    }

    private void OnForestSlotSelected()
    {
        SelectForestMap();
    }

    private void SelectForestMap()
    {
        _forestSlot.SetSelected(true);
        _valleySlot.SetSelected(false);
        _dungeonSlot.SetSelected(false);

        _forestMap.SetActive(true);
        _selectMapTxt.text = "현재 선택된 맵 : 녹색 숲";
    }

    private void OnPortalSelected(PortalDestinationButton portal)
    {
        _selectedPortal = portal;

        foreach (PortalDestinationButton btn in _forestPortalBtns)
        {
            btn.SetSelected(btn == portal);
        }

        _selectPortalTxt.text = $"현재 선택된 포탈 : {portal.PortalNumber}번 포탈";
    }

    private void OnEnterClicked()
    {
        SoundManager.Instance?.PlayButtonClick();
        if (_selectedPortal == null)
        {
            GameCore.AlertManager.Enqueue(AlertType.PortalNotSelected);
            return;
        }

        CloseUI();

        GameCore.SceneTransitionManager.MoveToScene(
            SceneNames.GetSceneName(_selectedPortal.TargetScene),
            _selectedPortal.TargetSpawnId
        );
    }

    private void OnPortalSelectClicked()
    {
        SoundManager.Instance?.PlayButtonClick();
        OpenMapSelectPanel();
    }

    private void OnReturnClicked()
    {
        SoundManager.Instance?.PlayButtonClick();
        CloseUI();

        GameCore.SceneTransitionManager.MoveToScene(
            SceneNames.GetSceneName(SceneType.Main),
            "main_1"
        );
        GameCore.RescueManager.ClearList();
    }

    private void ResetSelection()
    {
        _selectedPortal = null;
        _selectPortalTxt.text = "현재 선택된 포탈 : 없음";

        foreach (PortalDestinationButton btn in _forestPortalBtns)
        {
            btn.SetSelected(false);
        }
    }
}