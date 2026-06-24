using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimeUIController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text _timeText;
    [SerializeField] private Image _timeIcon;

    [Header("Icons")]
    [SerializeField] private Sprite _dayIcon;
    [SerializeField] private Sprite _nightIcon;

    private void Update()
    {
        if (GameTimeManager.Instance == null)
        {
            return;
        }

        UpdateTimeText();
        UpdateTimeIcon();
    }

    private void UpdateTimeText()
    {
        float currentHour = GameTimeManager.Instance.CurrentHour;

        int hour = Mathf.FloorToInt(currentHour);
        int minute = Mathf.FloorToInt((currentHour - hour) * 60f);

        _timeText.text = $"{hour:00}:{minute:00}";
    }

    private void UpdateTimeIcon()
    {
        if (_timeIcon == null)
        {
            return;
        }

        _timeIcon.sprite = GameTimeManager.Instance.IsDay() ? _dayIcon : _nightIcon;
    }
}