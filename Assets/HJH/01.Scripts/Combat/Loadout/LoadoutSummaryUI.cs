using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadoutSummaryUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _typeText;

    public void Init(Sprite icon, string displayName, string typeName)
    {
        if (_icon != null)
            _icon.sprite = icon;

        if (_nameText != null)
            _nameText.text = displayName;

        if (_typeText != null)
            _typeText.text = typeName;
    }
}