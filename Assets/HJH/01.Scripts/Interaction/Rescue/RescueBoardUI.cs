using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RescueBoardUI : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private PlayerInputs _inputs;

    [Header("Board")]
    [SerializeField] private RectTransform _board;
    [SerializeField] private Transform _slotRoot;
    [SerializeField] private RescueBoardSlotUI _slotPrefab;

    [Header("Position")]
    [SerializeField] private float _openedPosX = 0f;
    [SerializeField] private float _closedPosX = 300f;

    [Header("Animation")]
    [SerializeField] private float _moveDuration = 0.2f;

    [SerializeField] private RescueManager _rescueManager;

    private readonly List<RescueBoardSlotUI> _slots = new();
    private Coroutine _moveRoutine;

    private void OnEnable()
    {
        _inputs.OpenBoardEvent += OpenBoard;

        if (_rescueManager != null)
        {
            _rescueManager.OnRescuedMonstersChanged += RefreshBoard;
            RefreshBoard();
        }
    }

    private void OnDisable()
    {
        _inputs.OpenBoardEvent -= OpenBoard;

        if (_rescueManager != null)
        {
            _rescueManager.OnRescuedMonstersChanged -= RefreshBoard;
        }
    }

    private void OpenBoard(bool isPressed)
    {
        float targetX = isPressed ? _openedPosX : _closedPosX;

        if (_moveRoutine != null)
        {
            StopCoroutine(_moveRoutine);
        }

        _moveRoutine = StartCoroutine(MoveBoardRoutine(targetX));
    }

    private IEnumerator MoveBoardRoutine(float targetX)
    {
        Vector2 startPos = _board.anchoredPosition;
        Vector2 targetPos = new(targetX, startPos.y);

        float elapsed = 0f;

        while (elapsed < _moveDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float timer = Mathf.Clamp01(elapsed / _moveDuration);
            timer = Mathf.SmoothStep(0f, 1f, timer);

            _board.anchoredPosition = Vector2.Lerp(startPos, targetPos, timer);
            yield return null;
        }

        _board.anchoredPosition = targetPos;
        _moveRoutine = null;
    }

    private void RefreshBoard()
    {
        if (_rescueManager == null)
        {
            return;
        }

        List<RescuedMonsterEntry> rescuedMonsters = _rescueManager.RescuedMonsters;

        EnsureSlotCount(rescuedMonsters.Count);

        for (int i = 0; i < _slots.Count; i++)
        {
            if (i < rescuedMonsters.Count)
            {
                _slots[i].gameObject.SetActive(true);
                _slots[i].Bind(rescuedMonsters[i]);
            }
            else
            {
                _slots[i].Clear();
                _slots[i].gameObject.SetActive(false);
            }
        }
    }

    private void EnsureSlotCount(int targetCount)
    {
        while (_slots.Count < targetCount)
        {
            RescueBoardSlotUI slot = Instantiate(_slotPrefab, _slotRoot);
            _slots.Add(slot);
        }
    }
}