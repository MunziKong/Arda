using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Skill", menuName = "Arda/Skill/Skill")]
public class SkillDefinition : ScriptableObject
{
    [Header("Skill Base")]
    public string skillId;
    public string skillName;
    [TextArea] public string description;
    public float cooldown;
    public SkillTargetType targetType;
    public int animationIndex;
    public ActionDirectionMode actionDirectionMode;
    public WeaponType weaponType;
    public Sprite skillIcon;

    [Header("Aim")]
    public AimMode aimMode;
    public float aimRadius;
    public float aimDistance;

    [Header("Skill Effects")]
    public List<SkillEffectEntry> startEffects = new();
    public List<SkillEffectEntry> applyEffects = new();

    [Header("Skill Prefabs")]
    public List<SkillPrefabEntry> prefabs = new();

    [Header("Weapon")]
    public int weaponSlotIndex = 0;

    [Header("Sfx")]
    public SkillSfxEntry startSfx;
    public SkillSfxEntry applySfx;

    public void ExecuteStart(SkillContext context)
    {
        context.Sfx = startSfx;
        ExecuteEffects(startEffects, context);
    }

    public void ExecuteApply(SkillContext context)
    {
        context.Sfx = applySfx;
        ExecuteEffects(applyEffects, context);
    }

    private void ExecuteEffects(List<SkillEffectEntry> effects, SkillContext context)
    {
        if (effects == null)
        {
            return;
        }

        foreach (var entry in effects)
        {
            if (entry == null || entry.effect == null)
            {
                continue;
            }

            entry.effect.Execute(context, entry.multipliers);
        }
    }

    public GameObject GetPrefab(SkillPrefabType type)
    {
        foreach (var entry in prefabs)
        {
            if (entry.type == type)
            {
                return entry.prefab;
            }
        }

        return null;
    }


}
