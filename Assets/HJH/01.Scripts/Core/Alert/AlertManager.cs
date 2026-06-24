using System.Collections.Generic;
using UnityEngine;

public class AlertManager : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private AlertData _alertData;

    [Header("Ref")]
    [SerializeField] private AlertUIController _alertUIController;

    private Dictionary<AlertType, AlertFormatEntry> _alertDict = new();

    private void Awake()
    {
        foreach (AlertFormatEntry entry in _alertData.entries)
        {
            _alertDict[entry.type] = entry;
        }
    }

    public void Enqueue(AlertType type, params string[] args)
    {
        if (!_alertDict.TryGetValue(type, out AlertFormatEntry entry))
        {
            Debug.Log($"[AlertManager] AlertType {type}이 AlertData에 등록되지 않았습니다.");
            return;
        }

        string title = entry.titleFormat;

        string content = args.Length > 0 ? string.Format(entry.contentFormat, args) : entry.contentFormat;

        _alertUIController.Enqueue(title, content, entry.icon);
    }
}