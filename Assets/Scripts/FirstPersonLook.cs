using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonLook : MonoBehaviour
{
    [SerializeField] private Transform yawPivot;
    [SerializeField] private Transform pitchCamera;
    [SerializeField] private float mouseSensitivity = 0.18f;
    [SerializeField] private float minPitch = -88f;
    [SerializeField] private float maxPitch = 88f;

    private float _pitchAngle;

    private void LateUpdate()
    {
        if (GameplayModeController.Instance == null || !GameplayModeController.Instance.IsFirstPerson)
            return;
        if (yawPivot == null || pitchCamera == null)
            return;
        if (Cursor.lockState != CursorLockMode.Locked || Mouse.current == null)
            return;

        var delta = Mouse.current.delta.ReadValue();
        float yaw = delta.x * mouseSensitivity;
        float pitch = -delta.y * mouseSensitivity;

        yawPivot.Rotate(0f, yaw, 0f, Space.Self);

        _pitchAngle = Mathf.Clamp(_pitchAngle + pitch, minPitch, maxPitch);
        pitchCamera.localRotation = Quaternion.Euler(_pitchAngle, 0f, 0f);
    }
}
