using UnityEngine;
using Unity.Cinemachine;

public class PlayerCameraController : MonoBehaviour
{
    [Header("Cinemachine")]
    [SerializeField] private Transform _cinemachineCameraTarget;

    [Header("Camera Clamp")]
    [SerializeField] private float _topClamp = 70f;
    [SerializeField] private float _bottomClamp = -30f;

    [Header("Camera Settings")]
    [SerializeField] private float _cameraAngleOverride = 0f;
    [SerializeField] private bool _lockCameraPosition = false;

    [Header("Mouse")]
    [SerializeField] private float _mouseSensitivity = 0.1f;

    private PlayerInputs _input;
    private PlayerMove _move;

    private float _cinemachineTargetPitch;

    private const float _threshold = 0.01f;

    private void Awake()
    {
        _input = GetComponent<PlayerInputs>();
        _move = GetComponent<PlayerMove>();
    }

    private void Start()
    {
        _cinemachineTargetPitch = _cinemachineCameraTarget.localRotation.eulerAngles.x;

    }

    private void LateUpdate()
    {
        CameraRotation();
        _move.UpdateTurnAnim();
    }

    public void SetCameraControl(bool value)
    {
        _lockCameraPosition = !value;
    }


    private void CameraRotation()
    {
        if (_input.Look.sqrMagnitude < _threshold || _lockCameraPosition)
        {
            ApplyCameraTargetRotation();
            return;
        }

        transform.Rotate(Vector3.up, _input.Look.x * _mouseSensitivity, Space.World);

        _cinemachineTargetPitch -= _input.Look.y * _mouseSensitivity;

        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, _bottomClamp, _topClamp);

        ApplyCameraTargetRotation();
    }

    private void ApplyCameraTargetRotation()
    {
        _cinemachineCameraTarget.localRotation =
            Quaternion.Euler(
                _cinemachineTargetPitch + _cameraAngleOverride,
                0f,
                0f
            );
    }

    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f)
        {
            angle += 360f;
        }

        if (angle > 360f)
        {
            angle -= 360f;
        }

        return Mathf.Clamp(angle, min, max);
    }
}