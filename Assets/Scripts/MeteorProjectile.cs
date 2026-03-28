using UnityEngine;

public class MeteorProjectile : MonoBehaviour
{
    [SerializeField] private int debrisCount = 12;
    [SerializeField] private float debrisLifetime = 3.8f;
    [SerializeField] private float debrisImpulse = 7f;

    private float _damage;
    private float _radius;
    private bool _hit;

    private static Material s_debrisMat;

    public void Configure(float damage, float radius)
    {
        _damage = damage;
        _radius = radius;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_hit)
            return;
        _hit = true;

        Vector3 center = transform.position;

        foreach (var c in Physics.OverlapSphere(center, _radius))
        {
            var enemy = c.GetComponent<Enemy>();
            if (enemy != null)
                enemy.TakeDamage(_damage);
        }

        SpawnDebris(center);
        SpawnExplosion(center);
        CameraShake.TriggerShake(0.6f, 0.4f);
        Destroy(gameObject);
    }

    private void SpawnExplosion(Vector3 point)
    {
        var explosion = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        explosion.transform.position = point;
        explosion.transform.localScale = Vector3.one * (_radius * 0.5f);
        Destroy(explosion.GetComponent<Collider>());
        
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        mat.SetColor("_BaseColor", new Color(1f, 0.5f, 0.1f, 0.5f));
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", new Color(1f, 0.3f, 0.05f) * 2f);
        
        mat.SetFloat("_Surface", 1);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
        
        explosion.GetComponent<Renderer>().sharedMaterial = mat;
        
        var expand = explosion.AddComponent<ExplosionEffect>();
        expand.targetScale = _radius * 2f;
        expand.duration = 0.4f;
    }

    private void SpawnDebris(Vector3 point)
    {
        EnsureDebrisMaterial();

        for (int i = 0; i < debrisCount; i++)
        {
            var piece = GameObject.CreatePrimitive(PrimitiveType.Cube);
            piece.name = "MeteorDebris";
            piece.transform.position = point + Random.insideUnitSphere * 0.35f;
            piece.transform.localScale = Vector3.one * Random.Range(0.14f, 0.36f);
            piece.transform.rotation = Random.rotation;

            var rb = piece.AddComponent<Rigidbody>();
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.mass = 0.08f;

            Vector3 impulse = Random.onUnitSphere * Random.Range(debrisImpulse * 0.35f, debrisImpulse);
            impulse.y = Mathf.Max(impulse.y, 1.2f) + Random.Range(0f, 3f);
            rb.AddForce(impulse, ForceMode.VelocityChange);
            rb.AddTorque(Random.insideUnitSphere * 8f, ForceMode.VelocityChange);

            var r = piece.GetComponent<Renderer>();
            if (r != null)
                r.sharedMaterial = s_debrisMat;

            Destroy(piece, debrisLifetime + Random.Range(0f, 0.6f));
        }
    }

    private static void EnsureDebrisMaterial()
    {
        if (s_debrisMat != null)
            return;

        var lit = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        s_debrisMat = new Material(lit);
        s_debrisMat.name = "M_MeteorDebris_Runtime";
        if (s_debrisMat.HasProperty("_BaseColor"))
            s_debrisMat.SetColor("_BaseColor", new Color(0.18f, 0.14f, 0.12f));
        if (s_debrisMat.HasProperty("_Metallic"))
            s_debrisMat.SetFloat("_Metallic", 0.35f);
        if (s_debrisMat.HasProperty("_Smoothness"))
            s_debrisMat.SetFloat("_Smoothness", 0.4f);
        s_debrisMat.EnableKeyword("_EMISSION");
        if (s_debrisMat.HasProperty("_EmissionColor"))
            s_debrisMat.SetColor("_EmissionColor", new Color(0.85f, 0.25f, 0.05f) * 0.9f);
    }
}
