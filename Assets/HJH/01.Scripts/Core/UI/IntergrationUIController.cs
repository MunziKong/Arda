using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class IntergrationUIController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject _intergrationUIPanel;
    [SerializeField] private GameObject _interactUIPanel;
    [SerializeField] private GameObject _codexUIPanel;
    [SerializeField] private PortalMapUI _portalMapUI;
    [SerializeField] private InventoryUI _inventoryUI;
    [SerializeField] private PlayerStatusUIController _playerStatusUIController;
    [SerializeField] private Button _closeBtn;


    [Header("Input")]
    [SerializeField] private PlayerInputs _playerInputs;


    private bool _isOpened;

    private void Awake()
    {
        CloseUI();
    }

    void OnEnable()
    {
        _closeBtn.onClick.AddListener(() => { SoundManager.Instance?.PlayButtonClick(); HandleClose(); });
        _playerInputs.ToggleUIEvent += ToggleUI;
        _playerInputs.CloseEvent += HandleClose;
    }

    void OnDisable()
    {
        _closeBtn.onClick.RemoveListener(HandleClose);
        _playerInputs.ToggleUIEvent -= ToggleUI;
        _playerInputs.CloseEvent -= HandleClose;
    }
    public void ToggleUI()
    {
        if (_codexUIPanel.activeSelf)
        {
            return;
        }

        if (_isOpened)
        {
            HandleClose();
        }
        else
        {
            OpenUI();
        }
    }

    public void OpenUI()
    {
        _isOpened = true;

        _intergrationUIPanel.SetActive(true);
        _interactUIPanel.SetActive(false);
        _portalMapUI.ForceCloseUI();
        _playerStatusUIController.RefreshAll();
        Time.timeScale = 0f;

        GameCore.PlayerInputs.SetCursorLock(false);
        GameCore.PlayerInputs.SetUIInput();
    }

    public void CloseUI()
    {

        if (!_isOpened)
        {
            return;
        }

        _isOpened = false;

        _intergrationUIPanel.SetActive(false);
        _interactUIPanel.SetActive(true);
        _inventoryUI.CloseDetailPanel();
        Time.timeScale = 1f;

        GameCore.PlayerInputs.SetCursorLock(true);
        GameCore.PlayerInputs.SetPlayerInput();
    }

    private void HandleClose()
    {

        if (_inventoryUI.IsDetailOpened)
        {
            _inventoryUI.CloseDetailPanel();
            return;
        }

        CloseUI();
    }
}