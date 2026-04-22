using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Enemy : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;

    [Header("Health")]
    [SerializeField] private float maxHealth = 40f;

    [Header("Score")]
    [SerializeField] private int pointValue = 10;

    [Header("Contact damage")]
    [SerializeField] private float contactDamage = 8f;
    [SerializeField] private float contactHitCooldown = 0.9f;

    private Rigidbody _rigidbody;
    private Transform _player;
    private float _health;
    private Renderer _renderer;
    private Color _baseColor;
    private float _nextContactDamageTime;
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    
    private float _slowMultiplier = 1f;
    private float _slowEndTime;

    public event System.Action<int> OnDied;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _health = maxHealth;
        _renderer = FindPrimaryBodyRenderer();
        if (_renderer != null && _renderer.material.HasProperty(BaseColorId))
            _baseColor = _renderer.material.GetColor(BaseColorId);
        else if (_renderer != null)
            _baseColor = _renderer.material.color;
    }

    private Renderer FindPrimaryBodyRenderer()
    {
        var vr = transform.Find("VisualRoot");
        if (vr == null)
            return GetComponentInChildren<Renderer>();

        string[] bodyNames = { "StalkerBody", "BruteTorso", "SwarmCore" };
        foreach (var n in bodyNames)
        {
            var t = vr.Find(n);
            if (t != null)
            {
                var r = t.GetComponent<Renderer>();
                if (r != null)
                    return r;
            }
        }

        foreach (Transform c in vr)
        {
            if (c.name.StartsWith("Detail_") || c.name == "Detail_Meta")
                continue;
            var r = c.GetComponent<Renderer>();
            if (r != null)
                return r;
        }

        return GetComponentInChildren<Renderer>();
    }

    private void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            _player = p.transform;
    }

    private void FixedUpdate()
    {
        if (_player == null)
            return;

        if (Time.time > _slowEndTime)
            _slowMultiplier = 1f;

        Vector3 dir = _player.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f)
            return;

        dir.Normalize();
        Vector3 v = _rigidbody.linearVelocity;
        float currentSpeed = moveSpeed * _slowMultiplier;
        _rigidbody.linearVelocity = new Vector3(dir.x * currentSpeed, v.y, dir.z * currentSpeed);
    }

    public void ApplySlow(float multiplier, float duration)
    {
        _slowMultiplier = multiplier;
        _slowEndTime = Time.time + duration;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.CompareTag("Player"))
            return;
        if (Time.time < _nextContactDamageTime)
            return;

        var health = collision.collider.GetComponentInParent<PlayerHealth>();
        if (health == null || !health.IsAlive)
            return;

        health.TakeDamage(contactDamage);
        _nextContactDamageTime = Time.time + contactHitCooldown;
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0f)
            return;

        _health -= amount;
        HitFlash();
        if (_health <= 0f)
        {
            OnDied?.Invoke(pointValue);
            ScoreTracker.Instance.RegisterKill(pointValue);
            Destroy(gameObject);
        }
    }

    private void HitFlash()
    {
        if (_renderer == null)
            return;
        StopAllCoroutines();
        StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        SetTint(Color.white);
        yield return new WaitForSeconds(0.08f);
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
}
