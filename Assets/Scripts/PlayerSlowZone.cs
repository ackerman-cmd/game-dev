using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerSlowZone : MonoBehaviour
{
    [SerializeField] private float durationSeconds = 5f;
    [SerializeField] private float cooldownSeconds = 12f;
    [SerializeField] private float zoneRadius = 6f;
    [SerializeField] private float slowMultiplier = 0.4f;
    [SerializeField] private GameObject zonePrefab;

    private float _nextAllowedTime;

    public float CooldownRemaining => Mathf.Max(0f, _nextAllowedTime - Time.time);

    private void Update()
    {
        if (Keyboard.current == null) return;

        if (Time.time < _nextAllowedTime)
            return;

        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            SpawnSlowZone();
            _nextAllowedTime = Time.time + cooldownSeconds;
        }
    }

    private void SpawnSlowZone()
    {
        var zone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        zone.name = "SlowZone";
        zone.transform.position = transform.position + Vector3.up * 0.05f;
        zone.transform.localScale = new Vector3(zoneRadius * 2, 0.05f, zoneRadius * 2);
        
        Destroy(zone.GetComponent<Collider>()); // No physical collision

        var renderer = zone.GetComponent<Renderer>();
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        mat.SetColor("_BaseColor", new Color(0.2f, 0.6f, 0.8f, 0.4f));
        
        // Setup transparency
        mat.SetFloat("_Surface", 1);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;

        renderer.sharedMaterial = mat;

        var comp = zone.AddComponent<SlowZoneEffect>();
        comp.duration = durationSeconds;
        comp.radius = zoneRadius;
        comp.slowMultiplier = slowMultiplier;
    }
}

public class SlowZoneEffect : MonoBehaviour
{
    public float duration;
    public float radius;
    public float slowMultiplier;
    private float _spawnTime;

    private void Start()
    {
        _spawnTime = Time.time;
    }

    private void Update()
    {
        if (Time.time - _spawnTime > duration)
        {
            Destroy(gameObject);
            return;
        }

        // Apply slow to enemies in radius
        float sqrRadius = radius * radius;
        foreach (var enemy in FindObjectsByType<Enemy>(FindObjectsSortMode.None))
        {
            if ((enemy.transform.position - transform.position).sqrMagnitude <= sqrRadius)
            {
                enemy.ApplySlow(slowMultiplier, 0.2f); // Apply slow for 0.2s, refreshed every frame
            }
        }
    }
}
