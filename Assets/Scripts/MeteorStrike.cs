using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class MeteorStrike : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float windupSeconds = 1.2f;
    [SerializeField] private float meteorCooldown = 4f;

    [Header("Effect")]
    [SerializeField] private float meteorRadius = 4f;
    [SerializeField] private float meteorDamage = 35f;
    [SerializeField] private float meteorSpawnHeight = 14f;
    [SerializeField] private float meteorBodyScale = 1.15f;

    private InputAction _meteorAction;
    private bool _casting;
    private float _nextMeteorAllowedTime;

    public float CooldownRemaining => Mathf.Max(0f, _nextMeteorAllowedTime - Time.time);

    private void Start()
    {
        _meteorAction = GetComponent<PlayerInput>().actions.FindActionMap("Player").FindAction("MeteorStrike", throwIfNotFound: false);
        if (_meteorAction == null)
            Debug.LogWarning("MeteorStrike: действие 'MeteorStrike' не найдено в PlayerControls.");
    }

    private void Update()
    {
        if (_meteorAction == null || _casting)
            return;
        if (Time.time < _nextMeteorAllowedTime)
            return;
        if (_meteorAction.WasPressedThisFrame() && MouseGroundUtility.TryGetAimGroundPoint(out Vector3 groundPoint))
            StartCoroutine(MeteorRoutine(groundPoint));
    }

    private IEnumerator MeteorRoutine(Vector3 groundPoint)
    {
        _casting = true;

        DangerZoneIndicator.Create(groundPoint, meteorRadius, windupSeconds);
        yield return new WaitForSeconds(windupSeconds);

        var meteor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        meteor.name = "Meteor";
        meteor.transform.position = groundPoint + Vector3.up * meteorSpawnHeight;
        meteor.transform.localScale = Vector3.one * meteorBodyScale;
        ApplyMeteorLook(meteor);

        var rb = meteor.AddComponent<Rigidbody>();
        rb.useGravity = true;
        rb.mass = 2.2f;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.angularVelocity = Random.insideUnitSphere * 1.8f;

        var trail = meteor.AddComponent<TrailRenderer>();
        trail.time = 0.4f;
        trail.startWidth = meteorBodyScale * 0.85f;
        trail.endWidth = meteorBodyScale * 0.12f;
        trail.minVertexDistance = 0.2f;
        trail.material = meteor.GetComponent<Renderer>().sharedMaterial;
        trail.startColor = new Color(1f, 0.55f, 0.15f, 0.95f);
        trail.endColor = new Color(0.4f, 0.1f, 0f, 0f);

        meteor.AddComponent<MeteorProjectile>().Configure(meteorDamage, meteorRadius);

        _nextMeteorAllowedTime = Time.time + meteorCooldown;
        _casting = false;
    }

    private static void ApplyMeteorLook(GameObject meteor)
    {
        var lit = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var outer = new Material(lit);
        outer.name = "M_MeteorOuter_Runtime";
        if (outer.HasProperty("_BaseColor"))
            outer.SetColor("_BaseColor", new Color(0.1f, 0.09f, 0.1f));
        if (outer.HasProperty("_Metallic"))
            outer.SetFloat("_Metallic", 0.45f);
        if (outer.HasProperty("_Smoothness"))
            outer.SetFloat("_Smoothness", 0.38f);
        outer.EnableKeyword("_EMISSION");
        if (outer.HasProperty("_EmissionColor"))
            outer.SetColor("_EmissionColor", new Color(0.95f, 0.32f, 0.08f) * 0.75f);

        var r = meteor.GetComponent<Renderer>();
        r.sharedMaterial = outer;

        var core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        core.name = "MeteorCore";
        core.transform.SetParent(meteor.transform, false);
        core.transform.localPosition = Vector3.zero;
        core.transform.localScale = Vector3.one * 0.58f;
        Object.Destroy(core.GetComponent<Collider>());

        var inner = new Material(outer);
        if (inner.HasProperty("_BaseColor"))
            inner.SetColor("_BaseColor", new Color(1f, 0.42f, 0.1f));
        if (inner.HasProperty("_EmissionColor"))
            inner.SetColor("_EmissionColor", new Color(1f, 0.45f, 0.05f) * 2.2f);

        core.GetComponent<Renderer>().sharedMaterial = inner;

        meteor.AddComponent<ProceduralFallAudio>();

        var light = meteor.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1f, 0.5f, 0.1f);
        light.range = 20f;
        light.intensity = 15f;
    }
}
