using System;
using UnityEngine;

[Serializable]
public class AlertFormatEntry
{
    public AlertType type;
    public Sprite icon;
    [TextArea]
    public string titleFormat;
    [TextArea]
    public string contentFormat;
}