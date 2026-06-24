using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class EndingChoiceData
{
    [TextArea(1, 3)]
    public string[] afterDialogues;
    public bool goToTitle;
}

public class EndingPanel : MonoBehaviour
{
    [Header("사전 대사 (이미지 전)")]
    [SerializeField] private DialoguePanel _preDialoguePanel;
    [TextArea(1, 3)]
    [SerializeField] private string[] _preDialogues;

    [Header("이미지 + 대사 (BigImagePanel)")]
    [SerializeField] private BigImagePanel _bigImagePanel;
    [TextArea(1, 3)]
    [SerializeField] private string[] _imageDialogues;

    [Header("이미지 후 대사 (선택지 전)")]
    [SerializeField] private DialoguePanel _postDialoguePanel;
    [TextArea(1, 3)]
    [SerializeField] private string[] _postDialogues;

    [Header("선택지")]
    [SerializeField] private GameObject _choiceRoot;
    [SerializeField] private Button[] _choiceButtons;
    [SerializeField] private EndingChoiceData[] _choices;

    [Header("선택 후 대사")]
    [SerializeField] private DialoguePanel _resultDialoguePanel;

    [Header("타이틀 복귀")]
    [SerializeField] private TitleCinemachineIntro _titleIntro;
    [SerializeField] private Image _fadeOverlay;
    [SerializeField] private float _fadeDuration = 1.5f;
    [SerializeField] private MainQuestUI _mainQuestUI;

    private Action _onComplete;

    private void Update()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void Awake()
    {
        if (_choiceRoot != null)
            _choiceRoot.SetActive(false);

        if (_fadeOverlay != null)
        {
            Color c = _fadeOverlay.color;
            c.a = 0f;
            _fadeOverlay.color = c;
            _fadeOverlay.gameObject.SetActive(false);
        }
    }

    public void StartEnding(Action onComplete)
    {
        _onComplete = onComplete;
        gameObject.SetActive(true);

        if (_fadeOverlay != null)
        {
            Color c = _fadeOverlay.color;
            c.a = 0f;
            _fadeOverlay.color = c;
            _fadeOverlay.gameObject.SetActive(false);
        }

        if (_preDialoguePanel != null && _preDialogues != null && _preDialogues.Length > 0)
        {
            DialoguePanel.BeginSession();
            _preDialoguePanel.Show(_preDialogues, OnPreDialogueDone);
        }
        else
        {
            OnPreDialogueDone();
        }
    }

    private void OnPreDialogueDone()
    {
        if (_bigImagePanel != null && _imageDialogues != null && _imageDialogues.Length > 0)
        {
            _bigImagePanel.Show(_imageDialogues, OnImagesDone);
        }
        else
        {
            OnImagesDone();
        }
    }

    private void OnImagesDone()
    {
        if (_postDialoguePanel != null && _postDialogues != null && _postDialogues.Length > 0)
        {
            _postDialoguePanel.Show(_postDialogues, ShowChoices);
        }
        else
        {
            ShowChoices();
        }
    }

    private void ShowChoices()
    {
        DialoguePanel.BeginSession();

        if (_choices == null || _choices.Length == 0)
        {
            Finish(false);
            return;
        }

        _choiceRoot.SetActive(true);

        for (int i = 0; i < _choiceButtons.Length; i++)
        {
            if (i < _choices.Length)
            {
                int captured = i;
                _choiceButtons[i].gameObject.SetActive(true);
                _choiceButtons[i].onClick.RemoveAllListeners();
                _choiceButtons[i].onClick.AddListener(() => OnChoiceSelected(_choices[captured]));
            }
            else
            {
                _choiceButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnChoiceSelected(EndingChoiceData choice)
    {
        SoundManager.Instance?.PlayButtonClick();
        _choiceRoot.SetActive(false);

        if (_resultDialoguePanel != null && choice.afterDialogues != null && choice.afterDialogues.Length > 0)
        {
            _resultDialoguePanel.Show(choice.afterDialogues, () => Finish(choice.goToTitle));
        }
        else
        {
            Finish(choice.goToTitle);
        }
    }

    private void Finish(bool goToTitle)
    {
        DialoguePanel.EndSession();

        if (goToTitle && _titleIntro != null)
        {
            StartCoroutine(FadeAndReturnToTitle());
        }
        else
        {
            gameObject.SetActive(false);
            _onComplete?.Invoke();
            _mainQuestUI?.ForceHide();
        }
    }

    private IEnumerator FadeAndReturnToTitle()
    {
        GameCore.GameController.DisablePlayerInteraction();

        if (_fadeOverlay != null)
        {
            _fadeOverlay.gameObject.SetActive(true);
            float elapsed = 0f;
            Color c = _fadeOverlay.color;

            while (elapsed < _fadeDuration)
            {
                elapsed += Time.deltaTime;
                c.a = Mathf.Clamp01(elapsed / _fadeDuration);
                _fadeOverlay.color = c;
                yield return null;
            }

            c.a = 1f;
            _fadeOverlay.color = c;
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }

        // 화면이 완전히 검은 상태에서 카메라 즉시 전환
        GameCore.GameController.SetInstantBlend();
        _onComplete?.Invoke();           // EndInteraction: NPC캠 해제
        _titleIntro.ReturnToTitle();     // PrepareTitleMode: 타이틀캠 전환
        GameCore.GameController.DisableAllOutlines();
        GameCore.GameController.ResetBlend();

        // Cinemachine이 LateUpdate에서 카메라 위치를 반영할 수 있도록 한 프레임 대기
        yield return null;

        gameObject.SetActive(false);     // 오버레이 제거 → 타이틀 화면 노출
    }
}
