using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterData", menuName = "Arda/Monster/MonsterData")]
public class MonsterData : ScriptableObject
{
    [Header("Info")]
    public string monsterName;
    public Sprite icon;

    // 도감에 표시될 몬스터 설명
    [TextArea(3, 6)]
    public string description;

    [Header("Grade & Type")]
    public MonsterGrade grade;
    // MonsterAI 행동 방식 결정
    public MonsterType type;

    [Header("Spawn Time")]
    // true 면 아래 시간 설정 무시
    public bool alwaysSpawn = true;

    // 등장 시작 시간 (0~24시 기준)
    [Range(0, 24)]
    public float spawnStartHour;

    // 등장 종료 시간 (0~24시 기준)
    [Range(0, 24)]
    public float spawnEndHour;

    // 등급이 Unique일 때만 사용 — 구조에 필요한 아이템 목록
    [Header("Unique Rescue Condition")]
    public List<ItemData> requiredItems;

    [Header("Drop Table")]
    // 구조 성공 시 드랍되는 아이템 목록
    public List<DropEntry> dropTable;

    [Header("Base Stats")]
    // 거점 생산 속도 배율
    [Range(0f, 2f)]
    public float productionSpeedMultiplier = 1f;

    // 거점 제작 속도 배율
    [Range(0f, 2f)]
    public float craftingSpeedMultiplier = 1f;
}

[System.Serializable]
public class DropEntry
{
    public ItemData item;

    // 드랍 확률 (0 = 0%, 1 = 100%)
    [Range(0f, 1f)]
    public float dropChance;

    // 드랍 최소 수량
    public int minQuantity = 1;

    // 드랍 최대 수량
    public int maxQuantity = 1;
}
