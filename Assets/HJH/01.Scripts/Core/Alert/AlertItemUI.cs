using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AlertItemUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _contentText;
    [SerializeField] private CanvasGroup _canvasGroup;

    private RectTransform _rectTransform;

    public RectTransform RectTransform => _rectTransform;

    private void Awake()
    {
        _rectTransform = transform as RectTransform;
    }

    public void Apply(Sprite icon, string title, string content)
    {
        _canvasGroup.alpha = 0f;

        _icon.sprite = icon;
        _titleText.text = title;
        _contentText.text = content;
    }

    public void SetAnchoredPosition(Vector2 position)
    {
        _rectTransform.anchoredPosition = position;
    }

    public IEnumerator MoveRoutine(Vector2 targetPosition, float duration, bool fadeIn = false)
    {
        if (this == null || _rectTransform == null) yield break;
        Vector2 startPosition = _rectTransform.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (this == null || _rectTransform == null)
            {
                yield break;
            }

            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            _rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);

            if (fadeIn)
            {
                _canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            }

            yield return null;
        }

        if (_rectTransform != null)
        {
            _rectTransform.anchoredPosition = targetPosition;
        }

        if (fadeIn && _canvasGroup != null)
        {
            _canvasGroup.alpha = 1f;
        }
    }

    public IEnumerator ExitRoutine(float duration, float moveDistance)
    {
        if (this == null || _rectTransform == null || _canvasGroup == null) yield break;
        Vector2 startPosition = _rectTransform.anchoredPosition;
        Vector2 endPosition = startPosition - Vector2.up * moveDistance;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (this == null || _rectTransform == null)
            {
                yield break;
            }

            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            _rectTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, t);
            _canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);

            yield return null;
        }

        if (_rectTransform != null)
        {
            _rectTransform.anchoredPosition = endPosition;
        }

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
        }
    }
}