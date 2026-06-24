using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;

public class CutsceneController : MonoBehaviour
{
    [SerializeField] private PlayableDirector _director;
    [SerializeField] private CinemachineCamera _lyingDownCam;
    [SerializeField] private DialoguePanel _dialoguePanel;
    [SerializeField] private GameObject[] _cutsceneOnlyObjects;
    [SerializeField] private GameObject[] _hideOnCutscene;
    [SerializeField] private bool _hidePlayer = true;

    private bool _timelineDone;

    // Start 버튼 누르기 전에 호출 — LyingDownCam Priority 높여서 즉시 보이게
    public void Prepare()
    {
        _director.playOnAwake = false;
        gameObject.SetActive(true);

        if (_lyingDownCam != null)
        {
            _lyingDownCam.Priority = 1000;
        }

        if (_hidePlayer && GameCore.PlayerInputs != null)
        {
            GameCore.PlayerInputs.gameObject.SetActive(false);
        }

        foreach (GameObject obj in _hideOnCutscene)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
    }

    // Timeline + 대사 동시 재생, 둘 다 끝나면 onEnd 호출
    public void Play(string[] dialogues, Action onEnd)
    {
        StartCoroutine(PlayRoutine(dialogues, onEnd));
    }

    private IEnumerator PlayRoutine(string[] dialogues, Action onEnd)
    {
        _timelineDone = false;
        bool dialogueDone = false;

        _director.stopped += OnDirectorStopped;
        _director.Play();

        if (_dialoguePanel != null && dialogues != null && dialogues.Length > 0)
        {
            _dialoguePanel.Show(dialogues, () => dialogueDone = true);
        }
        else
        {
            dialogueDone = true;
        }

        yield return new WaitUntil(() => _timelineDone && dialogueDone);

        if (_lyingDownCam != null)
        {
            _lyingDownCam.Priority = 0;
        }

        foreach (GameObject obj in _cutsceneOnlyObjects)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }

        if (_hidePlayer && GameCore.PlayerInputs != null)
        {
            GameCore.PlayerInputs.gameObject.SetActive(true);
        }

        foreach (GameObject obj in _hideOnCutscene)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }

        onEnd?.Invoke();
        gameObject.SetActive(false);
    }

    private void OnDirectorStopped(PlayableDirector director)
    {
        _director.stopped -= OnDirectorStopped;
        _timelineDone = true;
    }
}
