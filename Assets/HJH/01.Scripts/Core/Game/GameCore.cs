using UnityEngine;

public class GameCore : MonoBehaviour
{
    private static GameCore _instance;

    [Header("Managers")]
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private GameController _gameController;
    [SerializeField] private SceneTransitionManager _sceneTransitionManager;
    [SerializeField] private RescueManager _rescueManager;
    [SerializeField] private LoadoutManager _loadoutManager;
    [SerializeField] private EnhanceManager _enhanceManager;
    [SerializeField] private ShopManager _shopManager;
    [SerializeField] private CraftManager _craftManager;
    [SerializeField] private AlertManager _alertManager;
    [SerializeField] private SoundManager _soundManager;

    [Header("Player")]
    [SerializeField] private PlayerOwnedData _playerOwnedData;
    [SerializeField] private PlayerInputs _playerInputs;
    [SerializeField] private PlayerStats _playerStats;
    [SerializeField] private PlayerInventory _playerInventory;
    [SerializeField] private PlayerEquipment _playerEquipments;
    [SerializeField] private PlayerAttack _playerAttack;
    [SerializeField] private PlayerSkillController _playerSkillController;
    [SerializeField] private PlayerAnimController _playerAnimController;
    [SerializeField] private PlayerDeathHandler _playerDeathHandler;

    [Header("Save")]
    [SerializeField] private GameDatabase _gameDatabase;

    [Header("UI")]
    [SerializeField] private PortalUIController _portalUIController;
    [SerializeField] private SkillSetUIController _skillSetUIController;
    [SerializeField] private MessageUIController _messageUIController;
    [SerializeField] private RescueQuickSlotUI _rescueQuickSlotUI;
    [SerializeField] private MainQuestUI _mainQuestUI;

    [Header("HUD Panels")]
    [SerializeField] private GameObject _quickSlotPanel;
    [SerializeField] private GameObject _skillSetPanel;
    [SerializeField] private GameObject _codexIconPanel;

    [Header("FadePanel")]
    [SerializeField] private FadePanel _fadePanel;

    public static FadePanel FadePanel => _instance._fadePanel;
    public static GameManager GameManager => _instance._gameManager;
    public static GameController GameController => _instance._gameController;
    public static SoundManager SoundManager => _instance._soundManager;
    public static SceneTransitionManager SceneTransitionManager => _instance._sceneTransitionManager;
    public static RescueManager RescueManager => _instance._rescueManager;
    public static LoadoutManager LoadoutManager => _instance._loadoutManager;
    public static EnhanceManager EnhanceManager => _instance._enhanceManager;
    public static ShopManager ShopManager => _instance._shopManager;
    public static CraftManager CraftManager => _instance._craftManager;
    public static AlertManager AlertManager => _instance._alertManager;

    public static PlayerOwnedData PlayerOwnedData => _instance._playerOwnedData;
    public static PlayerInputs PlayerInputs => _instance._playerInputs;
    public static PlayerStats PlayerStats => _instance._playerStats;
    public static PlayerInventory PlayerInventory => _instance._playerInventory;
    public static PlayerEquipment PlayerEquipment => _instance._playerEquipments;
    public static PlayerAttack PlayerAttack => _instance._playerAttack;
    public static PlayerSkillController PlayerSkillController => _instance._playerSkillController;
    public static PlayerAnimController PlayerAnimController => _instance._playerAnimController;
    public static PlayerDeathHandler PlayerDeathHandler => _instance._playerDeathHandler;

    public static GameDatabase GameDatabase => _instance._gameDatabase;
    public static SkillSetUIController SkillSetUIController => _instance._skillSetUIController;
    public static MessageUIController MessageUIController => _instance._messageUIController;
    public static PortalUIController PortalUIController => _instance._portalUIController;
    public static RescueQuickSlotUI RescueQuickSlotUI => _instance._rescueQuickSlotUI;


    public static bool IsQuickSlotPanelActive => _instance._quickSlotPanel != null && _instance._quickSlotPanel.activeSelf;
    public static bool IsSkillSetPanelActive => _instance._skillSetPanel != null && _instance._skillSetPanel.activeSelf;
    public static bool IsCodexIconPanelActive => _instance._codexIconPanel != null && _instance._codexIconPanel.activeSelf;

    public static void ActivateQuickSlotPanel()
    {
        if (_instance._quickSlotPanel != null)
        {
            _instance._quickSlotPanel.SetActive(true);
            if (SaveManager.Instance != null) SaveManager.Instance.Save();
        }
    }

    public static void ActivateSkillSetPanel()
    {
        if (_instance._skillSetPanel != null)
        {
            _instance._skillSetPanel.SetActive(true);
            if (SaveManager.Instance != null) SaveManager.Instance.Save();
        }
    }

    public static void ActivateCodexIconPanel()
    {
        if (_instance._codexIconPanel != null)
        {
            _instance._codexIconPanel.SetActive(true);
            if (SaveManager.Instance != null) SaveManager.Instance.Save();
        }
    }

    public static void ResetHUDPanels()
    {
        if (_instance._quickSlotPanel != null) _instance._quickSlotPanel.SetActive(false);
        if (_instance._skillSetPanel != null) _instance._skillSetPanel.SetActive(false);
        if (_instance._codexIconPanel != null) _instance._codexIconPanel.SetActive(false);
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
}