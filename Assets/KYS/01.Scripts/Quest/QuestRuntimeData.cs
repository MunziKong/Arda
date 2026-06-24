using System.Collections.Generic;

public class QuestRuntimeData
{
    public QuestDefinition Quest;
    public int killCount;
    public HashSet<string> interactedObjectIds = new HashSet<string>();
    public Dictionary<string, int> rescuedMonsterCounts = new Dictionary<string, int>();
    public Dictionary<string, int> killCounts = new Dictionary<string, int>();

    public void AddRescue(MonsterData monsterData)
    {
        if (monsterData == null)
        {
            return;
        }

        string key = monsterData.name;
        rescuedMonsterCounts.TryGetValue(key, out int count);
        rescuedMonsterCounts[key] = count + 1;
    }

    public int GetRescueCount(MonsterData monsterData)
    {
        if (monsterData == null)
        {
            return 0;
        }

        return rescuedMonsterCounts.TryGetValue(monsterData.name, out int count) ? count : 0;
    }

    public void AddKill(EnemyData enemyData)
    {
        if (enemyData == null)
        {
            return;
        }

        string key = enemyData.name;
        killCounts.TryGetValue(key, out int count);
        killCounts[key] = count + 1;

        killCount++;
    }

    public int GetKillCount(EnemyData enemyData)
    {
        if (enemyData == null)
        {
            return 0;
        }

        return killCounts.TryGetValue(enemyData.name, out int count) ? count : 0;
    }
}
