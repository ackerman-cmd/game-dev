using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    public float targetScale = 10f;
    public float duration = 0.5f;
    
    private float _time;
    private Vector3 _initialScale;
    private Material _mat;
    private Color _initialColor;

    private void Start()
    {
        gameObject.AddComponent<ProceduralExplosionAudio>();
        _initialScale = transform.localScale;
        var renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            _mat = renderer.material;
            if (_mat.HasProperty("_BaseColor"))
                _initialColor = _mat.GetColor("_BaseColor");
        }
    }

    private void Update()
    {
        _time += Time.deltaTime;
        float t = Mathf.Clamp01(_time / duration);
        float easeOut = 1f - Mathf.Pow(1f - t, 3f);
        
        transform.localScale = Vector3.Lerp(_initialScale, Vector3.one * targetScale, easeOut);
        
        if (_mat != null)
        {
            Color c = _initialColor;
            c.a = Mathf.Lerp(_initialColor.a, 0f, easeOut);
            _mat.SetColor("_BaseColor", c);
        }

        if (t >= 1f)
            Destroy(gameObject);
    }
}
