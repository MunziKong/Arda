using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CodexSlotUI : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private Image _backgroundImage;

    [Header("Colors")]
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _selectedColor = Color.yellow;

    private MonsterData _monsterData;
    private Button _button;

    public MonsterData MonsterData => _monsterData;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    public void Setup(MonsterData monsterData, Action<MonsterData> onClick)
    {
        _monsterData = monsterData;

        _icon.sprite = monsterData.icon;
        _nameText.text = monsterData.monsterName;

        SetSelected(false);

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() => { SoundManager.Instance?.PlayButtonClick(); onClick?.Invoke(_monsterData); });
    }

    public void SetSelected(bool isSelected)
    {
        if (_backgroundImage != null)
        {
            _backgroundImage.color = isSelected ? _selectedColor : _normalColor;
        }
    }
}