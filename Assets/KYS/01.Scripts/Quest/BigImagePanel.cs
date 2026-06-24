using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// 이미지 4개를 앞에서부터 순서대로 페이드아웃하며 대사 진행
// Canvas 정렬: _images[0]이 가장 앞(위), _images[3]이 가장 뒤
public class BigImagePanel : MonoBehaviour
{
    [SerializeField] private Image[] _images;
    [SerializeField] private DialoguePanel _dialoguePanel;
    [SerializeField] private float _imageFadeDuration = 0.3f;

    private string[] _dialogues;
    private int _currentIndex;
    private Action _onComplete;

    public void Show(string[] dialogues, Action onComplete)
    {
        if (dialogues == null || dialogues.Length == 0)
        {
            onComplete?.Invoke();
            return;
        }

        _dialogues = dialogues;
        _currentIndex = 0;
        _onComplete = onComplete;

        foreach (Image img in _images)
        {
            if (img != null)
            {
                img.gameObject.SetActive(true);
                img.raycastTarget = false;
                Color c = img.color;
                c.a = 1f;
                img.color = c;
            }
        }

        DialoguePanel.BeginSession();
        gameObject.SetActive(true);
        _dialoguePanel.Show(new[] { _dialogues[_currentIndex] }, OnLineDone, alwaysShowArrow: true);
    }

    private void OnLineDone()
    {
        StartCoroutine(FadeOutAndNext());
    }

    private IEnumerator FadeOutAndNext()
    {
        if (_currentIndex < _images.Length && _images[_currentIndex] != null)
        {
            yield return StartCoroutine(FadeImage(_images[_currentIndex], 1f, 0f));
            _images[_currentIndex].gameObject.SetActive(false);
        }

        _currentIndex++;

        if (_currentIndex < _dialogues.Length)
        {
            _dialoguePanel.Show(new[] { _dialogues[_currentIndex] }, OnLineDone, alwaysShowArrow: true);
        }
        else
        {
            Finish();
        }
    }

    private void Finish()
    {
        foreach (Image img in _images)
        {
            if (img != null)
            {
                img.gameObject.SetActive(false);
            }
        }

        DialoguePanel.EndSession();
        gameObject.SetActive(false);
        _onComplete?.Invoke();
    }

    private IEnumerator FadeImage(Image image, float from, float to)
    {
        float elapsed = 0f;
        Color c = image.color;
        c.a = from;
        image.color = c;

        while (elapsed < _imageFadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / _imageFadeDuration));
            image.color = c;
            yield return null;
        }

        c.a = to;
        image.color = c;
    }
}
