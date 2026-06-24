using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlertUIController : MonoBehaviour
{
    [Header("Prefab & Container")]
    [SerializeField] private AlertItemUI _alertPrefab;
    [SerializeField] private Transform _container;

    [Header("Positions")]
    [SerializeField] private RectTransform _spawnPoint;
    [SerializeField] private RectTransform[] _slotPositions;

    [Header("Timing")]
    [SerializeField] private float _moveDuration = 0.3f;
    [SerializeField] private float _lifetime = 3f;
    [SerializeField] private float _exitMoveDistance = 50f;

    private readonly Queue<(string title, string content, Sprite icon)> _queue = new();
    private readonly ActiveAlert[] _activeAlerts = new ActiveAlert[3];
    private bool _isProcessing;

    private class ActiveAlert
    {
        public AlertItemUI Instance;
        public Coroutine LifetimeCoroutine;
        public Coroutine MoveCoroutine;
        public bool IsRemoved;
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        for (int i = 0; i < _activeAlerts.Length; i++)
        {
            if (_activeAlerts[i] != null)
            {
                if (_activeAlerts[i].Instance != null)
                {
                    Destroy(_activeAlerts[i].Instance.gameObject);
                }

                _activeAlerts[i] = null;
            }
        }

        _queue.Clear();
        _isProcessing = false;
    }

    public void Enqueue(string title, string content, Sprite icon)
    {
        _queue.Enqueue((title, content, icon));
        GameCore.SoundManager?.PlayAlert();
        if (!_isProcessing)
        {
            StartCoroutine(ProcessQueue());
        }
    }

    private IEnumerator ProcessQueue()
    {
        _isProcessing = true;

        while (_queue.Count > 0)
        {
            var (title, content, icon) = _queue.Dequeue();
            yield return ProcessAlert(title, content, icon);
        }

        _isProcessing = false;
    }

    private IEnumerator ProcessAlert(string title, string content, Sprite icon)
    {
        if (_activeAlerts[2] != null)
        {
            ForceRemove(_activeAlerts[2]);
            _activeAlerts[2] = null;
        }

        if (_activeAlerts[1] != null)
        {
            if (_activeAlerts[1].MoveCoroutine != null)
                StopCoroutine(_activeAlerts[1].MoveCoroutine);
            _activeAlerts[1].MoveCoroutine = StartCoroutine(_activeAlerts[1].Instance.MoveRoutine(_slotPositions[2].anchoredPosition, _moveDuration));
        }

        if (_activeAlerts[0] != null)
        {
            if (_activeAlerts[0].MoveCoroutine != null)
                StopCoroutine(_activeAlerts[0].MoveCoroutine);
            _activeAlerts[0].MoveCoroutine = StartCoroutine(_activeAlerts[0].Instance.MoveRoutine(_slotPositions[1].anchoredPosition, _moveDuration));
        }

        AlertItemUI newInstance = Instantiate(_alertPrefab, _container);
        newInstance.Apply(icon, title, content);
        newInstance.SetAnchoredPosition(_spawnPoint.anchoredPosition);

        Coroutine newMoveCoroutine = StartCoroutine(newInstance.MoveRoutine(_slotPositions[0].anchoredPosition, _moveDuration, fadeIn: true));

        yield return new WaitForSecondsRealtime(_moveDuration);

        _activeAlerts[2] = _activeAlerts[1];
        _activeAlerts[1] = _activeAlerts[0];

        ActiveAlert newActive = new ActiveAlert
        {
            Instance = newInstance,
            MoveCoroutine = newMoveCoroutine,
            IsRemoved = false
        };

        _activeAlerts[0] = newActive;

        newActive.LifetimeCoroutine = StartCoroutine(LifetimeRoutine(newActive));
    }

    private void ForceRemove(ActiveAlert alert)
    {
        if (alert.IsRemoved)
        {
            return;
        }

        alert.IsRemoved = true;

        if (alert.MoveCoroutine != null)
        {
            StopCoroutine(alert.MoveCoroutine);
            alert.MoveCoroutine = null;
        }

        if (alert.LifetimeCoroutine != null)
        {
            StopCoroutine(alert.LifetimeCoroutine);
        }

        StartCoroutine(ExitAndDestroy(alert.Instance));
    }

    private IEnumerator LifetimeRoutine(ActiveAlert alert)
    {
        yield return new WaitForSecondsRealtime(_lifetime);

        if (alert.IsRemoved)
        {
            yield break;
        }

        alert.IsRemoved = true;

        for (int i = 0; i < _activeAlerts.Length; i++)
        {
            if (_activeAlerts[i] == alert)
            {
                _activeAlerts[i] = null;
                break;
            }
        }

        if (alert.MoveCoroutine != null)
        {
            StopCoroutine(alert.MoveCoroutine);
            alert.MoveCoroutine = null;
        }

        yield return ExitAndDestroy(alert.Instance);
    }

    private IEnumerator ExitAndDestroy(AlertItemUI instance)
    {
        yield return instance.ExitRoutine(_moveDuration, _exitMoveDistance);
        Destroy(instance.gameObject);
    }
}