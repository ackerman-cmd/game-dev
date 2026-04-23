using UnityEngine;

public class RuneSpawner : MonoBehaviour
{
    [SerializeField] private float spawnInterval = 20f;
    [SerializeField] private float spawnRadius = 24f;
    
    private float _nextSpawnTime;

    private void Start()
    {
        _nextSpawnTime = Time.time + spawnInterval;
    }

    private void Update()
    {
        if (Time.time >= _nextSpawnTime)
        {
            SpawnRune();
            _nextSpawnTime = Time.time + spawnInterval;
        }
    }

    private void SpawnRune()
    {
        Vector2 r = Random.insideUnitCircle * spawnRadius;
        Vector3 pos = new Vector3(r.x, 1f, r.y);

        var rune = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rune.transform.position = pos;
        rune.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        rune.transform.rotation = Quaternion.Euler(45f, 45f, 45f);

        var col = rune.GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        var comp = rune.AddComponent<PowerupRune>();
        comp.runeType = (PowerupRune.Type)Random.Range(0, 3);

        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        mat.EnableKeyword("_EMISSION");
        
        switch (comp.runeType)
        {
            case PowerupRune.Type.Heal:
                mat.SetColor("_BaseColor", Color.green);
                mat.SetColor("_EmissionColor", Color.green * 2f);
                break;
            case PowerupRune.Type.AttackSpeed:
                mat.SetColor("_BaseColor", Color.yellow);
                mat.SetColor("_EmissionColor", Color.yellow * 2f);
                break;
            case PowerupRune.Type.Invincible:
                mat.SetColor("_BaseColor", Color.cyan);
                mat.SetColor("_EmissionColor", Color.cyan * 2f);
                break;
        }

        var rend = rune.GetComponent<Renderer>();
        if (rend != null) rend.sharedMaterial = mat;

        var light = rune.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = mat.GetColor("_EmissionColor");
        light.range = 5f;
        light.intensity = 5f;
        
        Destroy(rune, 15f);
    }
}
