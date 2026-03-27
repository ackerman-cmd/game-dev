using UnityEngine;

/// <summary>
/// Layers primitive "greebles" for a stronger top-down silhouette (rim, cockpit, thrusters / spikes).
/// Typical arcade read: friendly = round + cool accents; enemies = angular / warm palette.
/// </summary>
[DefaultExecutionOrder(-200)]
public class TopDownVisualDetail : MonoBehaviour
{
    public enum Style
    {
        PlayerHero = 0,
        EnemyStalker = 1,
        EnemyBrute = 2,
        EnemySwarm = 3
    }

    [SerializeField] private Style style = Style.PlayerHero;

    private static Material s_trimHero;
    private static Material s_trimEnemy;
    private static Material s_glowHero;
    private static Material s_glowEnemyWarm;
    private static Material s_glowEnemyHot;

    private static void EnsureMaterials()
    {
        if (s_trimHero != null)
            return;

        var lit = Shader.Find("Universal Render Pipeline/Lit");
        if (lit == null)
            lit = Shader.Find("Standard");

        s_trimHero = new Material(lit) { name = "TD_TrimHero" };
        ApplyBase(s_trimHero, new Color(0.06f, 0.12f, 0.22f), 0.65f, 0.55f);

        s_trimEnemy = new Material(lit) { name = "TD_TrimEnemy" };
        ApplyBase(s_trimEnemy, new Color(0.18f, 0.05f, 0.06f), 0.4f, 0.35f);

        s_glowHero = new Material(lit) { name = "TD_GlowHero" };
        ApplyBase(s_glowHero, new Color(0.2f, 0.85f, 0.95f), 0.35f, 0.65f);
        ApplyEmission(s_glowHero, new Color(0.15f, 0.65f, 0.85f), 1.1f);

        s_glowEnemyWarm = new Material(lit) { name = "TD_GlowEnemyWarm" };
        ApplyBase(s_glowEnemyWarm, new Color(0.95f, 0.45f, 0.2f), 0.2f, 0.45f);
        ApplyEmission(s_glowEnemyWarm, new Color(0.6f, 0.15f, 0.05f), 0.85f);

        s_glowEnemyHot = new Material(lit) { name = "TD_GlowEnemyHot" };
        ApplyBase(s_glowEnemyHot, new Color(0.55f, 0.1f, 0.12f), 0.25f, 0.4f);
        ApplyEmission(s_glowEnemyHot, new Color(0.35f, 0.02f, 0.04f), 0.6f);
    }

    private static void ApplyBase(Material m, Color baseColor, float metallic, float smoothness)
    {
        if (m.HasProperty("_BaseColor"))
            m.SetColor("_BaseColor", baseColor);
        else
            m.color = baseColor;
        if (m.HasProperty("_Metallic"))
            m.SetFloat("_Metallic", metallic);
        if (m.HasProperty("_Smoothness"))
            m.SetFloat("_Smoothness", smoothness);
    }

    private static void ApplyEmission(Material m, Color emission, float intensity)
    {
        m.EnableKeyword("_EMISSION");
        if (m.HasProperty("_EmissionColor"))
            m.SetColor("_EmissionColor", emission * intensity);
    }

    private void Awake()
    {
        if (transform.Find("Detail_Meta") != null)
            return;

        EnsureMaterials();

        var meta = new GameObject("Detail_Meta");
        meta.transform.SetParent(transform, false);

        switch (style)
        {
            case Style.PlayerHero:
                BuildPlayer();
                break;
            case Style.EnemyStalker:
                BuildStalker();
                break;
            case Style.EnemyBrute:
                BuildBrute();
                break;
            case Style.EnemySwarm:
                BuildSwarm();
                break;
        }

        var mark = new GameObject("Detail_Meta");
        mark.transform.SetParent(transform, false);
    }

    private void BuildPlayer()
    {
        AddRing(transform, "Detail_Skirt", new Vector3(0f, -0.04f, 0f), new Vector3(1.42f, 0.08f, 1.42f), s_trimHero);

        var hull = transform.Find("PlayerHull");
        if (hull != null)
        {
            AddPrim(hull, "Detail_Cockpit", PrimitiveType.Sphere, new Vector3(0f, 0.3f, 0f), Vector3.one * 0.3f, s_glowHero);
            AddPrim(hull, "Detail_WingL", PrimitiveType.Cube, new Vector3(-0.5f, 0.14f, 0.02f), new Vector3(0.14f, 0.07f, 0.32f), s_trimHero);
            AddPrim(hull, "Detail_WingR", PrimitiveType.Cube, new Vector3(0.5f, 0.14f, 0.02f), new Vector3(0.14f, 0.07f, 0.32f), s_trimHero);
        }

        AddPrim(transform, "Detail_ThrusterL", PrimitiveType.Cube, new Vector3(-0.16f, 0.06f, -0.62f), new Vector3(0.14f, 0.1f, 0.22f), s_glowHero);
        AddPrim(transform, "Detail_ThrusterR", PrimitiveType.Cube, new Vector3(0.16f, 0.06f, -0.62f), new Vector3(0.14f, 0.1f, 0.22f), s_glowHero);
        AddPrim(transform, "Detail_Spine", PrimitiveType.Cube, new Vector3(0f, 0.2f, -0.08f), new Vector3(0.35f, 0.08f, 0.55f), s_trimHero);
    }

