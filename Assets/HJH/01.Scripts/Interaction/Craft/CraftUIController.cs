using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftUIController : MonoBehaviour
{
    [SerializeField] private GameObject _rootPanel;

    [Header("Input")]
    [SerializeField] private Button _closeBtn;

    [Header("Recipe Section")]
    [SerializeField] private Transform _recipeContent;
    [SerializeField] private RecipeSlotUI _recipeSlotPrefab;
    [SerializeField] private TMP_Text _totalRecipeText;

    [Header("Detail Section")]
    [SerializeField] private Image _detailIcon;
    [SerializeField] private TMP_Text _detailNameText;
    [SerializeField] private TMP_Text _detailDescriptionText;
    [SerializeField] private TMP_Text _detailBaseRatioText;

    [Header("Required Materials")]
    [SerializeField] private Transform _materialContent;
    [SerializeField] private CraftMaterialSlotUI _materialSlotPrefab;

    [Header("Quantity")]
    [SerializeField] private Button _minusBtn;
    [SerializeField] private Button _plusBtn;
    [SerializeField] private Button _maxBtn;
    [SerializeField] private TMP_Text _quantityText;
    [SerializeField] private TMP_Text _maxStackText;

    [Header("Monster Grade")]
    [SerializeField] private Transform _monsterContent;
    [SerializeField] private CraftMonsterSlotUI _monsterSlotPrefab;

    [Header("Calculate")]
    [SerializeField] private TMP_Text _baseRatioText;
    [SerializeField] private TMP_Text _bonusRatioText;
    [SerializeField] private TMP_Text _totalRatioText;

    [Header("Craft Button")]
    [SerializeField] private Button _craftBtn;

    private RecipeData _selectedRecipe;
    private MonsterGrade? _selectedGrade;
    private int _currentQuantity = 1;
    private int _maxQuantity = 1;

    private List<RecipeSlotUI> _recipeSlots = new();
    private List<CraftMonsterSlotUI> _monsterSlots = new();

    private void Awake()
    {
        _minusBtn.onClick.AddListener(OnMinusClicked);
        _plusBtn.onClick.AddListener(OnPlusClicked);
        _maxBtn.onClick.AddListener(OnMaxClicked);
        _craftBtn.onClick.AddListener(OnCraftClicked);
    }

    private void OnEnable()
    {
        _closeBtn.onClick.AddListener(() => { SoundManager.Instance?.PlayButtonClick(); ClosePanel(); });
        GameCore.PlayerInputs.CloseEvent += ClosePanel;
    }

    private void OnDisable()
    {
        _closeBtn.onClick.RemoveListener(ClosePanel);
        GameCore.PlayerInputs.CloseEvent -= ClosePanel;
    }

    public void OpenPanel()
    {
        _rootPanel.SetActive(true);
        _selectedRecipe = null;
        _selectedGrade = null;
        _currentQuantity = 1;

        RefreshRecipeList();
        ClearDetail();

        GameCore.GameController.InactiveCoreUI();
        GameCore.PlayerInputs.SetCursorLock(false);
        GameCore.PlayerInputs.SetUIInput();
        Time.timeScale = 0f;
    }

    public void ClosePanel()
    {
        if (!_rootPanel.activeSelf)
        {
            return;
        }

        _rootPanel.SetActive(false);

        GameCore.PlayerInputs.SetPlayerInput();
        GameCore.GameController.ActiveCoreUI();
        GameCore.PlayerInputs.SetCursorLock(true);
        Time.timeScale = 1f;
    }

    private void RefreshRecipeList()
    {
        for (int i = _recipeContent.childCount - 1; i >= 0; i--)
        {
            Destroy(_recipeContent.GetChild(i).gameObject);
        }

        _recipeSlots.Clear();

        IReadOnlyList<RecipeData> recipes = GameCore.PlayerOwnedData.UnlockedRecipes;

        foreach (RecipeData recipe in recipes)
        {
            RecipeSlotUI slot = Instantiate(_recipeSlotPrefab, _recipeContent);
            slot.Setup(recipe, OnRecipeSelected);
            slot.SetSelected(_selectedRecipe == recipe);
            _recipeSlots.Add(slot);
        }

        _totalRecipeText.text = $"총 보유 레시피 : {recipes.Count}";
    }

    private void OnRecipeSelected(RecipeData recipe)
    {
        if (_selectedRecipe == recipe)
        {
            _selectedRecipe = null;
            _selectedGrade = null;
            _currentQuantity = 1;

            foreach (RecipeSlotUI slot in _recipeSlots)
            {
                slot.SetSelected(false);
            }

            ClearDetail();
            return;
        }

        _selectedRecipe = recipe;
        _selectedGrade = null;
        _currentQuantity = 1;
        _maxQuantity = recipe.resultItem != null ? recipe.resultItem.MaxStack : 1;

        foreach (RecipeSlotUI slot in _recipeSlots)
        {
            slot.SetSelected(slot.Recipe == recipe);
        }

        RefreshDetail();
        RefreshMaterials();
        RefreshQuantity();
        RefreshMonsterList();
        RefreshCalculate();
        RefreshCraftButton();
    }

    private void RefreshDetail()
    {
        if (_selectedRecipe == null || _selectedRecipe.resultItem == null)
        {
            ClearDetail();
            return;
        }

        _detailIcon.gameObject.SetActive(true);
        _detailIcon.sprite = _selectedRecipe.resultItem.Icon;
        _detailNameText.text = _selectedRecipe.resultItem.ItemName;
        _detailDescriptionText.text = _selectedRecipe.resultItem.Description;
        _detailBaseRatioText.text = $"{Mathf.RoundToInt(_selectedRecipe.baseSuccessRate * 100f)}%";
    }

    private void RefreshMaterials()
    {
        for (int i = _materialContent.childCount - 1; i >= 0; i--)
        {
            Destroy(_materialContent.GetChild(i).gameObject);
        }

        if (_selectedRecipe == null)
        {
            return;
        }

        foreach (ItemRequirement req in _selectedRecipe.materials)
        {
            CraftMaterialSlotUI slot = Instantiate(_materialSlotPrefab, _materialContent);
            slot.Setup(req, _currentQuantity);
        }
    }

    private void RefreshQuantity()
    {
        _quantityText.text = _currentQuantity.ToString();
        _maxStackText.text = $"(보유 최대 : {_maxQuantity}개)";

        _minusBtn.interactable = _currentQuantity > 1;
        _plusBtn.interactable = _currentQuantity < _maxQuantity;
        _maxBtn.interactable = _currentQuantity < _maxQuantity;
    }

    private void RefreshMonsterList()
    {
        for (int i = _monsterContent.childCount - 1; i >= 0; i--)
        {
            Destroy(_monsterContent.GetChild(i).gameObject);
        }

        _monsterSlots.Clear();

        Dictionary<MonsterGrade, int> availableMonsters = GameCore.CraftManager.GetAvailableMonstersByGrade();

        foreach (GradeBonus gradeBonus in GameCore.CraftManager.GradeBonuses)
        {
            MonsterGrade grade = gradeBonus.grade;
            int count = availableMonsters.ContainsKey(grade) ? availableMonsters[grade] : 0;

            float successBonus = gradeBonus.successRateBonus;
            float criticalRate = gradeBonus.criticalRate;

            CraftMonsterSlotUI slot = Instantiate(_monsterSlotPrefab, _monsterContent);
            slot.Setup(grade, count, successBonus, criticalRate, OnMonsterGradeSelected);

            _monsterSlots.Add(slot);
        }
    }

    private void OnMonsterGradeSelected(MonsterGrade grade)
    {
        Dictionary<MonsterGrade, int> availableMonsters = GameCore.CraftManager.GetAvailableMonstersByGrade();
        int count = availableMonsters.ContainsKey(grade) ? availableMonsters[grade] : 0;

        if (count <= 0)
        {
            return;
        }

        if (_selectedGrade.HasValue && _selectedGrade.Value == grade)
        {
            _selectedGrade = null;
        }
        else
        {
            _selectedGrade = grade;
        }

        foreach (CraftMonsterSlotUI slot in _monsterSlots)
        {
            slot.SetSelected(_selectedGrade.HasValue && slot.Grade == _selectedGrade.Value);
        }

        RefreshCalculate();
        RefreshCraftButton();
    }

    private void RefreshCalculate()
    {
        float baseRate = _selectedRecipe != null ? _selectedRecipe.baseSuccessRate : 0f;
        float bonusRate = 0f;

        if (_selectedGrade.HasValue)
        {
            bonusRate = GameCore.CraftManager.GetSuccessBonus(_selectedGrade.Value);
        }

        float totalRate = Mathf.Clamp01(baseRate + bonusRate);

        _baseRatioText.text = $"{Mathf.RoundToInt(baseRate * 100f)}%";
        _bonusRatioText.text = $"{Mathf.RoundToInt(bonusRate * 100f)}%";
        _totalRatioText.text = $"{Mathf.RoundToInt(totalRate * 100f)}%";
    }

    private void RefreshCraftButton()
    {
        if (_selectedRecipe == null)
        {
            _craftBtn.interactable = false;
            return;
        }

        _craftBtn.interactable = GameCore.CraftManager.HasEnoughMaterials(_selectedRecipe, _currentQuantity);
    }

    private void OnMinusClicked()
    {
        SoundManager.Instance?.PlayButtonClick();
        if (_currentQuantity <= 1)
        {
            return;
        }

        _currentQuantity--;

        RefreshMaterials();
        RefreshQuantity();
        RefreshCraftButton();
    }

    private void OnPlusClicked()
    {
        SoundManager.Instance?.PlayButtonClick();
        if (_currentQuantity >= _maxQuantity)
        {
            return;
        }

        _currentQuantity++;

        RefreshMaterials();
        RefreshQuantity();
        RefreshCraftButton();
    }

    private void OnMaxClicked()
    {
        SoundManager.Instance?.PlayButtonClick();
        _currentQuantity = GetMaxAffordableQuantity();

        RefreshMaterials();
        RefreshQuantity();
        RefreshCraftButton();
    }

    private void OnCraftClicked()
    {
        SoundManager.Instance?.PlayButtonClick();
        if (_selectedRecipe == null)
        {
            return;
        }

        List<CraftResult> results = GameCore.CraftManager.TryCraft(_selectedRecipe, _currentQuantity, _selectedGrade);
        CraftSummary summary = GameCore.CraftManager.Summarize(results);

        Debug.Log($"[Craft] {_selectedRecipe.resultItem.ItemName} - 성공: {summary.TotalSuccess}회 / 대성공: {summary.TotalCritical}회 / 실패: {summary.TotalFail}회 / 총 획득: {summary.TotalAcquired}개");

        _currentQuantity = 1;
        _selectedGrade = null;

        RefreshRecipeList();
        RefreshMaterials();
        RefreshQuantity();
        RefreshMonsterList();
        RefreshCalculate();
        RefreshCraftButton();

        GameCore.AlertManager.Enqueue(AlertType.CraftResult,
            _selectedRecipe.resultItem.ItemName,
            summary.TotalSuccess.ToString(),
            summary.TotalCritical.ToString(),
            summary.TotalFail.ToString()
        );
    }

    private void ClearDetail()
    {
        _detailIcon.gameObject.SetActive(false);
        _detailIcon.sprite = null;

        _detailNameText.text = string.Empty;
        _detailDescriptionText.text = string.Empty;
        _detailBaseRatioText.text = string.Empty;

        for (int i = _materialContent.childCount - 1; i >= 0; i--)
        {
            Destroy(_materialContent.GetChild(i).gameObject);
        }

        _quantityText.text = "1";
        _maxStackText.text = string.Empty;
        _baseRatioText.text = "0%";
        _bonusRatioText.text = "0%";
        _totalRatioText.text = "0%";

        _craftBtn.interactable = false;
    }

    private int GetMaxAffordableQuantity()
    {
        if (_selectedRecipe == null || _selectedRecipe.materials == null)
            return _maxQuantity;

        int affordable = _maxQuantity;

        foreach (ItemRequirement req in _selectedRecipe.materials)
        {
            if (req.item == null || req.amount <= 0) continue;

            int canCraft;

            if (req.item is CurrencyItemData)
            {
                canCraft = GameCore.PlayerInventory.Gold / req.amount;
            }
            else
            {
                int owned = GameCore.PlayerInventory.GetQuantity(req.item);
                canCraft = owned / req.amount;
            }

            affordable = Mathf.Min(affordable, canCraft);
        }

        return Mathf.Max(1, affordable);
    }
}