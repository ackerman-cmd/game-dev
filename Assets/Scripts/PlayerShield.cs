using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerShield : MonoBehaviour
{
    [SerializeField] private float durationSeconds = 2.2f;
    [SerializeField] private float cooldownSeconds = 9f;
    [SerializeField] [Range(0f, 1f)] private float damageTakenMultiplierWhileActive = 0.45f;

    private InputAction _shieldAction;
    private float _shieldEndTime;
    private float _nextAllowedTime;

    public bool IsShieldActive => Time.time < _shieldEndTime;
    public float DamageMultiplier => IsShieldActive ? damageTakenMultiplierWhileActive : 1f;
    public float CooldownRemaining => Mathf.Max(0f, _nextAllowedTime - Time.time);

    private void Awake()
    {
        _shieldAction = GetComponent<PlayerInput>().actions.FindActionMap("Player").FindAction("Shield", throwIfNotFound: false);
    }

    private void Update()
    {
        if (Time.time < _nextAllowedTime)
            return;
            
        bool triggered = false;
        if (_shieldAction != null && _shieldAction.WasPressedThisFrame()) triggered = true;
        if (_shieldAction == null && Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame) triggered = true;

        if (triggered)
        {
            _shieldEndTime = Time.time + durationSeconds;
            _nextAllowedTime = Time.time + cooldownSeconds;
        }
    }
}
