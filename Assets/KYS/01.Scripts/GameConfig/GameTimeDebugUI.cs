using UnityEngine;

public class GameTimeDebugUI : MonoBehaviour
{
    [SerializeField] private int fontSize = 36;
    [SerializeField] private Color textColor = Color.white;

    private GUIStyle _style;

    private void OnGUI()
    {
        if (GameTimeManager.Instance == null)
        {
            return;
        }

        float hour = GameTimeManager.Instance.CurrentHour;
        int h = Mathf.FloorToInt(hour);
        int m = Mathf.FloorToInt((hour - h) * 60f);

        if (_style == null)
        {
            _style = new GUIStyle(GUI.skin.label)
            {
                fontSize = fontSize,
                fontStyle = FontStyle.Bold
            };
            _style.normal.textColor = textColor;
        }

        string time = $"{h:00}:{m:00}";
        // 테두리 효과 (검정 그림자)
        GUI.color = Color.black;
        GUI.Label(new Rect(22, 22, 160, 60), time, _style);
        GUI.color = textColor;
        GUI.Label(new Rect(20, 20, 160, 60), time, _style);
        GUI.color = Color.white;
    }
}
