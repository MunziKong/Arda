using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecipeSlotUI : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _ratioText;
    [SerializeField] private Button _button;
    [SerializeField] private Image _backgroundImage;

    [Header("Colors")]
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _selectedColor = Color.green;

    private RecipeData _recipe;

    public RecipeData Recipe => _recipe;

    public void Setup(RecipeData recipe, Action<RecipeData> onClick)
    {
        _recipe = recipe;

        _icon.sprite = recipe.resultItem != null ? recipe.resultItem.Icon : null;
        _nameText.text = recipe.resultItem != null ? recipe.resultItem.ItemName : string.Empty;
        _ratioText.text = $"{Mathf.RoundToInt(recipe.baseSuccessRate * 100f)}%";

        SetSelected(false);

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() => { SoundManager.Instance?.PlayButtonClick(); onClick?.Invoke(_recipe); });
    }

    public void SetSelected(bool isSelected)
    {
        if (_backgroundImage != null)
        {
            _backgroundImage.color = isSelected ? _selectedColor : _normalColor;
        }
    }
}