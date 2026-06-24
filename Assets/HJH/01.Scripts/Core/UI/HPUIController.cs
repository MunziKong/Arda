using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HPUIController : MonoBehaviour
{
    [SerializeField] private PlayerStats _stats;
    [SerializeField] private TextMeshProUGUI _hpTxt;
    [SerializeField] private Slider _hpSlider;
    [SerializeField] private Image _icon;

    [SerializeField] private float _sliderLerpDuration = 0.5f;

    private Coroutine _sliderCoroutine;

    void OnEnable()
    {
        _stats.ChangeHpEvent += UpdateHPUI;
        UpdateHPUI(_stats.CurrentHp, _stats.MaxHp);
    }

    void OnDisable()
    {
        _stats.ChangeHpEvent -= UpdateHPUI;
    }

    public void UpdateHPUI(int current, int max)
    {
        _hpTxt.text = $"{current} / {max}";

        float targetValue = max > 0 ? (float)current / max : 0f;

        if (_sliderCoroutine != null)
        {
            StopCoroutine(_sliderCoroutine);
        }

        _sliderCoroutine = StartCoroutine(LerpSlider(targetValue));
    }

    private IEnumerator LerpSlider(float targetValue)
    {
        float startValue = _hpSlider.value;
        float elapsed = 0f;

        while (elapsed < _sliderLerpDuration)
        {
            elapsed += Time.deltaTime;
            float timer = elapsed / _sliderLerpDuration;
            _hpSlider.value = Mathf.Lerp(startValue, targetValue, timer);
            yield return null;
        }

        _hpSlider.value = targetValue;
    }
}