using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftMonsterSlotUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image _slotBG;
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _gradeText;
    [SerializeField] private TMP_Text _quantityText;
    [SerializeField] private TMP_Text _bonusRatioText;
    [SerializeField] private TMP_Text _criticalRatioText;
    [SerializeField] private Button _button;

    [Header("Colors")]
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _selectedColor = Color.green;

    private MonsterGrade _grade;

    public MonsterGrade Grade => _grade;

    public void Setup(MonsterGrade grade, int count, float successBonus, float criticalRate, Action<MonsterGrade> onClick)
    {
        _grade = grade;

        _gradeText.text = grade.ToString();
        _quantityText.text = $"보유: {count}";
        _bonusRatioText.text = $"{Mathf.RoundToInt(successBonus * 100f)}%";
        _criticalRatioText.text = $"{Mathf.RoundToInt(criticalRate * 100f)}%";

        SetSelected(false);

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() => { SoundManager.Instance?.PlayButtonClick(); onClick?.Invoke(_grade); });
    }

    public void SetSelected(bool isSelected)
    {
        if (_slotBG != null)
        {
            _slotBG.color = isSelected ? _selectedColor : _normalColor;
        }
    }
}