using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MessageItemUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _contentText;
    [SerializeField] private CanvasGroup _canvasGroup;

    [Header("Gold")]
    [SerializeField] private Sprite _goldIcon;

    private RectTransform _rectTransform;

    public RectTransform RectTransform => _rectTransform;

    private void Awake()
    {
        _rectTransform = transform as RectTransform;
    }

    public void Apply(MessageData data)
    {
        _canvasGroup.alpha = 1f;

        switch (data.Type)
        {
            case PopupMessageType.Item:
                _icon.sprite = data.Icon;
                _contentText.text = $"{data.Name} x{data.Quantity}";
                break;

            case PopupMessageType.SuccessRescue:
                _icon.sprite = data.Icon;
                _contentText.text = $"{data.Name} 구조 성공!";
                break;

            case PopupMessageType.Gold:
                _icon.sprite = _goldIcon;
                _contentText.text = $"{data.Quantity} 골드";
                break;
            case PopupMessageType.FailRescue:
                _icon.sprite = data.Icon;
                _contentText.text = $"{data.Name} 구조 실패!";
                break;
        }
    }

    public void SetAnchoredPosition(Vector2 position)
    {
        _rectTransform.anchoredPosition = position;
    }

    public IEnumerator MoveRoutine(Vector2 targetPosition, float duration)
    {
        if (this == null || _rectTransform == null) yield break;
        Vector2 startPosition = _rectTransform.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (this == null || _rectTransform == null) yield break;
            elapsed += Time.unscaledDeltaTime;
            float timer = Mathf.Clamp01(elapsed / duration);

            _rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, timer);

            yield return null;
        }

        if (this == null || _rectTransform == null) yield break;
        _rectTransform.anchoredPosition = targetPosition;
    }

    public IEnumerator ExitRoutine(float duration, float moveDistance)
    {
        if (this == null || _rectTransform == null || _canvasGroup == null) yield break;
        Vector2 startPosition = _rectTransform.anchoredPosition;
        Vector2 endPosition = startPosition + Vector2.up * moveDistance;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (this == null || _rectTransform == null || _canvasGroup == null) yield break;
            elapsed += Time.unscaledDeltaTime;
            float timer = Mathf.Clamp01(elapsed / duration);

            _rectTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, timer);
            _canvasGroup.alpha = Mathf.Lerp(1f, 0f, timer);

            yield return null;
        }

        if (this == null || _rectTransform == null || _canvasGroup == null) yield break;
        _rectTransform.anchoredPosition = endPosition;
        _canvasGroup.alpha = 0f;
    }
}