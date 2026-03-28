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

    private Transform _hpBarFill;
    private float _invincibleEndTime;

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

        CreateFloatingHpBar();
    }

    private void CreateFloatingHpBar()
    {
        var barRoot = new GameObject("FloatingHPBar");
        barRoot.transform.SetParent(transform, false);
        barRoot.transform.localPosition = new Vector3(0f, 2.5f, 0f);

        var bg = GameObject.CreatePrimitive(PrimitiveType.Quad);
        bg.name = "Bg";
        UnityEngine.Object.DestroyImmediate(bg.GetComponent<Collider>());
        bg.transform.SetParent(barRoot.transform, false);
        bg.transform.localPosition = Vector3.zero;
        bg.transform.localScale = new Vector3(1.5f, 0.2f, 1f);
        var bgMat = new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color"));
        bgMat.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        bg.GetComponent<Renderer>().sharedMaterial = bgMat;

        var fill = GameObject.CreatePrimitive(PrimitiveType.Quad);
        fill.name = "Fill";
        UnityEngine.Object.DestroyImmediate(fill.GetComponent<Collider>());
        fill.transform.SetParent(barRoot.transform, false);
        fill.transform.localPosition = new Vector3(0f, 0f, -0.01f);
        fill.transform.localScale = new Vector3(1.45f, 0.15f, 1f);
        var fillMat = new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color"));
        fillMat.color = new Color(0.3f, 0.8f, 0.4f, 1f);
        fill.GetComponent<Renderer>().sharedMaterial = fillMat;

        _hpBarFill = fill.transform;
        
        // Ensure bar always faces camera
        barRoot.AddComponent<FaceCamera>();
    }

    private void Update()
    {
        if (_hpBarFill != null)
        {
            float pct = Mathf.Clamp01(_current / maxHealth);
            _hpBarFill.localScale = new Vector3(1.45f * pct, 0.15f, 1f);
            _hpBarFill.localPosition = new Vector3(-1.45f * (1f - pct) * 0.5f, 0f, -0.01f);
        }
    }

    public void HealFull()
    {
        _current = maxHealth;
    }

    public void MakeInvincible(float duration)
    {
        _invincibleEndTime = Mathf.Max(_invincibleEndTime, Time.time + duration);
    }

    public void TakeDamage(float amount)
    {
        if (_current <= 0f || amount <= 0f || Time.time < _invincibleEndTime)
            return;

        _current = Mathf.Max(0f, _current - amount);
        OnDamaged?.Invoke(amount);
        FlashDamage();

        if (_current <= 0f)
        {
            DisablePlayerControls();
            
            // Make sure the event goes out so GameUI can handle Game Over state
            if (OnDeath != null)
                OnDeath.Invoke();
                
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
