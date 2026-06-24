using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPCUIController : MonoBehaviour
{
    [Header("NPC")]
    [SerializeField] private NPCInteractable _npc;

    [SerializeField] private GameObject _npcUIRoot;
    [SerializeField] private GameObject _npcUIBase;
    [SerializeField] private Button _closeBtn;

    [Header("Base Buttons")]
    [SerializeField] private Button _enhanceBtn;
    [SerializeField] private Button _shopBtn;
    [SerializeField] private Button _questBtn;

    [Header("Content Panels")]
    [SerializeField] private GameObject _enhancePanel;
    [SerializeField] private GameObject _shopPanel;
    [SerializeField] private GameObject _questPanel;

    [Header("Quest Dialogue")]
    [SerializeField] private DialoguePanel _questDialoguePanel;

    private GameObject _currentContentPanel;

    private EnhanceUIController _enhanceUIController;
    private ShopUIController _shopUIController;

    private List<ShopItemEntry> _currentShopItems = new();

    private void Awake()
    {
        _enhanceBtn.onClick.AddListener(() => OpenContentPanel(_enhancePanel));
        _shopBtn.onClick.AddListener(() => OpenContentPanel(_shopPanel));
        _questBtn.onClick.AddListener(OnQuestButtonClick);
        _enhanceUIController = _enhancePanel.GetComponent<EnhanceUIController>();
        _shopUIController = _shopPanel.GetComponent<ShopUIController>();
        _npcUIRoot.SetActive(false);
    }

    private void OnEnable()
    {
        _closeBtn.onClick.AddListener(() => { SoundManager.Instance?.PlayButtonClick(); HandleCloseButton(); });
        GameCore.PlayerInputs.CloseEvent += HandleCloseButton;
    }

    private void OnDisable()
    {
        GameCore.PlayerInputs.CloseEvent -= HandleCloseButton;
        _closeBtn.onClick.RemoveListener(HandleCloseButton);
    }

    public void OpenPanel(List<NPCContentType> availableContents, List<ShopItemEntry> shopItems = null)
    {

        if (shopItems != null)
        {
            _currentShopItems = shopItems;
        }
        else
        {
            _currentShopItems = new List<ShopItemEntry>();
        }

        _npcUIRoot.SetActive(true);
        _npcUIBase.SetActive(true);

        _currentContentPanel = null;
        _enhancePanel.SetActive(false);
        _shopPanel.SetActive(false);
        _questPanel.SetActive(false);

        _enhanceBtn.gameObject.SetActive(availableContents.Contains(NPCContentType.Enhance));
        _shopBtn.gameObject.SetActive(availableContents.Contains(NPCContentType.Shop));
        _questBtn.gameObject.SetActive(availableContents.Contains(NPCContentType.Quest));
    }



    private void HandleCloseButton()
    {
        if (_questDialoguePanel != null && _questDialoguePanel.gameObject.activeSelf)
            return;

        if (_currentContentPanel == _enhancePanel && _enhanceUIController.IsResultOverlayOpen)
        {
            _enhanceUIController.CloseResultOverlay();
            return;
        }

        if (_currentContentPanel == _shopPanel && _shopUIController.IsDetailOpen)
        {
            _shopUIController.CloseDetail();
            return;
        }

        if (_currentContentPanel != null)
        {
            BackToBasePanel();
        }
        else
        {
            ClosePanel();
        }
    }

    private void OnQuestButtonClick()
    {
        SoundManager.Instance?.PlayButtonClick();
        string[] dialogues = QuestManager.Instance != null && QuestManager.Instance.ActiveQuest != null
            ? QuestManager.Instance.ActiveQuest.inProgressDialogues
            : null;

        if (_questDialoguePanel != null && dialogues != null && dialogues.Length > 0)
        {
            _npcUIBase.SetActive(false);
            _questDialoguePanel.Show(dialogues, () =>
            {
                _npcUIBase.SetActive(true);
                GameCore.PlayerInputs.SetCursorLock(false);
            });
        }
    }

    private void OpenContentPanel(GameObject panel)
    {
        SoundManager.Instance?.PlayButtonClick();
        _npcUIBase.SetActive(false);

        panel.SetActive(true);
        _currentContentPanel = panel;

        if (panel == _enhancePanel || panel == _shopPanel)
        {
            GameCore.GameController.MoveNPCCamForContent();
        }

        if (panel == _enhancePanel)
        {
            _enhanceUIController.OpenPanel();
        }

        if (panel == _shopPanel)
        {
            _shopUIController.OpenPanel(_currentShopItems);
        }
    }

    private void BackToBasePanel()
    {
        if (_currentContentPanel == _enhancePanel || _currentContentPanel == _shopPanel)
        {
            GameCore.GameController.ReturnNPCCamToBase();
        }

        _currentContentPanel.SetActive(false);
        _currentContentPanel = null;

        _npcUIBase.SetActive(true);
    }

    public void EndInteraction()
    {
        _npc.SetOutline(true);
        GameCore.PlayerInputs.SetPlayerInput();
        GameCore.GameController.ActiveCoreUI();
        GameCore.GameController.ReleaseFocus();
        GameCore.PlayerInputs.SetCursorLock(true);
        GameCore.RescueQuickSlotUI.ForceRefresh();
    }

    public void ClosePanel()
    {
        if (!_npcUIRoot.activeSelf)
        {
            return;
        }
        _npcUIRoot.SetActive(false);
        _npcUIBase.SetActive(true);

        _currentContentPanel = null;
        _enhancePanel.SetActive(false);
        _shopPanel.SetActive(false);
        _questPanel.SetActive(false);
        _npc.SetOutline(true);

        GameCore.PlayerInputs.SetPlayerInput();
        GameCore.GameController.ActiveCoreUI();
        GameCore.GameController.ReleaseFocus();
        GameCore.PlayerInputs.SetCursorLock(true);
        GameCore.RescueQuickSlotUI.ForceRefresh();
        // Time.timeScale = 1f;
    }
}