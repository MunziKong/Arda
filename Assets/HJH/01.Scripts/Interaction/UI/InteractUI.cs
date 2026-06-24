using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractUI : MonoBehaviour
{
    [Header("Icon Pool")]
    [SerializeField] private InteractIconPool _iconPool;

    [Header("Target UI")]
    [SerializeField] private GameObject _targetRoot;
    [SerializeField] private RectTransform _targetRect;
    [SerializeField] private Slider _holdProgressSlider;

    private readonly Dictionary<IInteractable, InteractIconUI> _activeIcons = new Dictionary<IInteractable, InteractIconUI>();

    private IInteractable _currentTarget;

    private void Awake()
    {
        HideTarget();
    }

    private void LateUpdate()
    {
        UpdateTargetPosition();
    }

    public void ShowIcons(List<IInteractable> targets)
    {
        HashSet<IInteractable> targetSet = new(targets);
        List<IInteractable> removeList = new List<IInteractable>();

        foreach (var pair in _activeIcons)
        {
            if (!targetSet.Contains(pair.Key))
            {
                _iconPool.Release(pair.Value);
                removeList.Add(pair.Key);
            }
        }

        foreach (IInteractable target in removeList)
        {
            _activeIcons.Remove(target);
        }

        foreach (IInteractable target in targets)
        {
            if (_activeIcons.ContainsKey(target))
            {
                continue;
            }

            InteractIconUI icon = _iconPool.Get();
            icon.SetTarget(target);

            _activeIcons.Add(target, icon);
        }
    }

    public void SetTarget(IInteractable target)
    {
        _currentTarget = target;

        if (_currentTarget == null)
        {
            HideTarget();
            return;
        }

        _targetRoot.SetActive(true);


        HideProgress();
        UpdateTargetPosition();
    }

    public void HideTarget()
    {
        _currentTarget = null;

        if (_targetRoot != null)
        {
            _targetRoot.SetActive(false);
        }

        HideProgress();
    }

    public void HideAll()
    {
        foreach (var pair in _activeIcons)
        {
            _iconPool.Release(pair.Value);
        }

        _activeIcons.Clear();
        HideTarget();
    }

    public void SetProgress(float value)
    {
        if (_holdProgressSlider == null)
        {
            return;
        }


        _holdProgressSlider.gameObject.SetActive(true);
        _holdProgressSlider.value = Mathf.Clamp01(value);
    }

    public void HideProgress()
    {
        if (_holdProgressSlider == null)
        {
            return;
        }

        _holdProgressSlider.value = 0f;
        _holdProgressSlider.gameObject.SetActive(false);


    }

    private void UpdateTargetPosition()
    {
        if (_currentTarget == null ||
            _currentTarget.PopupPoint == null ||
            Camera.main == null)
        {
            return;
        }

        Vector3 screenPos =
            Camera.main.WorldToScreenPoint(_currentTarget.PopupPoint.position);

        if (screenPos.z < 0f)
        {
            _targetRoot.SetActive(false);
            return;
        }

        if (!_targetRoot.activeSelf)
        {
            _targetRoot.SetActive(true);
        }

        _targetRect.position = screenPos;
    }
}