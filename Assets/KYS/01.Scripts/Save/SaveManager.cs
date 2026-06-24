using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private const string SAVE_FILE_NAME = "save.json";
    private static string SavePath => Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);

    public SaveData Data { get; private set; }

    private bool _isApplying;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Load();
    }

    private void Start()
    {
        if (GameCore.PlayerInventory != null)
        {
            GameCore.PlayerInventory.OnInventoryChanged += OnInventoryChanged;
        }

        ApplySettingsImmediately();
    }

    private void ApplySettingsImmediately()
    {
        if (Data == null) return;

        // Sound
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetMasterVolume(Data.masterVolume);
            SoundManager.Instance.SetBgmVolume(Data.bgmVolume);
            SoundManager.Instance.SetSfxVolume(Data.sfxVolume);

            SoundManager.Instance.SetMasterMute(Data.isMasterMuted);
            SoundManager.Instance.SetBgmMute(Data.isBgmMuted);
            SoundManager.Instance.SetSfxMute(Data.isSfxMuted);
        }

        // Resolution
        if (GameCore.GameManager != null)
        {
            GameCore.GameManager.SetWindowMode(Data.isFullScreen);
            GameCore.GameManager.SetResolution(Data.resolutionWidth, Data.resolutionHeight);
        }
    }

    private void OnDestroy()
    {
        if (GameCore.PlayerInventory != null)
        {
            GameCore.PlayerInventory.OnInventoryChanged -= OnInventoryChanged;
        }
    }

    private void OnInventoryChanged()
    {
        if (!_isApplying)
        {
            Save();
        }
    }

    public void Save()
    {
        if (_isApplying)
        {
            return;
        }

        // Quest
        QuestManager qm = QuestManager.Instance;
        if (qm != null)
        {
            Data.activeQuestId = qm.ActiveQuest != null ? qm.ActiveQuest.questId : "";
            Data.pendingQuestId = qm.PendingQuest != null ? qm.PendingQuest.questId : "";
            Data.isQuestCompleted = qm.IsQuestCompleted;

            Data.questProgress = new QuestProgressSaveData();
            QuestRuntimeData runtime = qm.RuntimeData;
            if (runtime != null)
            {
                Data.questProgress.interactedObjectIds = new List<string>(runtime.interactedObjectIds);

                foreach (KeyValuePair<string, int> kvp in runtime.rescuedMonsterCounts)
                {
                    Data.questProgress.rescuedMonsters.Add(new MonsterRescueSaveEntry
                    {
                        monsterName = kvp.Key,
                        count = kvp.Value
                    });
                }

                foreach (KeyValuePair<string, int> kvp in runtime.killCounts)
                {
                    Data.questProgress.killedEnemies.Add(new EnemyKillSaveEntry
                    {
                        enemyName = kvp.Key,
                        count = kvp.Value
                    });
                }
            }
        }

        // Inventory
        PlayerInventory inventory = GameCore.PlayerInventory;
        if (inventory != null)
        {
            Data.inventoryItems = new List<ItemSaveEntry>();
            foreach (InventoryItem item in inventory.Items)
            {
                if (item.ItemData == null)
                {
                    continue;
                }

                Data.inventoryItems.Add(new ItemSaveEntry
                {
                    itemId = item.ItemData.ItemId,
                    quantity = item.Quantity
                });
            }
            Data.gold = inventory.Gold;
        }

        // UI State
        Data.quickSlotUnlocked = GameCore.IsQuickSlotPanelActive;
        Data.skillSetUnlocked = GameCore.IsSkillSetPanelActive;
        Data.codexUnlocked = GameCore.PlayerOwnedData.IsCodexUnlocked;

        // Weapon Enhance
        PlayerOwnedData ownedData = GameCore.PlayerOwnedData;
        if (ownedData != null)
        {
            Data.weaponEnhanceStates = new List<WeaponEnhanceSaveEntry>();
            foreach (WeaponEnhanceState state in ownedData.OwnedWeapons)
            {
                if (state.WeaponData != null)
                {
                    Data.weaponEnhanceStates.Add(new WeaponEnhanceSaveEntry
                    {
                        weaponName = state.WeaponData.name,
                        currentLevel = state.CurrentLevel,
                        pityStack = state.PityStack,
                        totalBonusDamage = state.TotalBonusDamage
                    });
                }
            }

            // Owned Skills
            Data.ownedSkillNames = ownedData.OwnedSkills
                .Where(s => s != null)
                .Select(s => s.name)
                .ToList();

            // Owned Skins
            Data.ownedSkinNames = ownedData.OwnedSkins
                .Where(s => s != null)
                .Select(s => s.name)
                .ToList();

            // Unlocked Recipes
            Data.unlockedRecipeNames = ownedData.UnlockedRecipes
                .Where(r => r != null)
                .Select(r => r.name)
                .ToList();

            // Codex
            Data.monsterRescueRecords = new List<MonsterRescueSaveEntry>();
            foreach (MonsterRescueRecord record in ownedData.MonsterRescueRecords)
            {
                if (record.MonsterData != null)
                {
                    Data.monsterRescueRecords.Add(new MonsterRescueSaveEntry
                    {
                        monsterName = record.MonsterData.name,
                        count = record.Count
                    });
                }
            }
        }

        // Loadout
        LoadoutManager lm = GameCore.LoadoutManager;
        if (lm != null && lm.HasValidLoadout())
        {
            LoadoutData loadout = lm.CurrentLoadout;
            Data.loadout = new LoadoutSaveData
            {
                weaponName = loadout.Weapon != null ? loadout.Weapon.name : "",
                attackName = loadout.BasicAttack != null ? loadout.BasicAttack.name : "",
                skillNames = loadout.Skills != null
                    ? loadout.Skills.Select(s => s.name).ToList()
                    : new List<string>(),
                skinName = loadout.Skin != null ? loadout.Skin.name : ""
            };
        }

        // Player State
        PlayerStats playerStats = GameCore.PlayerStats;
        if (playerStats != null)
        {
            Data.playerHp = playerStats.CurrentHp;
        }

        ConsumableQuickSlotUI consumableSlot = Object.FindFirstObjectByType<ConsumableQuickSlotUI>(FindObjectsInactive.Include);
        if (consumableSlot != null && consumableSlot.CurrentItemData != null)
        {
            Data.consumableQuickSlotItemId = consumableSlot.CurrentItemData.ItemId;
        }
        else
        {
            Data.consumableQuickSlotItemId = "";
        }

        RescueQuickSlotUI rescueSlot = GameCore.RescueQuickSlotUI;
        if (rescueSlot != null && rescueSlot.CurrentItem != null)
        {
            Data.rescueQuickSlotItemId = rescueSlot.CurrentItem.ItemId;
        }
        else
        {
            Data.rescueQuickSlotItemId = "";
        }

        if (GameTimeManager.Instance != null)
        {
            Data.gameTime = GameTimeManager.Instance.CurrentHour;
        }

        // Resolution
        Data.isFullScreen = GameCore.GameManager.IsFullScreen;
        Data.resolutionWidth = GameCore.GameManager.ResolutionWidth;
        Data.resolutionHeight = GameCore.GameManager.ResolutionHeight;

        // Sound
        if (SoundManager.Instance != null)
        {
            Data.masterVolume = SoundManager.Instance.MasterVolume;
            Data.bgmVolume = SoundManager.Instance.BgmVolume;
            Data.sfxVolume = SoundManager.Instance.SfxVolume;
            Data.isMasterMuted = SoundManager.Instance.IsMasterMuted;
            Data.isBgmMuted = SoundManager.Instance.IsBgmMuted;
            Data.isSfxMuted = SoundManager.Instance.IsSfxMuted;
        }

        string json = JsonUtility.ToJson(Data, true);
        File.WriteAllText(SavePath, json);
    }

    public void ApplyToGame(GameDatabase database)
    {
        if (Data == null || database == null)
        {
            return;
        }

        _isApplying = true;

        // Inventory
        PlayerInventory inventory = GameCore.PlayerInventory;
        if (inventory != null)
        {
            inventory.ResetState();

            foreach (ItemSaveEntry entry in Data.inventoryItems)
            {
                ItemData item = database.GetItem(entry.itemId);
                if (item != null)
                {
                    inventory.AddItem(item, entry.quantity);
                }
            }

            if (Data.gold > 0)
            {
                inventory.AddGold(Data.gold);
            }
        }

        // Quest
        QuestManager qm = QuestManager.Instance;
        if (qm != null && !string.IsNullOrEmpty(Data.activeQuestId))
        {
            QuestDefinition quest = database.GetQuest(Data.activeQuestId);
            if (quest != null)
            {
                qm.RestoreQuest(quest);

                QuestRuntimeData runtime = qm.RuntimeData;
                if (runtime != null)
                {
                    foreach (string id in Data.questProgress.interactedObjectIds)
                    {
                        runtime.interactedObjectIds.Add(id);
                    }

                    foreach (MonsterRescueSaveEntry entry in Data.questProgress.rescuedMonsters)
                    {
                        MonsterData monster = database.GetMonster(entry.monsterName);
                        if (monster != null)
                        {
                            runtime.rescuedMonsterCounts[monster.name] = entry.count;
                        }
                    }

                    foreach (EnemyKillSaveEntry entry in Data.questProgress.killedEnemies)
                    {
                        EnemyData enemy = database.GetEnemy(entry.enemyName);
                        if (enemy != null)
                        {
                            runtime.killCounts[enemy.name] = entry.count;
                            runtime.killCount += entry.count;
                        }
                    }

                    if (Data.isQuestCompleted)
                    {
                        qm.DebugForceComplete();
                    }
                }
            }
        }

        if (!string.IsNullOrEmpty(Data.pendingQuestId))
        {
            QuestDefinition pending = database.GetQuest(Data.pendingQuestId);
            if (pending != null && QuestManager.Instance != null)
            {
                QuestManager.Instance.SetPendingQuest(pending);
            }
        }

        // Weapon Enhance
        PlayerOwnedData ownedData = GameCore.PlayerOwnedData;
        if (ownedData != null && Data.weaponEnhanceStates != null)
        {
            foreach (WeaponEnhanceSaveEntry entry in Data.weaponEnhanceStates)
            {
                WeaponData weapon = database.GetWeapon(entry.weaponName);
                if (weapon != null)
                {
                    WeaponEnhanceState state = ownedData.GetWeaponEnhanceState(weapon);
                    if (state != null)
                    {
                        state.CurrentLevel = entry.currentLevel;
                        state.PityStack = entry.pityStack;
                        state.TotalBonusDamage = entry.totalBonusDamage;
                    }
                }
            }
        }

        // Owned Skills
        if (ownedData != null && Data.ownedSkillNames != null)
        {
            foreach (string skillName in Data.ownedSkillNames)
            {
                SkillDefinition skill = database.GetSkill(skillName);
                if (skill != null)
                    ownedData.AddOwnedSkill(skill);
            }
        }

        // Owned Skins
        if (ownedData != null && Data.ownedSkinNames != null)
        {
            foreach (string skinName in Data.ownedSkinNames)
            {
                GameObject skin = database.GetSkin(skinName);
                Debug.Log($"[SaveManager] 스킨 복원 시도: {skinName} / 찾음: {skin != null}");
                if (skin != null)
                    ownedData.AddOwnedSkin(skin);
            }
        }


        // Unlocked Recipes
        if (ownedData != null && Data.unlockedRecipeNames != null)
        {
            foreach (string recipeName in Data.unlockedRecipeNames)
            {
                RecipeData recipe = database.GetRecipe(recipeName);
                if (recipe != null)
                    ownedData.UnlockRecipe(recipe);
            }
        }

        // Codex
        if (ownedData != null && Data.monsterRescueRecords != null)
        {
            foreach (MonsterRescueSaveEntry entry in Data.monsterRescueRecords)
            {
                MonsterData monster = database.GetMonster(entry.monsterName);
                if (monster != null)
                    ownedData.RestoreMonsterRescueRecord(monster, entry.count);
            }
        }

        // Loadout
        LoadoutManager lm = GameCore.LoadoutManager;
        if (lm != null && !string.IsNullOrEmpty(Data.loadout.weaponName))
        {
            WeaponData weapon = database.GetWeapon(Data.loadout.weaponName);
            AttackData attack = database.GetAttack(Data.loadout.attackName);
            List<SkillDefinition> skills = Data.loadout.skillNames
                .Select(n => database.GetSkill(n))
                .Where(s => s != null)
                .ToList();
            GameObject skin = database.GetSkin(Data.loadout.skinName);

            if (weapon != null && attack != null)
            {
                lm.SetLoadout(weapon, attack, skills, skin);
            }
        }

        // UI State
        if (Data.quickSlotUnlocked)
        {
            GameCore.ActivateQuickSlotPanel();
        }

        if (Data.skillSetUnlocked)
        {
            GameCore.ActivateSkillSetPanel();
        }

        if (Data.codexUnlocked)
        {
            GameCore.PlayerOwnedData.UnlockCodex();
            GameCore.ActivateCodexIconPanel();
        }

        // Player State
        if (Data.playerHp > 0 && GameCore.PlayerStats != null)
        {
            GameCore.PlayerStats.RestoreHp(Data.playerHp);
        }

        if (!string.IsNullOrEmpty(Data.consumableQuickSlotItemId))
        {
            ConsumableItemData itemData = database.GetItem(Data.consumableQuickSlotItemId) as ConsumableItemData;
            if (itemData != null)
            {
                ConsumableQuickSlotUI consumableSlot = Object.FindFirstObjectByType<ConsumableQuickSlotUI>(FindObjectsInactive.Include);
                if (consumableSlot != null)
                {
                    consumableSlot.RegisterItem(itemData);
                }
            }
        }

        if (!string.IsNullOrEmpty(Data.rescueQuickSlotItemId))
        {
            RescueQuickSlotUI rescueSlot = GameCore.RescueQuickSlotUI;
            if (rescueSlot != null)
            {
                rescueSlot.RestoreItemById(Data.rescueQuickSlotItemId);
            }
        }

        if (Data.gameTime >= 0f && GameTimeManager.Instance != null)
        {
            GameTimeManager.Instance.SetTime(Data.gameTime);
        }

        // Resolution
        GameCore.GameManager.SetWindowMode(Data.isFullScreen);
        GameCore.GameManager.SetResolution(Data.resolutionWidth, Data.resolutionHeight);

        // Sound
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetMasterVolume(Data.masterVolume);
            SoundManager.Instance.SetBgmVolume(Data.bgmVolume);
            SoundManager.Instance.SetSfxVolume(Data.sfxVolume);

            SoundManager.Instance.SetMasterMute(Data.isMasterMuted);
            SoundManager.Instance.SetBgmMute(Data.isBgmMuted);
            SoundManager.Instance.SetSfxMute(Data.isSfxMuted);
        }

        _isApplying = false;
    }

    private void Load()
    {
        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            Data = JsonUtility.FromJson<SaveData>(json);
        }
        else
        {
            Data = new SaveData();
        }
    }

    [ContextMenu("세이브 초기화 (테스트용)")]
    public void ResetSave()
    {
        Data = new SaveData();
        Save();
        Debug.Log("[SaveManager] 세이브 데이터 초기화 완료");
    }
}