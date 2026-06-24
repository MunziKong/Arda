using UnityEngine;

[CreateAssetMenu(
   fileName = "CHR_Stats_New",
   menuName = "Arda/CharacterStats")]
public class CharacterStatsDefinition : ScriptableObject
{
    [Header("Base Stats")]
    public string characterId;
    public int maxHp = 100;
    public int meleePower = 10;
    public int magicPower = 10;

    [Header("Combat")]
    public float walkSpeed = 2.0f;
    public float runSpeed = 5.5f;
}

