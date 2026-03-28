using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Top-down vs experimental first-person view. Keeps one active MainCamera and swaps cursor lock for FP.</summary>
public class GameplayModeController : MonoBehaviour
{
    public static GameplayModeController Instance { get; private set; }

    [SerializeField] private Camera topDownCamera;
    [SerializeField] private Camera firstPersonCamera;
    [SerializeField] private Transform firstPersonLookPivot;
    [SerializeField] private bool startInFirstPerson;

    public bool IsFirstPerson { get; private set; }
    public Transform FirstPersonLookPivot => firstPersonLookPivot;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("GameplayModeController: multiple instances; keeping first.");
            enabled = false;
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Start()
    {
        ApplyViewMode(startInFirstPerson);
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current[Key.V].wasPressedThisFrame)
            ToggleViewMode();

        if (IsFirstPerson && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (IsFirstPerson && Keyboard.current != null && Keyboard.current.leftAltKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void ToggleViewMode() => ApplyViewMode(!IsFirstPerson);

    public void SetTopDown() => ApplyViewMode(false);

    public void SetFirstPerson() => ApplyViewMode(true);

    private void ApplyViewMode(bool firstPerson)
    {
        IsFirstPerson = firstPerson;

        if (topDownCamera != null)
        {
            topDownCamera.gameObject.SetActive(!firstPerson);
            SetMainCameraTag(topDownCamera, !firstPerson);
        }

        if (firstPersonCamera != null)
        {
            firstPersonCamera.gameObject.SetActive(firstPerson);
            SetMainCameraTag(firstPersonCamera, firstPerson);
        }

        var alMain = topDownCamera != null ? topDownCamera.GetComponent<AudioListener>() : null;
        var alFps = firstPersonCamera != null ? firstPersonCamera.GetComponent<AudioListener>() : null;
        if (alMain != null)
            alMain.enabled = !firstPerson;
        if (alFps != null)
            alFps.enabled = firstPerson;

        if (firstPerson)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private static void SetMainCameraTag(Camera cam, bool isMain)
    {
        if (cam == null)
            return;
        cam.tag = isMain ? "MainCamera" : "Untagged";
    }

    /// <summary>World-space planar move direction from WASD relative to first-person yaw (no strafe drift from pitch).</summary>
    public Vector3 GetFirstPersonPlanarMove(Vector2 moveInput)
    {
        if (firstPersonLookPivot == null)
            return Vector3.zero;

        Vector3 f = firstPersonLookPivot.forward;
        f.y = 0f;
        if (f.sqrMagnitude < 1e-6f)
            f = Vector3.forward;
        else
            f.Normalize();

        Vector3 r = firstPersonLookPivot.right;
        r.y = 0f;
        if (r.sqrMagnitude < 1e-6f)
            r = Vector3.right;
        else
            r.Normalize();

        Vector3 world = f * moveInput.y + r * moveInput.x;
        if (world.sqrMagnitude > 1f)
            world.Normalize();
        return world;
    }

    public Vector3 GetFirstPersonPlanarForward()
    {
        if (firstPersonLookPivot == null)
            return transform.forward;

        Vector3 f = firstPersonLookPivot.forward;
        f.y = 0f;
        return f.sqrMagnitude > 1e-6f ? f.normalized : Vector3.forward;
    }
}
