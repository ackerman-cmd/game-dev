using UnityEngine;

public class ArenaEnvironment : MonoBehaviour
{
    [SerializeField] private Material crateMaterial;
    [SerializeField] private Material scorchMaterial;
    [SerializeField] private Material obstacleMaterial;

    [Header("Layout")]
    [SerializeField] private float edgePropDistance = 26f;
    [SerializeField] private float scorchDistance = 18f;
    [SerializeField] private int obstacleCount = 12;

    private void Awake()
    {
        if (transform.childCount > 0)
            return;

        BuildCrates();
        BuildScorchMarks();
        BuildObstacles();
        BuildSecondLevel();
        BuildBoundaries();
    }

    private void BuildSecondLevel()
    {
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        mat.SetColor("_BaseColor", new Color(0.12f, 0.13f, 0.18f));
        mat.SetFloat("_Metallic", 0.4f);
        mat.SetFloat("_Smoothness", 0.5f);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", new Color(0.02f, 0.05f, 0.08f));

        float pHeight = 3.5f;

        for (int i = 0; i < 4; i++)
        {
            float angle = (i * 90f + 45f) * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            Vector3 pos = dir * 26f;

            var platform = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            platform.name = $"Platform_{i}";
            platform.transform.SetParent(transform, false);
            platform.transform.position = pos + Vector3.up * (pHeight / 2f);
            platform.transform.localScale = new Vector3(18f, pHeight / 2f, 18f);
            Apply(platform.GetComponent<Renderer>(), mat);

            const float rampGroundDist = 8f;
            const float rampPlatDist   = 18f;
            const float rampGroundY    = 0.05f;
            float       rampPlatY      = pHeight;
            float       dH             = rampPlatDist - rampGroundDist;
            float       dV             = rampPlatY    - rampGroundY;
            float       rampLength     = Mathf.Sqrt(dH * dH + dV * dV);
            float       rampAngle      = Mathf.Atan2(dV, dH) * Mathf.Rad2Deg;

            var ramp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ramp.name = $"Ramp_{i}";
            ramp.transform.SetParent(transform, false);
            ramp.transform.localScale = new Vector3(8f, 0.5f, rampLength);
            ramp.transform.position   = dir * ((rampGroundDist + rampPlatDist) * 0.5f)
                                        + Vector3.up * ((rampGroundY + rampPlatY) * 0.5f);
            ramp.transform.rotation = Quaternion.LookRotation(-dir) * Quaternion.Euler(rampAngle, 0f, 0f);
            Apply(ramp.GetComponent<Renderer>(), mat);

            var rim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rim.name = $"PlatformRim_{i}";
            rim.transform.SetParent(platform.transform, false);
            rim.transform.localPosition = new Vector3(0f, 1.01f, 0f);
            rim.transform.localScale = new Vector3(1.02f, 0.05f, 1.02f);
            Destroy(rim.GetComponent<Collider>());
            var rimMat = new Material(mat);
            rimMat.SetColor("_EmissionColor", new Color(0.1f, 0.4f, 0.6f) * 1.5f);
            Apply(rim.GetComponent<Renderer>(), rimMat);
        }
    }

    private void BuildObstacles()
    {
        if (obstacleMaterial == null)
        {
            obstacleMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            obstacleMaterial.SetColor("_BaseColor", new Color(0.2f, 0.22f, 0.25f));
            obstacleMaterial.SetFloat("_Metallic", 0.3f);
            obstacleMaterial.SetFloat("_Smoothness", 0.2f);
        }

        Random.InitState(42);

        for (int i = 0; i < obstacleCount; i++)
        {
            Vector2 randCircle = Random.insideUnitCircle.normalized * Random.Range(10f, 32f);
            Vector3 pos = new Vector3(randCircle.x, 0f, randCircle.y);

            var root = new GameObject($"Obstacle_Rock_{i}");
            root.transform.SetParent(transform, false);
            root.transform.position = pos;

            int pieces = Random.Range(2, 5);
            for (int p = 0; p < pieces; p++)
            {
                var piece = GameObject.CreatePrimitive(PrimitiveType.Cube);
                piece.transform.SetParent(root.transform, false);
                piece.transform.localPosition = Random.insideUnitSphere * 0.5f + Vector3.up * 0.5f;
                piece.transform.localRotation = Random.rotation;
                float scale = Random.Range(1.2f, 2.5f);
                piece.transform.localScale = new Vector3(scale, scale * Random.Range(0.8f, 1.5f), scale);
                Apply(piece.GetComponent<Renderer>(), obstacleMaterial);
            }
        }
    }

    private void BuildBoundaries()
    {
        var boundaryMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        boundaryMat.SetColor("_BaseColor", new Color(0.1f, 0.1f, 0.12f));
        boundaryMat.SetFloat("_Metallic", 0.7f);
        boundaryMat.SetFloat("_Smoothness", 0.6f);
        boundaryMat.EnableKeyword("_EMISSION");
        boundaryMat.SetColor("_EmissionColor", new Color(0.06f, 0.28f, 0.46f) * 3f);

        for (int i = 0; i < 36; i++)
        {
            float angle = i * 10f * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * 36f;

            var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.name = $"Boundary_Pillar_{i}";
            pillar.transform.SetParent(transform, false);
            pillar.transform.position = pos + Vector3.up * 4f;
            pillar.transform.localScale = new Vector3(1.6f, 4f, 1.6f);

            Apply(pillar.GetComponent<Renderer>(), boundaryMat);
        }
    }

    private void BuildCrates()
    {
        float d = edgePropDistance;
        Vector3[] corners =
        {
            new Vector3(d, 0.6f, d),
            new Vector3(-d, 0.6f, d),
            new Vector3(d, 0.6f, -d),
            new Vector3(-d, 0.6f, -d)
        };

        for (int i = 0; i < corners.Length; i++)
        {
            var crate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crate.name = $"Prop_Crate_{i + 1}";
            crate.transform.SetParent(transform, false);
            crate.transform.localPosition = corners[i];
            crate.transform.localScale = new Vector3(1.4f, 1.2f, 1.4f);
            crate.transform.localRotation = Quaternion.Euler(0f, i * 17f, 0f);
            Apply(crate.GetComponent<Renderer>(), crateMaterial);
        }
    }

    private void BuildScorchMarks()
    {
        float d = scorchDistance;
        Vector3[] positions =
        {
            new Vector3(d, 0.02f, d * 0.4f),
            new Vector3(-d * 0.5f, 0.02f, d),
            new Vector3(d * 0.3f, 0.02f, -d),
            new Vector3(-d, 0.02f, -d * 0.35f)
        };

        for (int i = 0; i < positions.Length; i++)
        {
            var mark = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mark.name = $"Decal_Scorch_{i + 1}";
            mark.transform.SetParent(transform, false);
            mark.transform.localPosition = positions[i];
            mark.transform.localScale = new Vector3(5f, 0.02f, 5f);
            Object.Destroy(mark.GetComponent<Collider>());
            Apply(mark.GetComponent<Renderer>(), scorchMaterial);
        }
    }

    private static void Apply(Renderer renderer, Material mat)
    {
        if (renderer == null || mat == null)
            return;
        renderer.sharedMaterial = mat;
    }
}
