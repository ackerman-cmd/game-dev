using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Short burst of speed in the current move direction. Encages repositioning between melee and meteor.
/// </summary>
[RequireComponent(typeof(Rigidbody), typeof(PlayerInput))]
public class PlayerDash : MonoBehaviour
{
    [SerializeField] private float dashSpeed = 22f;
    [SerializeField] private float dashDuration = 0.14f;
    [SerializeField] private float cooldownSeconds = 2.4f;

    private Rigidbody _rigidbody;
    private InputAction _moveAction;
    private InputAction _dashAction;

    private float _nextDashTime;
    private float _dashEndTime;
    private Vector3 _dashPlanar = Vector3.zero;

    public bool IsDashing => Time.time < _dashEndTime;
    public float CooldownRemaining => Mathf.Max(0f, _nextDashTime - Time.time);

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        var map = GetComponent<PlayerInput>().actions.FindActionMap("Player");
        _moveAction = map.FindAction("Move");
        _dashAction = map.FindAction("Dash", throwIfNotFound: false);
    }

    private void Update()
    {
        if (Time.time < _nextDashTime)
            return;
        if (Time.time < _dashEndTime)
            return;
            
        bool triggered = false;
        if (_dashAction != null && _dashAction.WasPressedThisFrame()) triggered = true;
        if (_dashAction == null && Keyboard.current != null && Keyboard.current.leftShiftKey.wasPressedThisFrame) triggered = true;

        if (!triggered)
            return;

        Vector2 input = _moveAction != null ? _moveAction.ReadValue<Vector2>() : Vector2.zero;
        Vector3 dir;
        if (GameplayModeController.Instance != null && GameplayModeController.Instance.IsFirstPerson)
        {
            dir = GameplayModeController.Instance.GetFirstPersonPlanarMove(input);
            if (dir.sqrMagnitude < 0.01f)
                dir = GameplayModeController.Instance.GetFirstPersonPlanarForward();
            else
                dir.Normalize();
        }
        else
        {
            dir = new Vector3(input.x, 0f, input.y);
            if (dir.sqrMagnitude < 0.01f)
            {
                var forward = transform.forward;
                forward.y = 0f;
                if (forward.sqrMagnitude < 0.01f)
                    forward = Vector3.forward;
                dir = forward.normalized;
            }
            else
                dir.Normalize();
        }

        _dashPlanar = dir * dashSpeed;
        _dashEndTime = Time.time + dashDuration;
        _nextDashTime = Time.time + cooldownSeconds;
    }

    private void FixedUpdate()
    {
        if (!IsDashing)
            return;

        Vector3 v = _rigidbody.linearVelocity;
        _rigidbody.linearVelocity = new Vector3(_dashPlanar.x, v.y, _dashPlanar.z);
    }
}
