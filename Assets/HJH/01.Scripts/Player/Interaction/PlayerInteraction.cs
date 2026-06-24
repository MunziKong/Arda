using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EPOOutline;
public class PlayerInteraction : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float _detectRadius = 3f;
    [SerializeField] private float _maxDetectDistance = 3f;
    [SerializeField] private float _frontDotThreshold = 0.5f;
    [SerializeField] private LayerMask _layerMask;

    [Header("UI")]
    [SerializeField] private InteractUI _interactUI;

    private PlayerInputs _inputs;
    private IInteractable _currentObj;
    private Coroutine _holdRoutine;

    private readonly List<IInteractable> _visibleTargets = new();

    private void Awake()
    {
        _inputs = GetComponent<PlayerInputs>();
    }

    private void OnEnable()
    {
        _inputs.InteractPressedEvent += OnInteractPressed;
        _inputs.InteractReleasedEvent += OnInteractReleased;
    }

    private void OnDisable()
    {
        ClearCurrentObj();
        _inputs.InteractPressedEvent -= OnInteractPressed;
        _inputs.InteractReleasedEvent -= OnInteractReleased;
    }

    private void Update()
    {
        DetectInteractableObj();
    }

    private void DetectInteractableObj()
    {
        Vector3 origin = transform.position + transform.up;

        Collider[] hits = Physics.OverlapSphere(
            origin,
            _detectRadius,
            _layerMask,
            QueryTriggerInteraction.Ignore
        );

        _visibleTargets.Clear();

        IInteractable nearestObj = null;
        float nearestDistance = float.MaxValue;

        foreach (Collider hit in hits)
        {
            IInteractable interactable =
                hit.GetComponentInParent<IInteractable>();

            if (interactable == null || !interactable.IsEnabled)
            {
                continue;
            }

            if (!_visibleTargets.Contains(interactable) &&
                IsVisibleInCamera(interactable))
            {
                _visibleTargets.Add(interactable);
            }

            Vector3 targetPoint = hit.bounds.center;
            float distance = Vector3.Distance(origin, targetPoint);

            if (distance > _maxDetectDistance)
            {
                continue;
            }

            Vector3 direction = (targetPoint - origin).normalized;
            float dot = Vector3.Dot(transform.forward, direction);

            if (dot < _frontDotThreshold)
            {
                continue;
            }

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestObj = interactable;
            }
        }

        _interactUI.ShowIcons(_visibleTargets);

        if (nearestObj == null)
        {
            ClearCurrentObj();
            return;
        }

        if (!ReferenceEquals(nearestObj, _currentObj))
        {
            SetCurrentObj(nearestObj);
        }
    }

    private bool IsVisibleInCamera(IInteractable interactable)
    {
        if (interactable.PopupPoint == null || Camera.main == null)
        {
            return false;
        }

        Vector3 viewportPos =
            Camera.main.WorldToViewportPoint(interactable.PopupPoint.position);

        return viewportPos.z > 0f &&
               viewportPos.x >= 0f &&
               viewportPos.x <= 1f &&
               viewportPos.y >= 0f &&
               viewportPos.y <= 1f;
    }

    private void SetCurrentObj(IInteractable interactable)
    {
        ClearCurrentObj();
        _currentObj = interactable;
        _interactUI.SetTarget(_currentObj);
        _currentObj.SetOutline(true);
    }

    private void ClearCurrentObj()
    {
        if (_currentObj == null) return;

        _currentObj.SetOutline(false);
        StopHoldRoutine();
        _currentObj = null;
        _interactUI.HideTarget();
    }

    private void OnInteractPressed()
    {
        if (_currentObj == null)
        {
            return;
        }

        if (_currentObj.RequireHold)
        {
            StopHoldRoutine();
            _holdRoutine = StartCoroutine(HoldInteractRoutine(_currentObj));
        }
        else
        {
            _currentObj.Interact();
        }
    }

    private void OnInteractReleased()
    {
        StopHoldRoutine();
        _interactUI.HideProgress();
    }

    private IEnumerator HoldInteractRoutine(IInteractable target)
    {
        float timer = 0f;
        float holdTime = Mathf.Max(target.HoldTime, 0.01f);

        while (timer < holdTime)
        {
            if (!ReferenceEquals(target, _currentObj))
            {
                yield break;
            }

            timer += Time.deltaTime;

            float progress = timer / holdTime;
            _interactUI.SetProgress(progress);

            yield return null;
        }

        _holdRoutine = null;
        _interactUI.HideProgress();

        if (ReferenceEquals(target, _currentObj))
        {
            target.Interact();
        }
    }

    private void StopHoldRoutine()
    {
        if (_holdRoutine == null)
        {
            return;
        }

        StopCoroutine(_holdRoutine);
        _holdRoutine = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            transform.position + transform.up,
            _detectRadius
        );
    }
}