using UnityEngine;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(Button))]
public class PortalDestinationButton : MonoBehaviour
{
    [SerializeField] private SceneType _targetScene;
    [SerializeField] private string _targetSpawnId;
    [SerializeField] private int _portalNumber;

    [Header("Colors")]
    [SerializeField] private Image _buttonImage;
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _selectedColor = Color.green;

    [SerializeField] private Button _button;

    public SceneType TargetScene => _targetScene;
    public string TargetSpawnId => _targetSpawnId;
    public int PortalNumber => _portalNumber;

    public void Initialize(Action<PortalDestinationButton> onSelected)
    {
        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() =>
        {
            SoundManager.Instance?.PlayButtonClick();
            onSelected?.Invoke(this);
        });
    }

    public void SetSelected(bool isSelected)
    {
        if (_buttonImage != null)
        {
            _buttonImage.color = isSelected ? _selectedColor : _normalColor;
        }
    }
}