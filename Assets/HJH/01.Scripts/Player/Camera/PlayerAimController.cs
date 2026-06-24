using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class PlayerAimController : MonoBehaviour
{
    [Header("Crosshair")]
    [SerializeField] private GameObject _crosshair;
    [SerializeField] private Image _crosshairImage;
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _targetColor = Color.red;

    [Header("Area Preview")]
    [SerializeField] private GameObject _areaPreviewPrefab;
    [SerializeField] private float _areaPreviewYOffset = 0.02f;
    [SerializeField] private LayerMask _areaGroundLayerMask;

    [Header("Aim")]
    [SerializeField] private LayerMask _aimLayerMask;

    [Header("Cinemachine")]
    [SerializeField] private CinemachineCamera _virtualCamera;

    [Header("Shoulder Offset")]
    [SerializeField] private Vector3 _defaultFollowOffset;
    [SerializeField] private Vector3 _aimFollowOffset;

    [Header("Camera Offset Smooth")]
    [SerializeField] private float _offsetBlendTime = 0.2f;

    private GameObject _currentAreaPreviewObject;

    private Coroutine _offsetBlendRoutine;

    private SkillDefinition _currentAimSkill;
    private bool _isAimMode;
    private float _baseAimDistance;
    private bool _isBaseAimMode;

    public Vector3 AimPoint { get; private set; }
    public GameObject AimTarget { get; private set; }

    private void Start()
    {
        ExitAimMode();
    }

    private void Update()
    {
        if (!_isAimMode)
        {
            return;
        }

        UpdateAimPoint();
        UpdateAreaPreviewPosition();
        UpdateCrosshairColor();
    }

    public void EnterAimMode(SkillDefinition skill)
    {
        _isBaseAimMode = false;
        _baseAimDistance = 0f;

        _currentAimSkill = skill;
        _isAimMode = true;


        if (_crosshair != null)
        {
            _crosshair.SetActive(false);
        }

        ClearAreaPreview();

        SetCameraOffset(_aimFollowOffset);

        if (skill.aimMode == AimMode.Crosshair)
        {
            if (_crosshair != null)
            {
                _crosshair.SetActive(true);
            }
        }
        else if (skill.aimMode == AimMode.Area)
        {
            SetupAreaPreview(skill);
        }

        UpdateAimPoint();
        UpdateAreaPreviewPosition();
    }

    public void ExitAimMode()
    {
        _isAimMode = false;
        _currentAimSkill = null;
        _isBaseAimMode = false;

        if (_crosshair != null)
        {
            _crosshair.SetActive(false);
        }

        ClearAreaPreview();

        SetCameraOffset(_defaultFollowOffset);

        AimTarget = null;
    }

    private void UpdateAimPoint()
    {
        if (_currentAimSkill != null &&
            _currentAimSkill.aimMode == AimMode.Area)
        {
            UpdateAreaAimPoint();
            return;
        }

        UpdateCrosshairAimPoint();
    }

    private void UpdateAreaAimPoint()
    {
        float aimDistance = GetCurrentAimDistance();

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(
                ray,
                out RaycastHit hit,
                aimDistance,
                _areaGroundLayerMask))
        {
            AimPoint = hit.point;

            ClampAreaAimPoint(aimDistance);

            if (_currentAreaPreviewObject != null &&
                !_currentAreaPreviewObject.activeSelf)
            {
                _currentAreaPreviewObject.SetActive(true);
            }
        }
        else
        {
            if (_currentAreaPreviewObject != null &&
                _currentAreaPreviewObject.activeSelf)
            {
                _currentAreaPreviewObject.SetActive(false);
            }
        }
    }

    private void UpdateCrosshairAimPoint()
    {
        AimTarget = null;

        float aimDistance = GetCurrentAimDistance();

        Ray ray = Camera.main.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        if (Physics.Raycast(
                ray,
                out RaycastHit hit,
                aimDistance,
                _aimLayerMask))
        {
            AimPoint = hit.point;

            IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();

            if (damageable != null)
            {
                AimTarget = ((Component)damageable).gameObject;
            }
        }
        else
        {
            AimPoint = ray.origin + ray.direction * aimDistance;
        }
    }

    private void ClampAreaAimPoint(float maxDistance)
    {
        Vector3 origin = transform.position;

        Vector3 direction = AimPoint - origin;
        direction.y = 0f;

        float distance = direction.magnitude;

        if (distance <= maxDistance)
        {
            return;
        }

        Vector3 clampedPosition =
            origin + direction.normalized * maxDistance;

        AimPoint = new Vector3(
            clampedPosition.x,
            AimPoint.y,
            clampedPosition.z
        );
    }

    private void SetupAreaPreview(SkillDefinition skill)
    {
        if (_areaPreviewPrefab == null)
        {
            Debug.LogWarning("Area Preview Prefab이 연결되지 않았습니다.");
            return;
        }

        _currentAreaPreviewObject = Instantiate(_areaPreviewPrefab);

        float radius = Mathf.Max(skill.aimRadius, 0.1f);

        DecalProjector decal =
            _currentAreaPreviewObject.GetComponent<DecalProjector>();

        if (decal != null)
        {
            decal.size = new Vector3(
            radius * 2f,
            radius * 2f,
            decal.size.z
        );
        }
        else
        {
            Vector3 defaultScale = _currentAreaPreviewObject.transform.localScale;

            _currentAreaPreviewObject.transform.localScale = new Vector3(
                defaultScale.x * radius * 2f,
                defaultScale.y,
                defaultScale.z * radius * 2f
            );
        }
    }

    private void UpdateAreaPreviewPosition()
    {
        if (_currentAimSkill == null ||
            _currentAimSkill.aimMode != AimMode.Area ||
            _currentAreaPreviewObject == null)
        {
            return;
        }

        _currentAreaPreviewObject.transform.position =
            AimPoint + Vector3.up * _areaPreviewYOffset;
    }

    private void ClearAreaPreview()
    {
        if (_currentAreaPreviewObject == null)
        {
            return;
        }

        Destroy(_currentAreaPreviewObject);
        _currentAreaPreviewObject = null;
    }

    private void SetCameraOffset(Vector3 targetOffset)
    {
        if (_offsetBlendRoutine != null)
        {
            StopCoroutine(_offsetBlendRoutine);
        }

        _offsetBlendRoutine = StartCoroutine(BlendCameraOffsetRoutine(targetOffset));
    }

    private IEnumerator BlendCameraOffsetRoutine(Vector3 targetOffset)
    {
        if (_virtualCamera == null)
        {
            yield break;
        }

        CinemachineThirdPersonFollow follow = _virtualCamera.GetComponent<CinemachineThirdPersonFollow>();

        if (follow == null)
        {
            yield break;
        }

        Vector3 startOffset = follow.ShoulderOffset;
        float timer = 0f;

        while (timer < _offsetBlendTime)
        {
            timer += Time.deltaTime;

            float t = timer / _offsetBlendTime;
            t = Mathf.SmoothStep(0f, 1f, t);

            follow.ShoulderOffset =
                Vector3.Lerp(startOffset, targetOffset, t);

            yield return null;
        }

        follow.ShoulderOffset = targetOffset;
        _offsetBlendRoutine = null;
    }

    private float GetCurrentAimDistance()
    {
        if (_currentAimSkill != null)
        {
            return Mathf.Max(_currentAimSkill.aimDistance, 0.1f);
        }

        if (_isBaseAimMode)
        {
            return Mathf.Max(_baseAimDistance, 0.1f);
        }

        return 10f;
    }

    private void UpdateCrosshairColor()
    {
        if (_crosshairImage == null)
        {
            return;
        }

        _crosshairImage.color = AimTarget != null ? _targetColor : _normalColor;
    }

    public void EnterBaseAimMode(float aimDistance)
    {
        _currentAimSkill = null;
        _baseAimDistance = aimDistance;
        _isBaseAimMode = true;
        _isAimMode = true;

        ClearAreaPreview();

        SetCameraOffset(_aimFollowOffset);

        if (_crosshair != null)
        {
            _crosshair.SetActive(true);
        }

        UpdateAimPoint();
    }
}