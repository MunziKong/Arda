using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillSlotUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image _skillIcon;
    [SerializeField] private Image _cooldownFillImage;
    [SerializeField] private TMP_Text _cooldownText;

    private SkillDefinition _currentSkill;

    public SkillDefinition CurrentSkill => _currentSkill;


    public void SetSkill(SkillDefinition skill)
    {
        _currentSkill = skill;

        if (_currentSkill == null)
        {
            Clear();
            return;
        }

        _skillIcon.gameObject.SetActive(true);
        _skillIcon.sprite = _currentSkill.skillIcon;
        SetCooldown(0f, 0f);
    }

    public void Clear()
    {
        _currentSkill = null;

        if (_skillIcon != null)
        {
            _skillIcon.sprite = null;
            _skillIcon.gameObject.SetActive(false);
        }

        SetCooldown(0f, 0f);
    }

    public void SetCooldown(float remaining, float duration)
    {
        if (_cooldownFillImage == null)
            return;

        if (remaining <= 0f)
        {
            _cooldownFillImage.fillAmount = 0f;
            _cooldownFillImage.enabled = false;

            if (_cooldownText != null)
            {
                _cooldownText.text = string.Empty;
                _cooldownText.gameObject.SetActive(false);
            }

            return;
        }

        float normalizedValue = duration > 0f ? remaining / duration : 0f;

        _cooldownFillImage.enabled = true;
        _cooldownFillImage.fillAmount = normalizedValue;

        if (_cooldownText != null)
        {
            _cooldownText.gameObject.SetActive(true);
            _cooldownText.text = Mathf.CeilToInt(remaining).ToString();
        }
    }
}