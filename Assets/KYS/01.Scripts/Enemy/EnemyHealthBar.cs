using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Slider _slider;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private float _showDuration = 2f;
    [SerializeField] private float _fadeDuration = 0.3f;

    private Coroutine _hideCoroutine;

    private void Awake()
    {
        _canvasGroup.alpha = 0f;
    }

    public void Show(float hpRatio)
    {
        _slider.value = Mathf.Clamp01(hpRatio);
        _canvasGroup.alpha = 1f;

        if (_hideCoroutine != null)
        {
            StopCoroutine(_hideCoroutine);
        }
        _hideCoroutine = StartCoroutine(HideRoutine());
    }

    public void Hide()
    {
        if (_hideCoroutine != null)
        {
            StopCoroutine(_hideCoroutine);
        }
        _canvasGroup.alpha = 0f;
    }

    private IEnumerator HideRoutine()
    {
        yield return new WaitForSeconds(_showDuration);

        float elapsed = 0f;
        while (elapsed < _fadeDuration)
        {
            elapsed += Time.deltaTime;
            _canvasGroup.alpha = 1f - (elapsed / _fadeDuration);
            yield return null;
        }

        _canvasGroup.alpha = 0f;
    }

    private void LateUpdate()
    {
        if (Camera.main != null)
        {
            transform.rotation = Camera.main.transform.rotation;
        }
    }
}
