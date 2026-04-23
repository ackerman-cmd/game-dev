using System.Collections;
using UnityEngine;

public class AmbientMeteorShower : MonoBehaviour
{
    [SerializeField] private float arenaRadius    = 27f;
    [SerializeField] private float warningSeconds = 2.6f;
    [SerializeField] private float meteorDamage   = 18f;
    [SerializeField] private float meteorRadius   = 2.8f;
    [SerializeField] private float spawnHeight    = 20f;
    [SerializeField] private float baseInterval   = 14f;

    private EnemySpawner _spawner;
    private bool _showerStarted;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCreate()
    {
        if (Object.FindFirstObjectByType<AmbientMeteorShower>() == null)
            new GameObject("[AmbientMeteorShower]").AddComponent<AmbientMeteorShower>();
    }

    private void Start()
    {
        _spawner = Object.FindFirstObjectByType<EnemySpawner>();
    }

    private void Update()
    {
        if (_showerStarted) return;
        int wave = _spawner != null ? _spawner.CurrentWave : 0;
        if (wave >= 2)
        {
            _showerStarted = true;
            StartCoroutine(ShowerLoop());
        }
    }

    private IEnumerator ShowerLoop()
    {
        while (true)
        {
            int wave    = _spawner != null ? _spawner.CurrentWave : 2;
            float delay = Mathf.Max(5f, baseInterval - wave * 0.5f);
            yield return new WaitForSeconds(delay);

            int count = Mathf.Min(1 + wave / 5, 3);
            for (int i = 0; i < count; i++)
            {
                Vector2 r   = Random.insideUnitCircle * arenaRadius;
                Vector3 pos = new Vector3(r.x, 0f, r.y);
                StartCoroutine(DropOne(pos));
                if (count > 1)
                    yield return new WaitForSeconds(Random.Range(0.4f, 1.4f));
            }
        }
    }

    private IEnumerator DropOne(Vector3 groundPos)
    {
        DangerZoneIndicator.Create(groundPos, meteorRadius, warningSeconds);
        yield return new WaitForSeconds(warningSeconds);

        var meteor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        meteor.name              = "AmbientMeteor";
        meteor.transform.position   = groundPos + Vector3.up * spawnHeight;
        meteor.transform.localScale = Vector3.one * 0.65f;
        ApplyLook(meteor);

        var rb = meteor.AddComponent<Rigidbody>();
        rb.useGravity             = true;
        rb.mass                   = 1.6f;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.angularVelocity        = Random.insideUnitSphere * 2.5f;

        var trail               = meteor.AddComponent<TrailRenderer>();
        trail.time              = 0.32f;
        trail.startWidth        = 0.52f;
        trail.endWidth          = 0.04f;
        trail.minVertexDistance = 0.15f;
        trail.startColor        = new Color(1f, 0.48f, 0.1f, 0.88f);
        trail.endColor          = new Color(0.4f, 0.08f, 0f, 0f);
        trail.material          = meteor.GetComponent<Renderer>().sharedMaterial;

        meteor.AddComponent<MeteorProjectile>().Configure(meteorDamage, meteorRadius, meteorDamage * 0.55f);
    }

    private static void ApplyLook(GameObject go)
    {
        var lit = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var mat = new Material(lit) { name = "M_AmbientMeteor_Runtime" };
        if (mat.HasProperty("_BaseColor"))     mat.SetColor("_BaseColor",    new Color(0.12f, 0.10f, 0.09f));
        if (mat.HasProperty("_Metallic"))      mat.SetFloat("_Metallic",     0.5f);
        if (mat.HasProperty("_Smoothness"))    mat.SetFloat("_Smoothness",   0.3f);
        mat.EnableKeyword("_EMISSION");
        if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", new Color(0.8f, 0.26f, 0.05f) * 0.55f);
        go.GetComponent<Renderer>().sharedMaterial = mat;

        go.AddComponent<ProceduralFallAudio>();

        var light       = go.AddComponent<Light>();
        light.type      = LightType.Point;
        light.color     = new Color(1f, 0.45f, 0.1f);
        light.range     = 12f;
        light.intensity = 7f;
    }
}
