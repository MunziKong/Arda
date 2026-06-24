using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Arda/Enemy/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("Info")]
    public string enemyName;

    [Header("Stats")]
    public float maxHp = 100f;
    public float moveSpeed = 3.5f;
    public float attackPower = 10f;
    public float detectionRange = 10f;
    public float patrolRadius = 15f;

    [Header("Drop Table")]
    public List<DropEntry> dropTable;
}
