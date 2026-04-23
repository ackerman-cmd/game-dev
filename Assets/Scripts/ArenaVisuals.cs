using UnityEngine;

public class ArenaVisuals : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCreate()
    {
        if (Object.FindFirstObjectByType<ArenaVisuals>() == null)
            new GameObject("[ArenaVisuals]").AddComponent<ArenaVisuals>();
    }

    private void Start()
    {
        ApplyAtmosphere();
        BuildGroundEmblem();
        BuildArenaLights();
    }

    private static void ApplyAtmosphere()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.055f, 0.065f, 0.11f);

        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.04f, 0.045f, 0.08f);
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 22f;
        RenderSettings.fogEndDistance   = 55f;
    }

    private static void BuildGroundEmblem()
    {
        MakeDisk("Emblem_Boundary", 0.010f, 72.8f,
            new Color(0.03f, 0.03f, 0.05f),
            new Color(0.04f, 0.22f, 0.4f) * 0.5f);

        MakeDisk("Emblem_Mid", 0.014f, 36.4f,
            new Color(0.04f, 0.04f, 0.07f),
            new Color(0.04f, 0.22f, 0.4f) * 0.35f);

        MakeDisk("Emblem_Center", 0.018f, 10f,
            new Color(0.06f, 0.14f, 0.24f),
            new Color(0.05f, 0.32f, 0.58f) * 0.9f);

        MakeDisk("Emblem_Core", 0.024f, 2.8f,
            new Color(0.10f, 0.26f, 0.42f),
            new Color(0.08f, 0.50f, 0.80f) * 1.4f);

        AddLight("Emblem_CoreLight", Vector3.up * 0.5f,
            new Color(0.2f, 0.6f, 1.0f), 8f, 1.4f);
    }

    private static void BuildArenaLights()
    {
        AddLight("Sky_Storm", Vector3.up * 28f,
            new Color(0.65f, 0.30f, 0.07f), 75f, 0.7f);

        float[] innerAngles = { 0f, 90f, 180f, 270f };
        foreach (float a in innerAngles)
        {
            float rad = a * Mathf.Deg2Rad;
            var pos = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * 20f + Vector3.up * 5f;
            AddLight($"Accent_Teal_{a:0}", pos, new Color(0.10f, 0.45f, 0.78f), 22f, 2.5f);
        }

        float[] cornerAngles = { 45f, 135f, 225f, 315f };
        foreach (float a in cornerAngles)
        {
            float rad = a * Mathf.Deg2Rad;
            var pos = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * 26f + Vector3.up * 6f;
            AddLight($"Platform_Danger_{a:0}", pos, new Color(0.9f, 0.26f, 0.05f), 18f, 1.8f);
        }

        for (int i = 0; i < 8; i++)
        {
            float rad = i * 45f * Mathf.Deg2Rad;
            var pos = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * 35f + Vector3.up * 4f;
            AddLight($"Boundary_Rim_{i}", pos, new Color(0.12f, 0.38f, 0.62f), 14f, 1.2f);
        }
    }

    private static void MakeDisk(string name, float yPos, float diameter, Color baseColor, Color emission)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = name;
        go.transform.position   = Vector3.up * yPos;
        go.transform.localScale = new Vector3(diameter, 0.012f, diameter);
        Object.Destroy(go.GetComponent<Collider>());

        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var mat    = new Material(shader) { name = $"M_{name}" };
        if (mat.HasProperty("_BaseColor"))     mat.SetColor("_BaseColor",    baseColor);
        if (mat.HasProperty("_Metallic"))      mat.SetFloat("_Metallic",     0.5f);
        if (mat.HasProperty("_Smoothness"))    mat.SetFloat("_Smoothness",   0.65f);
        mat.EnableKeyword("_EMISSION");
        if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", emission);
        go.GetComponent<Renderer>().sharedMaterial = mat;
    }

    private static void AddLight(string name, Vector3 pos, Color color, float range, float intensity)
    {
        var go    = new GameObject(name);
        go.transform.position = pos;
        var light = go.AddComponent<Light>();
        light.type      = LightType.Point;
        light.color     = color;
        light.range     = range;
        light.intensity = intensity;
    }
}
