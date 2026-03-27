using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Player hit points and death. Attach to the Player object.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;

    private float _current;
    private Renderer _renderer;
    private Color _baseColor;
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    public float CurrentHealth => _current;
    public float MaxHealth => maxHealth;
    public bool IsAlive => _current > 0f;

    public event Action OnDeath;
    public event Action<float> OnDamaged;

    private void Awake()
    {
        _current = maxHealth;
        var hull = transform.Find("VisualRoot/PlayerHull");
        _renderer = hull != null ? hull.GetComponent<Renderer>() : GetComponentInChildren<Renderer>();
        if (_renderer != null && _renderer.material.HasProperty(BaseColorId))
            _baseColor = _renderer.material.GetColor(BaseColorId);
        else if (_renderer != null)
            _baseColor = _renderer.material.color;
    }

    public void TakeDamage(float amount)
    {
        if (_current <= 0f || amount <= 0f)
            return;

        _current = Mathf.Max(0f, _current - amount);
        OnDamaged?.Invoke(amount);
        FlashDamage();

        if (_current <= 0f)
        {
            DisablePlayerControls();
            OnDeath?.Invoke();
            enabled = false;
        }
    }

    private void FlashDamage()
    {
        if (_renderer == null)
            return;
        StopAllCoroutines();
        StartCoroutine(FlashRoutine());
    }

    private System.Collections.IEnumerator FlashRoutine()
    {
        SetTint(Color.red);
        yield return new WaitForSeconds(0.12f);
        SetTint(_baseColor);
    }

    private void SetTint(Color c)
    {
        if (_renderer == null)
            return;
        if (_renderer.material.HasProperty(BaseColorId))
            _renderer.material.SetColor(BaseColorId, c);
        else
            _renderer.material.color = c;
    }

    private void DisablePlayerControls()
    {
        var movement = GetComponent<PlayerMovement>();
        if (movement != null)
            movement.enabled = false;
        var combat = GetComponent<PlayerCombat>();
        if (combat != null)
            combat.enabled = false;
        var meteor = GetComponent<MeteorStrike>();
        if (meteor != null)
            meteor.enabled = false;
        var input = GetComponent<PlayerInput>();
        if (input != null)
            input.enabled = false;

        var rb = GetComponent<Rigidbody>();
        if (rb != null)
            rb.linearVelocity = Vector3.zero;
    }
}
