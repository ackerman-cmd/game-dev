using UnityEngine;

public class MeteorProjectile : MonoBehaviour
{
    [SerializeField] private float impactFlashScale = 1.35f;

    private float _damage;
    private float _radius;
    private bool _hit;

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

        transform.localScale = Vector3.one * impactFlashScale;

        Vector3 center = transform.position;
        foreach (var c in Physics.OverlapSphere(center, _radius))
        {
            var enemy = c.GetComponent<Enemy>();
            if (enemy != null)
                enemy.TakeDamage(_damage);
        }

        Destroy(gameObject);
    }
}
