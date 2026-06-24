using System.Collections.Generic;
using UnityEngine;

public class SkillCooldownTracker : MonoBehaviour
{
    private readonly Dictionary<SkillDefinition, float> _lastCastTimes = new();

    public bool IsReady(SkillDefinition skill)
    {
        if (!_lastCastTimes.TryGetValue(skill, out float lastTime))
        {
            return true;
        }
        return Time.time - lastTime >= skill.cooldown;
    }

    public float GetRemainingCooldown(SkillDefinition skill)
    {
        if (!_lastCastTimes.TryGetValue(skill, out float lastTime))
        {
            return 0f;
        }
        return Mathf.Max(0f, skill.cooldown - (Time.time - lastTime));
    }

    public void StartCooldown(SkillDefinition skill) => _lastCastTimes[skill] = Time.time;

}
