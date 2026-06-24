using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerOwnedData : MonoBehaviour
{
    [Header("Weapons")]
    [SerializeField] private List<WeaponEnhanceState> _ownedWeapons = new();

    [Header("Attacks")]
    [SerializeField] private List<AttackData> _ownedAttacks = new();

    [Header("Skills")]
    [SerializeField] private List<SkillDefinition> _ownedSkills = new();

    [Header("Skins")]
    [SerializeField] private List<GameObject> _ownedSkins = new();

    [Header("Rescue Stats")]
    [SerializeField] private int _totalRescueCount;
    [SerializeField] private List<MonsterRescueRecord> _monsterRescueRecords = new();

    [Header("Recipes")]
    [SerializeField] private List<RecipeData> _unlockedRecipes = new();

    [Header("Discovered Monsters")]
    [SerializeField] private List<MonsterData> _discoveredMonstersList = new();

    private HashSet<MonsterData> _discoveredMonsters = new();
    private bool _isCodexUnlocked;


    public IReadOnlyList<WeaponEnhanceState> OwnedWeapons => _ownedWeapons;
    public IReadOnlyCollection<MonsterData> DiscoveredMonsters => _discoveredMonsters;
    public IReadOnlyList<AttackData> OwnedAttacks => _ownedAttacks;
    public IReadOnlyList<SkillDefinition> OwnedSkills => _ownedSkills;
    public IReadOnlyList<GameObject> OwnedSkins => _ownedSkins;
    public int TotalRescueCount => _totalRescueCount;
    public IReadOnlyList<RecipeData> UnlockedRecipes => _unlockedRecipes;
    public IReadOnlyList<MonsterRescueRecord> MonsterRescueRecords => _monsterRescueRecords;
    public bool IsCodexUnlocked => _isCodexUnlocked;



    private void Awake()
    {
        ResetEnhanceStates();
        _discoveredMonsters = new HashSet<MonsterData>(_discoveredMonstersList);
    }

    public void ResetEnhanceStates()
    {
        foreach (WeaponEnhanceState state in _ownedWeapons)
        {
            state.CurrentLevel = 0;
            state.PityStack = 0;
            state.TotalBonusDamage = 0;
        }
    }

    public bool AddOwnedWeapon(WeaponData weapon)
    {
        if (weapon == null || _ownedWeapons.Any(s => s.WeaponData == weapon))
        {
            return false;
        }

        _ownedWeapons.Add(new WeaponEnhanceState
        {
            WeaponData = weapon,
            CurrentLevel = 0,
            PityStack = 0,
            TotalBonusDamage = 0
        });

        return true;
    }

    public WeaponEnhanceState GetWeaponEnhanceState(WeaponData weapon)
    {
        return _ownedWeapons.FirstOrDefault(s => s.WeaponData == weapon);
    }

    public bool AddOwnedAttack(AttackData attack)
    {
        if (attack == null || _ownedAttacks.Contains(attack))
        {
            return false;
        }

        _ownedAttacks.Add(attack);
        return true;
    }

    public bool AddOwnedSkill(SkillDefinition skill)
    {
        if (skill == null || _ownedSkills.Contains(skill))
        {
            return false;
        }

        _ownedSkills.Add(skill);
        return true;
    }

    public bool AddOwnedSkin(GameObject skin)
    {
        if (skin == null || _ownedSkins.Contains(skin))
        {
            return false;
        }

        _ownedSkins.Add(skin);
        return true;
    }

    public void IncrementRescueCount(MonsterData monsterData)
    {
        _totalRescueCount++;

        RegisterDiscoveredMonster(monsterData);

        MonsterRescueRecord record = _monsterRescueRecords.Find(r => r.MonsterData == monsterData);

        if (record != null)
        {
            record.Count++;
        }
        else
        {
            _monsterRescueRecords.Add(new MonsterRescueRecord
            {
                MonsterData = monsterData,
                Count = 1
            });
        }
    }

    public int GetMonsterRescueCount(MonsterData monsterData)
    {
        MonsterRescueRecord record = _monsterRescueRecords.Find(r => r.MonsterData == monsterData);
        return record != null ? record.Count : 0;
    }

    public void ResetState()
    {
        _totalRescueCount = 0;
        _monsterRescueRecords.Clear();
    }

    public void FullDebugReset()
    {
        ResetEnhanceStates();
        _totalRescueCount = 0;
        _monsterRescueRecords.Clear();
        _unlockedRecipes.Clear();
        _discoveredMonsters.Clear();
        _discoveredMonstersList.Clear();
        _isCodexUnlocked = false;
    }


    public bool UnlockRecipe(RecipeData recipe)
    {
        if (recipe == null || _unlockedRecipes.Contains(recipe))
        {
            return false;
        }

        _unlockedRecipes.Add(recipe);
        return true;
    }

    public bool IsRecipeUnlocked(RecipeData recipe)
    {
        if (recipe == null)
        {
            return false;
        }

        return _unlockedRecipes.Contains(recipe);
    }

    private void RegisterDiscoveredMonster(MonsterData monsterData)
    {
        if (monsterData == null || _discoveredMonsters.Contains(monsterData))
        {
            return;
        }

        _discoveredMonsters.Add(monsterData);
        _discoveredMonstersList.Add(monsterData);
    }

    public void UnlockCodex()
    {
        _isCodexUnlocked = true;
    }

    public void RestoreMonsterRescueRecord(MonsterData monster, int count)
    {
        if (monster == null || count <= 0) return;

        MonsterRescueRecord existing = _monsterRescueRecords.Find(r => r.MonsterData == monster);
        if (existing != null)
        {
            existing.Count = count;
        }
        else
        {
            _monsterRescueRecords.Add(new MonsterRescueRecord { MonsterData = monster, Count = count });
        }

        _totalRescueCount += count;

        if (!_discoveredMonsters.Contains(monster))
        {
            _discoveredMonsters.Add(monster);
            _discoveredMonstersList.Add(monster);
        }
    }
}