    private void BuildStalker()
    {
        AddRing(transform, "Detail_Rim", new Vector3(0f, -0.06f, 0f), new Vector3(0.95f, 0.07f, 0.95f), s_trimEnemy);

        var body = transform.Find("StalkerBody");
        if (body != null)
        {
            AddPrim(body, "Detail_Head", PrimitiveType.Sphere, new Vector3(0f, 0.62f, 0f), Vector3.one * 0.26f, s_glowEnemyWarm);
            AddPrim(body, "Detail_SpikeL", PrimitiveType.Cube, new Vector3(-0.32f, 0.38f, 0f), new Vector3(0.1f, 0.22f, 0.12f), s_trimEnemy);
            AddPrim(body, "Detail_SpikeR", PrimitiveType.Cube, new Vector3(0.32f, 0.38f, 0f), new Vector3(0.1f, 0.22f, 0.12f), s_trimEnemy);
            AddPrim(body, "Detail_Spine", PrimitiveType.Cube, new Vector3(0f, 0.15f, -0.18f), new Vector3(0.22f, 0.35f, 0.2f), s_trimEnemy);
        }
    }

    private void BuildBrute()
    {
        var torso = transform.Find("BruteTorso");
        if (torso != null)
        {
            AddPrim(torso, "Detail_ArmL", PrimitiveType.Cube, new Vector3(-0.72f, 0f, 0f), new Vector3(0.38f, 0.22f, 0.22f), s_trimEnemy);
            AddPrim(torso, "Detail_ArmR", PrimitiveType.Cube, new Vector3(0.72f, 0f, 0f), new Vector3(0.38f, 0.22f, 0.22f), s_trimEnemy);
            AddPrim(torso, "Detail_Belt", PrimitiveType.Cube, new Vector3(0f, -0.18f, 0f), new Vector3(1.05f, 0.1f, 0.88f), s_glowEnemyHot);
            AddPrim(torso, "Detail_Rivet", PrimitiveType.Cylinder, new Vector3(0.4f, 0.12f, 0.35f), new Vector3(0.14f, 0.06f, 0.14f), s_glowEnemyWarm);
        }

        var head = transform.Find("BruteHead");
        if (head != null)
            AddPrim(head, "Detail_Visor", PrimitiveType.Cube, new Vector3(0f, 0f, 0.22f), new Vector3(0.55f, 0.35f, 0.12f), s_glowEnemyWarm);
    }

    private void BuildSwarm()
    {
        var core = transform.Find("SwarmCore");
        if (core != null)
        {
            AddPrim(core, "Detail_InnerCore", PrimitiveType.Sphere, new Vector3(0f, 0.05f, 0f), Vector3.one * 0.4f, s_glowEnemyWarm);
            AddPrim(core, "Detail_Ring", PrimitiveType.Cylinder, Vector3.zero, new Vector3(1.12f, 0.04f, 1.12f), s_trimEnemy);
        }

        AddPrim(transform, "Detail_SpikeN", PrimitiveType.Cube, new Vector3(0f, 0.12f, 0.42f), new Vector3(0.1f, 0.2f, 0.1f), s_trimEnemy);
        AddPrim(transform, "Detail_SpikeS", PrimitiveType.Cube, new Vector3(0f, 0.12f, -0.42f), new Vector3(0.1f, 0.2f, 0.1f), s_trimEnemy);
    }

    private static void AddRing(Transform parent, string name, Vector3 localPos, Vector3 localScale, Material mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = localScale;
        StripCollider(go);
        Apply(go, mat);
    }

    private static void AddPrim(Transform parent, string name, PrimitiveType type, Vector3 localPos, Vector3 localScale, Material mat)
    {
        var go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = localScale;
        StripCollider(go);
        Apply(go, mat);
    }

    private static void StripCollider(GameObject go)
    {
        var c = go.GetComponent<Collider>();
        if (c != null)
            Destroy(c);
    }

    private static void Apply(GameObject go, Material mat)
    {
        var r = go.GetComponent<Renderer>();
        if (r != null)
            r.sharedMaterial = mat;
    }
}
