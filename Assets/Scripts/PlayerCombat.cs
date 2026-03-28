using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float projectileSpeed = 26f;
    [SerializeField] private float spawnHeight = 0.45f;
    [SerializeField] private float spawnForward = 0.9f;
    [SerializeField] private float baseAttackCooldown = 0.15f;

    private float _nextAttackTime;
    private float _buffEndTime;
    private float _buffedCooldown = 0.05f;

    private InputAction _attackAction;

    public void BuffAttackSpeed(float duration)
    {
        _buffEndTime = Mathf.Max(_buffEndTime, Time.time + duration);
    }

    private void Start()
    {
        var playerInput = GetComponent<PlayerInput>();
        _attackAction = playerInput.actions.FindActionMap("Player").FindAction("Attack", throwIfNotFound: false);
        if (_attackAction == null)
            Debug.LogWarning("PlayerCombat: действие 'Attack' не найдено в PlayerControls. Проверьте ассет и карту Player.");
    }

    private void Update()
    {
        if (_attackAction == null)
            return;

        float currentCooldown = Time.time < _buffEndTime ? _buffedCooldown : baseAttackCooldown;

        if (_attackAction.IsPressed() && Time.time >= _nextAttackTime)
        {
            FireProjectile();
            _nextAttackTime = Time.time + currentCooldown;
        }
    }

    private void FireProjectile()
    {
        Vector3 dir = GetFireDirection();
        Vector3 spawn = transform.position + Vector3.up * spawnHeight + dir * spawnForward;
        Projectile.Create(spawn, dir, attackDamage, projectileSpeed);
    }

    private Vector3 GetFireDirection()
    {
        if (GameplayModeController.Instance != null && GameplayModeController.Instance.IsFirstPerson)
        {
            var cam = Camera.main;
            if (cam != null)
            {
                Vector3 f = cam.transform.forward;
                f.y = 0f;
                if (f.sqrMagnitude > 0.01f)
                    return f.normalized;
            }

            return GameplayModeController.Instance.GetFirstPersonPlanarForward();
        }

        if (MouseGroundUtility.TryGetMouseGroundPoint(out Vector3 ground))
        {
            Vector3 d = ground - transform.position;
            d.y = 0f;
            if (d.sqrMagnitude > 0.01f)
                return d.normalized;
        }

        var vr = transform.Find("VisualRoot");
        if (vr != null)
        {
            Vector3 f = vr.forward;
            f.y = 0f;
            if (f.sqrMagnitude > 0.01f)
                return f.normalized;
        }

        return transform.forward;
    }
}

/// <summary>Точка на плоскости пола под курсором — для прицеливания сверху (общий хелпер для боя и метеора).</summary>
public static class MouseGroundUtility
{
    public static bool TryGetMouseGroundPoint(out Vector3 point, float planeHeight = 0f)
    {
        point = default;
        var cam = Camera.main;
        if (cam == null || Mouse.current == null)
            return false;

        var ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        return TryRayGroundIntersection(ray, out point, planeHeight);
    }

    /// <summary>Top-down: mouse ray. First-person: screen-center ray (where you look).</summary>
    public static bool TryGetAimGroundPoint(out Vector3 point, float planeHeight = 0f)
    {
        point = default;
        var cam = Camera.main;
        if (cam == null)
            return false;

        Ray ray;
        if (GameplayModeController.Instance != null && GameplayModeController.Instance.IsFirstPerson)
            ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        else
        {
            if (Mouse.current == null)
                return false;
            ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        }

        return TryRayGroundIntersection(ray, out point, planeHeight);
    }

    private static bool TryRayGroundIntersection(Ray ray, out Vector3 point, float planeHeight)
    {
        point = default;
        var plane = new Plane(Vector3.up, new Vector3(0f, planeHeight, 0f));
        if (!plane.Raycast(ray, out float enter))
            return false;

        point = ray.GetPoint(enter);
        return true;
    }
}

/// <summary>Видимый снаряд с хвостом: летит по горизонтали, урон при столкновении с врагом или препятствием.</summary>
[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private float damage = 10f;
    [SerializeField] private float maxLifetime = 2.5f;

    private float _spawnTime;

    public void SetDamage(float value) => damage = value;

    public void Launch(Vector3 direction, float speed)
    {
        var rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.01f)
            direction = Vector3.forward;
        direction.Normalize();
        rb.linearVelocity = direction * speed;
        _spawnTime = Time.time;
    }

    public static GameObject Create(Vector3 position, Vector3 direction, float damage, float speed)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.01f)
            direction = Vector3.forward;
        direction.Normalize();

        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "Projectile";
        go.transform.position = position;
        go.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        go.transform.localScale = new Vector3(0.14f, 0.14f, 0.52f);

        var lit = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var mat = new Material(lit);
        mat.name = "M_Projectile_Runtime";
        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", new Color(0.22f, 0.42f, 0.48f));
        else
            mat.color = new Color(0.22f, 0.42f, 0.48f);
        if (mat.HasProperty("_Metallic"))
            mat.SetFloat("_Metallic", 0.35f);
        if (mat.HasProperty("_Smoothness"))
            mat.SetFloat("_Smoothness", 0.62f);
        mat.EnableKeyword("_EMISSION");
        if (mat.HasProperty("_EmissionColor"))
            mat.SetColor("_EmissionColor", new Color(0.1f, 0.28f, 0.34f) * 0.85f);

        go.GetComponent<Renderer>().sharedMaterial = mat;

        var trail = go.AddComponent<TrailRenderer>();
        trail.time = 0.2f;
        trail.startWidth = 0.09f;
        trail.endWidth = 0.02f;
        trail.minVertexDistance = 0.03f;
        trail.material = mat;
        trail.startColor = new Color(0.45f, 0.65f, 0.72f, 0.75f);
        trail.endColor = new Color(0.15f, 0.22f, 0.28f, 0f);

        go.AddComponent<Rigidbody>();
        var proj = go.AddComponent<Projectile>();
        proj.SetDamage(damage);
        proj.Launch(direction, speed);
        return go;
    }

    private void Update()
    {
        if (Time.time - _spawnTime > maxLifetime)
            Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
            return;

        var enemy = collision.collider.GetComponentInParent<Enemy>();
        if (enemy != null)
            enemy.TakeDamage(damage);

        Destroy(gameObject);
    }
}
