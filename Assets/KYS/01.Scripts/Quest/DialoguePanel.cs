using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialoguePanel : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;
    [SerializeField] private Image _arrowImage;
    [SerializeField] private Button _backgroundButton;

    [SerializeField] private float _typingSpeed = 0.04f;
    [SerializeField] private float _arrowBlinkInterval = 0.5f;

    private string[] _lines;
    private int _currentLineIndex;
    private bool _isTyping;
    private bool _alwaysShowArrow;
    private Coroutine _typingCoroutine;
    private Coroutine _blinkCoroutine;
    private Action _onComplete;

    private static bool _hasActiveSession;

    public static void BeginSession()
    {
        _hasActiveSession = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public static void EndSession()
    {
        _hasActiveSession = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Awake()
    {
        _backgroundButton.onClick.AddListener(OnClick);
        _arrowImage.gameObject.SetActive(false);
    }

    // alwaysShowArrow: 줄 수와 상관없이 항상 화살표 표시 (BigImagePanel 등 외부에서 진행 제어할 때)
    public void Show(string[] lines, Action onComplete = null, bool alwaysShowArrow = false)
    {
        if (lines == null || lines.Length == 0)
        {
            onComplete?.Invoke();
            return;
        }

        _lines = lines;
        _currentLineIndex = 0;
        _onComplete = onComplete;
        _alwaysShowArrow = alwaysShowArrow;

        if (!_hasActiveSession)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        gameObject.SetActive(true);
        ShowLine(0);
    }

    public void Hide()
    {
        StopAllCoroutines();
        _blinkCoroutine = null;
        _typingCoroutine = null;

        if (!_hasActiveSession)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        gameObject.SetActive(false);
    }

    private void OnClick()
    {
        SoundManager.Instance?.PlayButtonClick();
        if (_isTyping)
        {
            if (_typingCoroutine != null)
            {
                StopCoroutine(_typingCoroutine);
            }

            _isTyping = false;
            _text.maxVisibleCharacters = _lines[_currentLineIndex].Length;

            bool hasNext = _alwaysShowArrow || _currentLineIndex < _lines.Length - 1;
            SetArrow(hasNext);
        }
        else
        {
            _currentLineIndex++;

            if (_currentLineIndex < _lines.Length)
            {
                ShowLine(_currentLineIndex);
            }
            else
            {
                Hide();
                _onComplete?.Invoke();
            }
        }
    }

    private void ShowLine(int index)
    {
        _text.text = _lines[index];
        _text.maxVisibleCharacters = 0;

        // 타이핑 시작부터 화살표 표시
        bool hasNext = _alwaysShowArrow || index < _lines.Length - 1;
        SetArrow(hasNext);

        _typingCoroutine = StartCoroutine(TypeLine(_lines[index]));
    }

    private IEnumerator TypeLine(string line)
    {
        _isTyping = true;

        for (int i = 0; i <= line.Length; i++)
        {
            _text.maxVisibleCharacters = i;
            yield return new WaitForSeconds(_typingSpeed);
        }

        _isTyping = false;
    }

    private void SetArrow(bool visible)
    {
        if (_blinkCoroutine != null)
        {
            StopCoroutine(_blinkCoroutine);
            _blinkCoroutine = null;
        }

        if (visible)
        {
            _arrowImage.gameObject.SetActive(true);
            _arrowImage.enabled = true;
            _blinkCoroutine = StartCoroutine(BlinkArrow());
        }
        else
        {
            _arrowImage.gameObject.SetActive(false);
        }
    }

    private IEnumerator BlinkArrow()
    {
        while (true)
        {
            _arrowImage.enabled = !_arrowImage.enabled;
            yield return new WaitForSeconds(_arrowBlinkInterval);
        }
    }
}
