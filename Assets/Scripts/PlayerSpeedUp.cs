using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerSpeedUp : MonoBehaviour
{
    [SerializeField] private float durationSeconds = 3f;
    [SerializeField] private float cooldownSeconds = 8f;
    [SerializeField] private float speedMultiplier = 1.6f;

    private float _endTime;
    private float _nextAllowedTime;
    private PlayerMovement _movement;

    public bool IsActive => Time.time < _endTime;
    public float CooldownRemaining => Mathf.Max(0f, _nextAllowedTime - Time.time);
    public float Multiplier => IsActive ? speedMultiplier : 1f;

    private void Awake()
    {
        _movement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        if (Time.time < _nextAllowedTime)
            return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            _endTime = Time.time + durationSeconds;
            _nextAllowedTime = Time.time + cooldownSeconds;
        }
    }
}
