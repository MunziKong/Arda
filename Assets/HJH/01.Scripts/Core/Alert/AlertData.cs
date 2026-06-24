using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AlertData", menuName = "Arda/Alert/AlertData")]
public class AlertData : ScriptableObject
{
    public List<AlertFormatEntry> entries = new();
}