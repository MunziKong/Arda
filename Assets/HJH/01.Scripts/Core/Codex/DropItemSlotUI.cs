using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DropItemSlotUI : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _itemText;

    public void Setup(DropEntry dropEntry)
    {
        if (dropEntry == null || dropEntry.item == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        _icon.sprite = dropEntry.item.Icon;
        _itemText.text = dropEntry.item.ItemName;
    }
}