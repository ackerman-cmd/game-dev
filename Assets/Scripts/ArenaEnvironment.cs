using UnityEngine;

/// <summary>
/// Spawns non-colliding edge props and old impact marks. Keeps the center of the arena clear.
/// Walls are authored in the scene; this only adds modular decoration at runtime.
/// </summary>
public class ArenaEnvironment : MonoBehaviour
{
    [SerializeField] private Material crateMaterial;
    [SerializeField] private Material scorchMaterial;

    [Header("Layout (matches expanded floor ~70m)")]
    [SerializeField] private float edgePropDistance = 26f;
    [SerializeField] private float scorchDistance = 18f;

    private void Awake()
    {
        if (transform.childCount > 0)
            return;

        BuildCrates();
        BuildScorchMarks();
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
