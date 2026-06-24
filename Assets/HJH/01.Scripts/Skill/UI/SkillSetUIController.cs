using System.Collections.Generic;
using UnityEngine;

public class SkillSetUIController : MonoBehaviour
{
    [Header("Skill Slots")]
    [SerializeField] private SkillSlotUI[] _skillSlots;

    [SerializeField] private PlayerSkillController _skillController;

    private void OnEnable()
    {
        _skillController.OnSkillCooldownUpdate += HandleSkillCooldownUpdate;
    }

    private void OnDisable()
    {
        _skillController.OnSkillCooldownUpdate -= HandleSkillCooldownUpdate;
    }

    private void HandleSkillCooldownUpdate(SkillDefinition skill, float remaining, float duration)
    {
        foreach (var slot in _skillSlots)
        {
            if (slot.CurrentSkill == skill)
            {
                slot.SetCooldown(remaining, duration);
            }
        }
    }

    public void SetSkills(List<SkillDefinition> skills)
    {
        ClearAll();

        if (skills == null)
            return;

        int count = Mathf.Min(skills.Count, _skillSlots.Length);
        for (int i = 0; i < count; i++)
        {
            _skillSlots[i].SetSkill(skills[i]);
        }
    }

    public void ClearAll()
    {
        for (int i = 0; i < _skillSlots.Length; i++)
        {
            _skillSlots[i].Clear();
        }
    }
}