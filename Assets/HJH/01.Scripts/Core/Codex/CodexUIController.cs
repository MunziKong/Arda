using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CodexUIController : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject _rootPanel;

    [Header("Ref")]
    [SerializeField] private PortalMapUI _portalMapUI;
    [SerializeField] private GameObject _interactUIPanel;
    [SerializeField] private GameObject _intergrationUI;

    [Header("Input")]
    [SerializeField] private PlayerInputs _playerInputs;
    [SerializeField] private Button _closeBtn;

    [Header("Current Overview")]
    [SerializeField] private TMP_Text _commonQuantityTxt;
    [SerializeField] private TMP_Text _uncommonQuantityTxt;
    [SerializeField] private TMP_Text _rareQuantityTxt;
    [SerializeField] private TMP_Text _epicQuantityTxt;
    [SerializeField] private TMP_Text _legendaryQuantityTxt;
    [SerializeField] private TMP_Text _uniqueQuantityTxt;

    [Header("Codex List")]
    [SerializeField] private Transform _codexContent;
    [SerializeField] private CodexSlotUI _codexSlotPrefab;

    [Header("Detail Section")]
    [SerializeField] private GameObject _detailSection;
    [SerializeField] private TMP_Text _detailNameText;
    [SerializeField] private Image _detailIcon;
    [SerializeField] private TMP_Text _gradeTxt;
    [SerializeField] private TMP_Text _mapTxt;
    [SerializeField] private TMP_Text _timeTxt;
    [SerializeField] private TMP_Text _typeTxt;
    [SerializeField] private TMP_Text _descriptionTxt;
    [SerializeField] private Transform _dropItemContent;
    [SerializeField] private DropItemSlotUI _dropItemSlotPrefab;

    private MonsterData _selectedMonster;
    private List<CodexSlotUI> _codexSlots = new();

    private void Awake()
    {
        _closeBtn.onClick.AddListener(() => { SoundManager.Instance?.PlayButtonClick(); ClosePanel(); });
        _rootPanel.SetActive(false);
    }

    private void OnEnable()
    {
        _playerInputs.CodexEvent += TogglePanel;
        _playerInputs.CloseEvent += ClosePanel;
    }

    private void OnDisable()
    {
        _playerInputs.CodexEvent -= TogglePanel;
        _playerInputs.CloseEvent -= ClosePanel;
    }

    private void TogglePanel()
    {
        if (!GameCore.PlayerOwnedData.IsCodexUnlocked)
        {
            return;
        }

        if (_intergrationUI.activeSelf)
        {
            return;
        }

        if (_rootPanel.activeSelf)
        {
            ClosePanel();
        }
        else
        {
            OpenPanel();
        }
    }

    public void OpenPanel()
    {
        _rootPanel.SetActive(true);

        _selectedMonster = null;
        _detailSection.SetActive(false);
        _interactUIPanel.SetActive(false);

        RefreshOverview();
        RefreshCodexList();

        Time.timeScale = 0f;
        GameCore.PlayerInputs.SetCursorLock(false);
        GameCore.PlayerInputs.SetUIInput();
    }

    public void ClosePanel()
    {
        if (!_rootPanel.activeSelf)
        {
            return;
        }

        _rootPanel.SetActive(false);
        _interactUIPanel.SetActive(true);
        _portalMapUI.ForceCloseUI();
        Time.timeScale = 1f;
        GameCore.PlayerInputs.SetCursorLock(true);
        GameCore.PlayerInputs.SetPlayerInput();
    }

    private void RefreshOverview()
    {
        Dictionary<MonsterGrade, int> gradeCounts = new();

        foreach (MonsterGrade grade in System.Enum.GetValues(typeof(MonsterGrade)))
        {
            gradeCounts[grade] = 0;
        }

        foreach (MonsterRescueRecord record in GameCore.PlayerOwnedData.MonsterRescueRecords)
        {
            if (record.MonsterData == null)
            {
                continue;
            }

            MonsterGrade grade = record.MonsterData.grade;

            if (gradeCounts.ContainsKey(grade))
            {
                gradeCounts[grade] += record.Count;
            }
        }

        _commonQuantityTxt.text = gradeCounts[MonsterGrade.Common].ToString();
        _uncommonQuantityTxt.text = gradeCounts[MonsterGrade.Uncommon].ToString();
        _rareQuantityTxt.text = gradeCounts[MonsterGrade.Rare].ToString();
        _epicQuantityTxt.text = gradeCounts[MonsterGrade.Epic].ToString();
        _legendaryQuantityTxt.text = gradeCounts[MonsterGrade.Legendary].ToString();
        _uniqueQuantityTxt.text = gradeCounts[MonsterGrade.Unique].ToString();
    }

    private void RefreshCodexList()
    {
        for (int i = _codexContent.childCount - 1; i >= 0; i--)
        {
            Destroy(_codexContent.GetChild(i).gameObject);
        }

        _codexSlots.Clear();
        List<string> names = new List<string>();
        foreach (MonsterData monster in GameCore.PlayerOwnedData.DiscoveredMonsters)
        {
            if (names.Contains(monster.monsterName))
            {
                continue;
            }
            else
            {
                names.Add(monster.monsterName);
                Debug.Log("도감에 나타날 monster Name" + monster.monsterName);
            }
            CodexSlotUI slot = Instantiate(_codexSlotPrefab, _codexContent);
            slot.Setup(monster, OnCodexSlotSelected);
            _codexSlots.Add(slot);
        }
    }

    private void OnCodexSlotSelected(MonsterData monster)
    {
        _selectedMonster = monster;

        foreach (CodexSlotUI slot in _codexSlots)
        {
            slot.SetSelected(slot.MonsterData == monster);
        }

        RefreshDetail(monster);
    }

    private void RefreshDetail(MonsterData monster)
    {
        _detailSection.SetActive(true);

        _detailNameText.text = monster.monsterName;
        _detailIcon.sprite = monster.icon;
        _gradeTxt.text = monster.grade.ToString();
        _mapTxt.text = "녹색 숲";
        _timeTxt.text = $"{(int)monster.spawnStartHour:00}시 ~ {(int)monster.spawnEndHour:00}시";
        _typeTxt.text = GetTypeText(monster.type);
        _descriptionTxt.text = monster.description;

        RefreshDropItems(monster);
    }

    private void RefreshDropItems(MonsterData monster)
    {
        for (int i = _dropItemContent.childCount - 1; i >= 0; i--)
        {
            Destroy(_dropItemContent.GetChild(i).gameObject);
        }

        if (monster.dropTable == null || monster.dropTable.Count == 0)
        {
            return;
        }

        foreach (DropEntry dropEntry in monster.dropTable)
        {
            DropItemSlotUI slot = Instantiate(_dropItemSlotPrefab, _dropItemContent);
            slot.Setup(dropEntry);
        }
    }

    private string GetTypeText(MonsterType type)
    {
        return type switch
        {
            MonsterType.Friendly => "우호",
            MonsterType.Ignore => "중립",
            MonsterType.Flee => "경계",
            _ => string.Empty
        };
    }
}