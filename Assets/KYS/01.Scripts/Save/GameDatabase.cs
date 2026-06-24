using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameDatabase : MonoBehaviour
{
    [Header("Quests")]
    [SerializeField] private QuestDefinition[] _quests;

    [Header("Items")]
    [SerializeField] private ItemData[] _items;

    [Header("Monsters")]
    [SerializeField] private MonsterData[] _monsters;

    [Header("Enemies")]
    [SerializeField] private EnemyData[] _enemies;

    [Header("Loadout")]
    [SerializeField] private WeaponData[] _weapons;
    [SerializeField] private AttackData[] _attacks;
    [SerializeField] private SkillDefinition[] _skills;
    [SerializeField] private GameObject[] _skins;

    [Header("Craft")]
    [SerializeField] private RecipeData[] _recipes;

    public ItemData[] AllItems => _items;

    public QuestDefinition GetQuest(string id)
    {
        return Array.Find(_quests, q => q.questId == id);
    }

    public ItemData GetItem(string id)
    {
        return Array.Find(_items, i => i.ItemId == id);
    }

    public MonsterData GetMonster(string monsterName)
    {
        return Array.Find(_monsters, m => m.name == monsterName);
    }

    public EnemyData GetEnemy(string enemyName)
    {
        return Array.Find(_enemies, e => e.name == enemyName);
    }

    public WeaponData GetWeapon(string assetName)
    {
        return Array.Find(_weapons, w => w.name == assetName);
    }

    public AttackData GetAttack(string assetName)
    {
        return Array.Find(_attacks, a => a.name == assetName);
    }

    public SkillDefinition GetSkill(string assetName)
    {
        return Array.Find(_skills, s => s.name == assetName);
    }

    public GameObject GetSkin(string assetName)
    {
        return Array.Find(_skins, s => s != null && s.name == assetName);
    }

    public RecipeData GetRecipe(string assetName)
    {
        return Array.Find(_recipes, r => r != null && r.name == assetName);
    }

#if UNITY_EDITOR
    [ContextMenu("S.O 폴더에서 자동 등록")]
    private void AutoPopulate()
    {
        string[] soFolder = new[] { "Assets/02.S.O" };

        _quests  = FindAssets<QuestDefinition>(soFolder);
        _items   = FindAssets<ItemData>(soFolder);
        _monsters = FindAssets<MonsterData>(soFolder);
        _enemies  = FindAssets<EnemyData>(soFolder);
        _weapons  = FindAssets<WeaponData>(soFolder);
        _attacks  = FindAssets<AttackData>(soFolder);
        _skills   = FindAssets<SkillDefinition>(soFolder);
        _recipes  = FindAssets<RecipeData>(soFolder);

        EditorUtility.SetDirty(this);

        Debug.Log($"[GameDatabase] 자동 등록 완료\n" +
                  $"퀘스트: {_quests.Length}  아이템: {_items.Length}  몬스터: {_monsters.Length}\n" +
                  $"무기: {_weapons.Length}  공격: {_attacks.Length}  스킬: {_skills.Length}\n" +
                  $"레시피: {_recipes.Length}  스킨: {_skins.Length} (프리팹은 수동 등록)");
    }

    private T[] FindAssets<T>(string[] searchFolders) where T : UnityEngine.Object
    {
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", searchFolders);
        T[] results = new T[guids.Length];

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            results[i] = AssetDatabase.LoadAssetAtPath<T>(path);
        }

        return results;
    }
#endif
}
