using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageUIController : MonoBehaviour
{
    [Header("Content")]
    [SerializeField] private MessageItemUI _messagePrefab;
    [SerializeField] private Transform _container;

    [Header("Position")]
    [SerializeField] private RectTransform _spawnPoint;
    [SerializeField] private RectTransform[] _slotPositions;

    [Header("Time")]
    [SerializeField] private float _moveDuration = 0.3f;
    [SerializeField] private float _lifetime = 3f;
    [SerializeField] private float _exitMoveDistance = 50f;

    private readonly Queue<MessageData> _queue = new();
    private readonly ActiveMessage[] _activeMessages = new ActiveMessage[3];
    private bool _isProcessing;

    private class ActiveMessage
    {
        public MessageItemUI Instance;
        public Coroutine LifetimeCoroutine;
        public Coroutine MoveCoroutine;
        public bool IsRemoved;
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        for (int i = _container.childCount - 1; i >= 0; i--)
        {
            Destroy(_container.GetChild(i).gameObject);
        }

        for (int i = 0; i < _activeMessages.Length; i++)
        {
            _activeMessages[i] = null;
        }

        _queue.Clear();
        _isProcessing = false;
    }

    public void Enqueue(MessageData data)
    {
        if (data == null)
        {
            return;
        }

        _queue.Enqueue(data);

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
            MessageData data = _queue.Dequeue();
            yield return ProcessMessage(data);
        }

        _isProcessing = false;
    }

    private IEnumerator ProcessMessage(MessageData data)
    {
        if (_activeMessages[2] != null)
        {
            ForceRemove(_activeMessages[2]);
            _activeMessages[2] = null;
        }

        if (_activeMessages[1] != null)
        {
            if (_activeMessages[1].MoveCoroutine != null)
                StopCoroutine(_activeMessages[1].MoveCoroutine);
            _activeMessages[1].MoveCoroutine = StartCoroutine(_activeMessages[1].Instance.MoveRoutine(_slotPositions[2].anchoredPosition, _moveDuration));
        }

        if (_activeMessages[0] != null)
        {
            if (_activeMessages[0].MoveCoroutine != null)
                StopCoroutine(_activeMessages[0].MoveCoroutine);
            _activeMessages[0].MoveCoroutine = StartCoroutine(_activeMessages[0].Instance.MoveRoutine(_slotPositions[1].anchoredPosition, _moveDuration));
        }

        MessageItemUI newInstance = Instantiate(_messagePrefab, _container);
        newInstance.Apply(data);
        newInstance.SetAnchoredPosition(_spawnPoint.anchoredPosition);

        Coroutine newMoveCoroutine = StartCoroutine(newInstance.MoveRoutine(_slotPositions[0].anchoredPosition, _moveDuration));

        yield return new WaitForSecondsRealtime(_moveDuration);

        _activeMessages[2] = _activeMessages[1];
        _activeMessages[1] = _activeMessages[0];

        ActiveMessage newActive = new ActiveMessage
        {
            Instance = newInstance,
            MoveCoroutine = newMoveCoroutine,
            IsRemoved = false
        };

        _activeMessages[0] = newActive;

        newActive.LifetimeCoroutine = StartCoroutine(LifetimeRoutine(newActive));
    }

    private void ForceRemove(ActiveMessage message)
    {
        if (message.IsRemoved)
        {
            return;
        }

        message.IsRemoved = true;

        if (message.MoveCoroutine != null)
        {
            StopCoroutine(message.MoveCoroutine);
            message.MoveCoroutine = null;
        }

        if (message.LifetimeCoroutine != null)
        {
            StopCoroutine(message.LifetimeCoroutine);
        }

        StartCoroutine(ExitAndDestroy(message.Instance));
    }

    private IEnumerator LifetimeRoutine(ActiveMessage message)
    {
        yield return new WaitForSecondsRealtime(_lifetime);

        if (message.IsRemoved)
        {
            yield break;
        }

        message.IsRemoved = true;

        if (message.MoveCoroutine != null)
        {
            StopCoroutine(message.MoveCoroutine);
            message.MoveCoroutine = null;
        }

        for (int i = 0; i < _activeMessages.Length; i++)
        {
            if (_activeMessages[i] == message)
            {
                _activeMessages[i] = null;
                break;
            }
        }

        yield return ExitAndDestroy(message.Instance);
    }

    private IEnumerator ExitAndDestroy(MessageItemUI instance)
    {
        yield return instance.ExitRoutine(_moveDuration, _exitMoveDistance);
        Destroy(instance.gameObject);
    }
}