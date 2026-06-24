using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RescueBoardSlotUI : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _text;

    public void Bind(RescuedMonsterEntry entry)
    {
        if (entry == null || entry.MonsterData == null)
        {
            Clear();
            return;
        }

        MonsterData monster = entry.MonsterData;

        _icon.gameObject.SetActive(true);
        _icon.sprite = monster.icon;

        _text.text = $"{monster.monsterName} x{entry.Amount}";
    }

    public void Clear()
    {
        _icon.sprite = null;
        _icon.gameObject.SetActive(false);
        _text.text = string.Empty;
    }
}