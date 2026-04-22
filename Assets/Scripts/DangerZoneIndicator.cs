using UnityEngine;

/// <summary>
/// Animated ground circle that warns players of an incoming meteor impact.
/// Transitions from yellow to red over its lifetime; auto-destroys on expiry.
/// </summary>
public class DangerZoneIndicator : MonoBehaviour
{
    private float _duration;
    private float _elapsed;
    private Material _diskMat;
    private Material _ringMat;
    private Transform _ringTransform;
    private Vector3 _ringBaseScale;

    private static readonly int BaseColorId     = Shader.PropertyToID("_BaseColor");
    private static readonly int EmissionColorId  = Shader.PropertyToID("_EmissionColor");

    public static DangerZoneIndicator Create(Vector3 groundPoint, float radius, float duration)
    {
        var root = new GameObject("DangerZoneIndicator");
        root.transform.position = new Vector3(groundPoint.x, 0.04f, groundPoint.z);

        var disk = MakeFlatCylinder("DZ_Disk", root.transform, radius * 2f, 0.018f);
        Destroy(disk.GetComponent<Collider>());

        var ring = MakeFlatCylinder("DZ_Ring", root.transform, radius * 2f + 0.8f, 0.024f);
        Destroy(ring.GetComponent<Collider>());
        ring.transform.localPosition = new Vector3(0f, 0.012f, 0f);

        var comp            = root.AddComponent<DangerZoneIndicator>();
        comp._duration      = duration;
        comp._diskMat       = MakeTransparentMat("M_DZ_Disk");
        comp._ringMat       = MakeTransparentMat("M_DZ_Ring");
        comp._ringTransform = ring.transform;
        comp._ringBaseScale = ring.transform.localScale;

        disk.GetComponent<Renderer>().sharedMaterial = comp._diskMat;
        ring.GetComponent<Renderer>().sharedMaterial = comp._ringMat;

        return comp;
    }

    private static GameObject MakeFlatCylinder(string name, Transform parent, float diameter, float height)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localScale = new Vector3(diameter, height, diameter);
        return go;
    }

    private static Material MakeTransparentMat(string matName)
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var mat    = new Material(shader) { name = matName };
        mat.SetFloat("_Surface", 1);
        mat.SetInt("_SrcBlend",  (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend",  (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
        mat.EnableKeyword("_EMISSION");
        return mat;
    }

    private void Update()
    {
        _elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(_elapsed / _duration);

        // Yellow → orange → red as impact approaches
        Color diskBase = Color.Lerp(new Color(1f, 0.88f, 0.08f, 0.14f), new Color(1f, 0.08f, 0.04f, 0.30f), t);
        Color ringBase = Color.Lerp(new Color(1f, 0.85f, 0.10f, 0.92f), new Color(1f, 0.08f, 0.04f, 1.00f), t);
        float pulse    = 1f + 0.7f * Mathf.Abs(Mathf.Sin(_elapsed * 10f));
        Color emission = Color.Lerp(new Color(0.7f, 0.5f, 0f), new Color(1.1f, 0.08f, 0f), t) * pulse;

        SetMatColor(_diskMat, diskBase, emission * 0.3f);
        SetMatColor(_ringMat, ringBase, emission * 2f);

        if (_ringTransform != null)
        {
            float s = 1f + 0.05f * Mathf.Sin(_elapsed * 10f);
            _ringTransform.localScale = new Vector3(
                _ringBaseScale.x * s,
                _ringBaseScale.y,
                _ringBaseScale.z * s);
        }

        if (_elapsed >= _duration)
            Destroy(gameObject);
    }

    private void SetMatColor(Material mat, Color base_, Color emission)
    {
        if (mat == null) return;
        if (mat.HasProperty(BaseColorId))    mat.SetColor(BaseColorId,    base_);
        else                                  mat.color = base_;
        if (mat.HasProperty(EmissionColorId)) mat.SetColor(EmissionColorId, emission);
    }
}
