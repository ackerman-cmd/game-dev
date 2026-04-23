using UnityEngine;

[RequireComponent(typeof(Enemy))]
public class EnemyRanged : MonoBehaviour
{
    [SerializeField] private float attackInterval  = 2.8f;
    [SerializeField] private float projectileDamage = 6f;
    [SerializeField] private float projectileSpeed  = 11f;
    [SerializeField] private float maxAttackRange   = 16f;
    [SerializeField] private float minAttackRange   = 3.5f;

    private Transform _player;
    private float _nextAttackTime;

    private void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) _player = p.transform;
        _nextAttackTime = Time.time + Random.Range(0.8f, attackInterval);
    }

    private void Update()
    {
        if (_player == null || Time.time < _nextAttackTime) return;

        float dist = Vector3.Distance(transform.position, _player.position);
        if (dist > maxAttackRange || dist < minAttackRange) return;

        Fire();
        _nextAttackTime = Time.time + attackInterval;
    }

    private void Fire()
    {
        Vector3 dir = _player.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) return;
        dir.Normalize();

        Vector3 spawn = transform.position + Vector3.up * 0.5f + dir * 0.7f;
        EnemyProjectile.Create(spawn, dir, projectileDamage, projectileSpeed);
    }
}

public class EnemyProjectile : MonoBehaviour
{
    private float _damage;
    private float _spawnTime;
    private const float MaxLifetime = 4f;

    public static void Create(Vector3 position, Vector3 direction, float damage, float speed)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.01f) direction = Vector3.forward;
        direction.Normalize();

        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "EnemyProjectile";
        go.tag  = "Enemy";
        go.transform.position   = position;
        go.transform.localScale = Vector3.one * 0.24f;

        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var mat    = new Material(shader) { name = "M_EnemyProjectile_Runtime" };
        if (mat.HasProperty("_BaseColor"))     mat.SetColor("_BaseColor",    new Color(0.75f, 0.08f, 0.04f));
        if (mat.HasProperty("_Metallic"))      mat.SetFloat("_Metallic",     0.2f);
        if (mat.HasProperty("_Smoothness"))    mat.SetFloat("_Smoothness",   0.5f);
        mat.EnableKeyword("_EMISSION");
        if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", new Color(1f, 0.18f, 0.05f) * 2f);
        go.GetComponent<Renderer>().sharedMaterial = mat;

        var trail               = go.AddComponent<TrailRenderer>();
        trail.time              = 0.16f;
        trail.startWidth        = 0.13f;
        trail.endWidth          = 0.02f;
        trail.minVertexDistance = 0.04f;
        trail.material          = mat;
        trail.startColor        = new Color(1f, 0.25f, 0.08f, 0.85f);
        trail.endColor          = new Color(0.5f, 0.08f, 0f, 0f);

        var light       = go.AddComponent<Light>();
        light.type      = LightType.Point;
        light.color     = new Color(1f, 0.3f, 0.05f);
        light.range     = 5f;
        light.intensity = 4f;

        var rb                    = go.AddComponent<Rigidbody>();
        rb.useGravity             = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.constraints            = RigidbodyConstraints.FreezeRotation;
        rb.linearVelocity         = direction * speed;

        var proj        = go.AddComponent<EnemyProjectile>();
        proj._damage    = damage;
        proj._spawnTime = Time.time;
    }

    private void Update()
    {
        if (Time.time - _spawnTime > MaxLifetime)
            Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Enemy")) return;

        var health = collision.collider.GetComponentInParent<PlayerHealth>();
        if (health != null && health.IsAlive)
            health.TakeDamage(_damage);

        Destroy(gameObject);
    }
}
