using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TitleCinemachineIntro : MonoBehaviour
{
    [SerializeField] private GameObject _titleUI;
    [SerializeField] private GameObject _firstBackground;
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _continueButton;
    [SerializeField] private Button _settingButton;
    [SerializeField] private Button _exitButton;
    [SerializeField] private float _blendTime = 2f;

    [SerializeField] private IntroControlUIController _intro;

    [Header("첫 실행")]
    [SerializeField] private QuestDefinition _firstQuest;
    [SerializeField] private QuestIntroUI _questIntroUI;
    [SerializeField] private CutsceneController _cutsceneController;
    [SerializeField] private BigImagePanel _bigImagePanel;

    private bool _isStarted;

    private void Awake()
    {
        _startButton.onClick.AddListener(OnStartClick);
        _continueButton.onClick.AddListener(OnContinueClick);
        _settingButton.onClick.AddListener(OnOpenSettings);
        _exitButton.onClick.AddListener(OnExitGame);
        if (!GameCore.GameManager.IsGameStarted)
        {
            GameCore.SoundManager?.PlayBgm(BgmType.Intro);
        }
    }

    private void Start()
    {
        if (GameCore.GameManager != null && GameCore.GameManager.IsGameStarted)
        {
            SkipTitle();
            return;
        }

        bool isFirstLaunch = SaveManager.Instance == null || SaveManager.Instance.Data.isFirstLaunch;

        if (isFirstLaunch)
        {
            ShowFirstLaunch();
        }
        else
        {
            ShowContinueTitle();
        }
    }

    // 완전 첫 실행 — LyingDownCam으로 시작, Start 버튼만 보임
    private void ShowFirstLaunch()
    {
        _isStarted = false;
        _firstBackground.SetActive(true);
        _startButton.gameObject.SetActive(true);
        _continueButton.gameObject.SetActive(false);
        _titleUI.SetActive(true);
        GameCore.GameController.PrepareTitleMode();

        if (_cutsceneController != null)
        {
            _cutsceneController.Prepare();
        }
    }

    // 이후 실행 — 하늘 카메라 인트로
    private void ShowContinueTitle()
    {
        _isStarted = false;
        _firstBackground.SetActive(false);
        _startButton.gameObject.SetActive(false);
        _continueButton.gameObject.SetActive(true);
        _titleUI.SetActive(true);

        if (SaveManager.Instance?.Data != null && SaveManager.Instance.Data.gameTime >= 0f)
            GameTimeManager.Instance?.SetTime(SaveManager.Instance.Data.gameTime);

        GameCore.GameController.PrepareTitleMode();
    }

    // 흐름: QuestIntroUI → 컷씬(대사 0-2) → 빅이미지(대사 3+) → 게임플레이
    private void OnStartClick()
    {
        SoundManager.Instance?.PlayButtonClick();
        if (_isStarted)
        {
            return;
        }

        _isStarted = true;

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.Data.isFirstLaunch = false;
            SaveManager.Instance.Save();
        }

        _firstBackground.SetActive(false);
        _titleUI.SetActive(false);
        GameCore.GameManager.SetGameStarted();

        if (_questIntroUI != null && _firstQuest != null)
        {
            _questIntroUI.PlayIntro(_firstQuest, OnIntroComplete);
        }
        else
        {
            OnIntroComplete();
        }
    }

    // QuestIntroUI 끝 → 컷씬 + 대사 0-2
    private void OnIntroComplete()
    {
        string[] allDialogues = _firstQuest != null ? _firstQuest.startDialogues : null;
        string[] cutsceneDialogues = allDialogues != null ? allDialogues.Take(3).ToArray() : null;

        if (_cutsceneController != null)
        {
            _cutsceneController.Play(cutsceneDialogues, OnCutsceneComplete);
        }
        else
        {
            OnCutsceneComplete();
        }
    }

    // 컷씬 끝 → 빅이미지 + 대사 3+
    private void OnCutsceneComplete()
    {
        string[] allDialogues = _firstQuest != null ? _firstQuest.startDialogues : null;
        string[] bigImageDialogues = allDialogues != null && allDialogues.Length > 3
            ? allDialogues.Skip(3).ToArray()
            : null;

        if (_bigImagePanel != null && bigImageDialogues != null && bigImageDialogues.Length > 0)
        {
            _bigImagePanel.Show(bigImageDialogues, StartGameplaySequence);
        }
        else
        {
            StartGameplaySequence();
        }
    }

    // 빅이미지 끝 → 퀘스트 시작 + 게임플레이
    private void StartGameplaySequence()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.StartQuest(_firstQuest);
        }

        GameCore.GameController.StartGameplayCamera();
        StartCoroutine(WaitAndEnableGameplay());
    }

    // 이후 실행 Continue 클릭
    private void OnContinueClick()
    {
        SoundManager.Instance?.PlayButtonClick();
        if (_isStarted)
        {
            return;
        }

        _isStarted = true;

        _titleUI.SetActive(false);
        GameCore.GameManager.SetGameStarted();

        if (SaveManager.Instance != null && GameCore.GameDatabase != null)
        {
            SaveManager.Instance.ApplyToGame(GameCore.GameDatabase);
        }
        else if (_firstQuest != null && QuestManager.Instance != null)
        {
            QuestManager.Instance.StartQuest(_firstQuest);
        }

        GameCore.GameController.StartGameplayCamera();

        StartCoroutine(WaitAndEnableGameplay());
    }

    private IEnumerator WaitAndEnableGameplay()
    {
        GameCore.SoundManager?.FadeOutBgm(_blendTime, () =>
        {
            GameCore.SoundManager?.PlayBgm(BgmType.Main);
        });

        yield return new WaitForSeconds(_blendTime);

        GameCore.GameController.EnableGameplay();
    }

    public void ReturnToTitle()
    {
        _isStarted = false;
        _firstBackground.SetActive(false);
        _startButton.gameObject.SetActive(false);
        _continueButton.gameObject.SetActive(true);
        _titleUI.SetActive(true);
        GameCore.GameController.PrepareTitleMode();
    }

    private void SkipTitle()
    {
        _isStarted = true;

        _firstBackground.SetActive(false);
        _titleUI.SetActive(false);
        GameCore.GameController.SkipTitleMode();
    }

    private void OnOpenSettings()
    {
        _intro.OpenPanel();
    }

    private void OnExitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
