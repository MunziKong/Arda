using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    [SerializeField] private Transform _player;

    private string _targetSpawnId;
    private bool _pendingShowAlert = true;
    private bool _isReviving;
    private bool _skipFade;


    private readonly Dictionary<string, SceneSpawnPoint> _spawnPoints = new();

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void MoveToScene(string sceneName, string spawnId, bool showAlert = true)
    {
        _pendingShowAlert = showAlert;

        if (SceneManager.GetActiveScene().name == sceneName)
        {
            MovePlayerToSpawnPoint(spawnId);
            return;
        }

        _targetSpawnId = spawnId;
        StartCoroutine(TransitionRoutine(sceneName));
    }

    public void MoveToSceneAfterDeath(string sceneName, string spawnId)
    {
        _isReviving = true;
        _targetSpawnId = spawnId;
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator TransitionRoutine(string sceneName)
    {
        GameCore.PlayerSkillController?.ResetAllCooldowns();
        GameCore.PlayerInputs.SetUIInput();
        GameCore.PlayerInputs.SetCursorLock(true);
        GameCore.FadePanel.gameObject.SetActive(true);

        bool fadeOutDone = false;
        GameCore.FadePanel.FadeOut(2f, () => fadeOutDone = true);
        yield return new WaitUntil(() => fadeOutDone);

        SceneManager.LoadScene(sceneName);
    }



    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (string.IsNullOrEmpty(_targetSpawnId))
        {
            return;
        }

        if (SceneManager.GetActiveScene().name != SceneNames.GetSceneName(SceneType.Main))
        {
            GameCore.RescueManager.OpenBoard();

            if (_pendingShowAlert)
            {
                GameCore.AlertManager.Enqueue(AlertType.MapEnter, "녹색 숲");
                GameCore.SoundManager?.PlayBgm(BgmType.Forest);
            }
        }
        else
        {
            if (_pendingShowAlert)
            {
                GameCore.AlertManager.Enqueue(AlertType.MapEnter, "거점");
                GameCore.SoundManager?.PlayBgm(BgmType.Main);
            }

            if (_isReviving)
            {
                _isReviving = false;
                MovePlayerToSpawnPoint(_targetSpawnId);
                StartCoroutine(ReviveRoutine());
                _targetSpawnId = null;
                _pendingShowAlert = true;
                return;
            }
        }

        if (_skipFade)
        {
            MovePlayerToSpawnPoint(_targetSpawnId);
            _skipFade = false;
        }
        else
        {
            StartCoroutine(FadeInRoutine(_targetSpawnId));
        }

        _targetSpawnId = null;
        _pendingShowAlert = true;
    }

    private IEnumerator FadeInRoutine(string spawnId)
    {
        MovePlayerToSpawnPoint(spawnId);
        bool fadeInDone = false;
        GameCore.FadePanel.FadeIn(2f, () => fadeInDone = true);
        yield return new WaitUntil(() => fadeInDone);

        GameCore.FadePanel.gameObject.SetActive(false);

        GameCore.PlayerInputs.SetPlayerInput();
        GameCore.PlayerInputs.SetCursorLock(true);
    }

    private IEnumerator ReviveRoutine()
    {
        GameCore.FadePanel.SetBlack();

        yield return new WaitForSecondsRealtime(0.5f);

        int halfHp = GameCore.PlayerStats.MaxHp / 2;
        GameCore.PlayerStats.Heal(halfHp);
        GameCore.PlayerAnimController?.ResetToIdle();
        GameCore.SoundManager?.PlayBgm(BgmType.Main);

        bool fadeInDone = false;
        GameCore.FadePanel.FadeIn(1f, () => fadeInDone = true);
        yield return new WaitUntil(() => fadeInDone);

        GameCore.FadePanel.gameObject.SetActive(false);

        GameCore.GameController.ActiveCoreUI();
        GameCore.PlayerInputs.SetPlayerInput();
        GameCore.PlayerInputs.SetCursorLock(true);
        GameCore.PlayerDeathHandler?.ResetDying();

        _targetSpawnId = null;
        _pendingShowAlert = true;
    }

    public void RegisterSpawnPoint(SceneSpawnPoint spawnPoint)
    {
        if (spawnPoint == null || string.IsNullOrEmpty(spawnPoint.SpawnId)) return;

        if (_spawnPoints.ContainsKey(spawnPoint.SpawnId))
        {
            Debug.LogWarning($"중복 SpawnId가 등록되었습니다: {spawnPoint.SpawnId}");
        }

        _spawnPoints[spawnPoint.SpawnId] = spawnPoint;
    }

    public void UnregisterSpawnPoint(SceneSpawnPoint spawnPoint)
    {
        if (spawnPoint == null || string.IsNullOrEmpty(spawnPoint.SpawnId)) return;

        if (_spawnPoints.TryGetValue(spawnPoint.SpawnId, out SceneSpawnPoint registeredSpawnPoint))
        {
            if (registeredSpawnPoint == spawnPoint)
            {
                _spawnPoints.Remove(spawnPoint.SpawnId);
            }
        }
    }

    private void MovePlayerToSpawnPoint(string spawnId)
    {
        if (!_spawnPoints.TryGetValue(spawnId, out SceneSpawnPoint spawnPoint))
        {
            Debug.LogWarning($"SpawnPoint를 찾을 수 없습니다. SpawnId: {spawnId}");
            return;
        }

        if (_player == null)
        {
            Debug.LogWarning("SceneTransitionManager에 Player가 등록되어 있지 않습니다.");
            return;
        }

        CharacterController controller = _player.GetComponent<CharacterController>();
        if (controller != null) controller.enabled = false;

        _player.position = spawnPoint.transform.position;
        _player.rotation = spawnPoint.transform.rotation;

        if (controller != null) controller.enabled = true;

        PlayerMove playerMove = _player.GetComponent<PlayerMove>();
        playerMove?.SuppressNextLandSound();
    }

    public void MoveToSceneDirect(string sceneName, string spawnId)
    {
        _pendingShowAlert = false;
        _skipFade = true;
        _targetSpawnId = spawnId;
        SceneManager.LoadScene(sceneName);
    }
}