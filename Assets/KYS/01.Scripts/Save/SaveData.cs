using System;
using System.Collections.Generic;

[Serializable]
public class ItemSaveEntry
{
    public string itemId;
    public int quantity;
}

[Serializable]
public class MonsterRescueSaveEntry
{
    public string monsterName;
    public int count;
}

[Serializable]
public class EnemyKillSaveEntry
{
    public string enemyName;
    public int count;
}

[Serializable]
public class QuestProgressSaveData
{
    public List<string> interactedObjectIds = new List<string>();
    public List<MonsterRescueSaveEntry> rescuedMonsters = new List<MonsterRescueSaveEntry>();
    public List<EnemyKillSaveEntry> killedEnemies = new List<EnemyKillSaveEntry>();
}

[Serializable]
public class WeaponEnhanceSaveEntry
{
    public string weaponName;
    public int currentLevel;
    public int pityStack;
    public int totalBonusDamage;
}

[Serializable]
public class LoadoutSaveData
{
    public string weaponName;
    public string attackName;
    public List<string> skillNames = new List<string>();
    public string skinName;
}

[Serializable]
public class SaveData
{
    public bool isFirstLaunch = true;

    // Quest
    public string activeQuestId;
    public string pendingQuestId;
    public string lastStartedQuestId;
    public bool isQuestCompleted;
    public bool endingCompleted;
    public QuestProgressSaveData questProgress = new QuestProgressSaveData();

    // UI State
    public bool quickSlotUnlocked;
    public bool skillSetUnlocked;
    public bool fullMenuUnlocked;
    public bool codexUnlocked;

    // Resolution
    public bool isFullScreen = true;
    public int resolutionWidth = 1920;
    public int resolutionHeight = 1080;

    // Sound
    public float masterVolume = 1f;
    public float bgmVolume = 1f;
    public float sfxVolume = 1f;
    public bool isMasterMuted;
    public bool isBgmMuted;
    public bool isSfxMuted;

    // Inventory
    public List<ItemSaveEntry> inventoryItems = new List<ItemSaveEntry>();
    public int gold;

    // Weapon Enhance
    public List<WeaponEnhanceSaveEntry> weaponEnhanceStates = new List<WeaponEnhanceSaveEntry>();

    // Loadout
    public LoadoutSaveData loadout = new LoadoutSaveData();

    // Shop Purchases
    public List<string> ownedSkillNames = new List<string>();
    public List<string> unlockedRecipeNames = new List<string>();
    public List<string> ownedSkinNames = new();

    // Codex
    public List<MonsterRescueSaveEntry> monsterRescueRecords = new List<MonsterRescueSaveEntry>();
    public List<string> discoveredMonsterNames = new List<string>();

    // Player State
    public int playerHp = -1;
    public string consumableQuickSlotItemId;
    public string rescueQuickSlotItemId;
    public float gameTime = -1f;
}
