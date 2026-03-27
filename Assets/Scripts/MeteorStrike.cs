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

    [Header("Indicator")]
    [SerializeField] private Color indicatorColor = new Color(1f, 0.35f, 0.1f, 0.9f);

    private InputAction _meteorAction;
    private bool _casting;
    private float _nextMeteorAllowedTime;

    /// <summary>Seconds until meteor can be used again (for HUD).</summary>
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
        if (_meteorAction.WasPressedThisFrame() && TryGetGroundPoint(out Vector3 groundPoint))
            StartCoroutine(MeteorRoutine(groundPoint));
    }

    private static bool TryGetGroundPoint(out Vector3 point)
    {
        point = default;
        var cam = Camera.main;
        if (cam == null || Mouse.current == null)
            return false;

        var ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        var plane = new Plane(Vector3.up, Vector3.zero);
        if (!plane.Raycast(ray, out float enter))
            return false;

        point = ray.GetPoint(enter);
        return true;
    }

    private IEnumerator MeteorRoutine(Vector3 groundPoint)
    {
        _casting = true;

        var indicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        indicator.name = "MeteorIndicator";
        indicator.transform.SetPositionAndRotation(new Vector3(groundPoint.x, 0.05f, groundPoint.z), Quaternion.identity);
        indicator.transform.localScale = new Vector3(meteorRadius * 2f, 0.05f, meteorRadius * 2f);
        Object.Destroy(indicator.GetComponent<Collider>());
        var indRenderer = indicator.GetComponent<Renderer>();
        if (indRenderer != null)
            indRenderer.material.color = indicatorColor;

        yield return new WaitForSeconds(windupSeconds);

        if (indicator != null)
            Object.Destroy(indicator);

        var meteor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        meteor.name = "Meteor";
        meteor.transform.position = groundPoint + Vector3.up * meteorSpawnHeight;
        meteor.transform.localScale = Vector3.one * 0.85f;
        var rb = meteor.AddComponent<Rigidbody>();
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        meteor.AddComponent<MeteorProjectile>().Configure(meteorDamage, meteorRadius);

        _nextMeteorAllowedTime = Time.time + meteorCooldown;
        _casting = false;
    }
}
