using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadoutSlotUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private Button _button;

    [Header("Colors")]
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _selectedColor = Color.green;

    public void Init(Sprite icon, string displayName, Action onClick, int enhanceLevel = 0)
    {
        if (_icon != null)
            _icon.sprite = icon;

        if (_nameText != null)
        {
            bool hasName = !string.IsNullOrEmpty(displayName);
            _nameText.gameObject.SetActive(hasName);

            _nameText.text = enhanceLevel > 0
                ? $"{displayName} (+{enhanceLevel})"
                : displayName;
        }

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() => { SoundManager.Instance?.PlayButtonClick(); onClick?.Invoke(); });

        SetSelected(false);
    }

    public void SetSelected(bool isSelected)
    {
        if (_backgroundImage == null)
            return;

        _backgroundImage.color = isSelected
            ? _selectedColor
            : _normalColor;
    }
}