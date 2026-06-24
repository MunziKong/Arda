using System;
using UnityEngine;
using UnityEngine.UI;

public class MapSlotUI : MonoBehaviour
{
    [SerializeField] private Image _slotImage;
    [SerializeField] private bool _isAvailable;

    [Header("Colors")]
    [SerializeField] private Color _selectedColor = Color.yellow;
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _unavailableColor = Color.gray;

    [SerializeField] private Button _button;

    private void Start()
    {

        if (!_isAvailable)
        {
            SetUnavailable();
        }
    }

    public void Initialize(Action onSelected)
    {
        _button.onClick.RemoveAllListeners();

        if (!_isAvailable)
        {
            _button.onClick.AddListener(() =>
            {
                SoundManager.Instance?.PlayButtonClick();
                GameCore.AlertManager.Enqueue(AlertType.MapNotAvailable);
            });
            return;
        }

        _button.onClick.AddListener(() => { SoundManager.Instance?.PlayButtonClick(); onSelected?.Invoke(); });
    }

    public void SetSelected(bool isSelected)
    {
        if (_slotImage == null)
        {
            return;
        }

        if (!_isAvailable)
        {
            _slotImage.color = _unavailableColor;
            return;
        }

        _slotImage.color = isSelected ? _selectedColor : _normalColor;
    }

    private void SetUnavailable()
    {
        if (_slotImage != null)
        {
            _slotImage.color = _unavailableColor;
        }
    }
}