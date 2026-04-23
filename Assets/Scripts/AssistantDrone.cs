using UnityEngine;

public class AssistantDrone : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float hoverHeight = 2.5f;
    [SerializeField] private float hoverRadius = 1.5f;
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private float hoverFrequency = 1.5f;
    [SerializeField] private float hoverAmplitude = 0.2f;

    [Header("Combat")]
    [SerializeField] private float detectionRadius = 12f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float attackDamage = 15f;
    [SerializeField] private float projectileSpeed = 20f;
    [SerializeField] private Transform firePoint;

    private Transform _playerTarget;
    private float _nextAttackTime;
    private float _time;

    private void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            _playerTarget = player.transform;

        if (firePoint == null)
            firePoint = transform;
    }

    private void Update()
    {
        if (_playerTarget == null)
            return;

        _time += Time.deltaTime;

        Vector3 targetPos = _playerTarget.position + new Vector3(
            Mathf.Cos(_time * 0.5f) * hoverRadius,
            hoverHeight + Mathf.Sin(_time * hoverFrequency) * hoverAmplitude,
            Mathf.Sin(_time * 0.5f) * hoverRadius);

        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * followSpeed);

        if (Time.time >= _nextAttackTime)
        {
            Enemy nearest = FindNearestEnemy();
            if (nearest != null)
            {
                Vector3 dir = nearest.transform.position - transform.position;
                dir.y = 0;
                if (dir.sqrMagnitude > 0.01f)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir.normalized), Time.deltaTime * 10f);
                }

                FireAt(nearest);
                _nextAttackTime = Time.time + attackCooldown;
            }
            else
            {
                Vector3 playerFwd = _playerTarget.forward;
                playerFwd.y = 0;
                if (playerFwd.sqrMagnitude > 0.01f)
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(playerFwd.normalized), Time.deltaTime * 2f);
            }
        }
    }

    private Enemy FindNearestEnemy()
    {
        Enemy nearest = null;
        float minDistSqr = detectionRadius * detectionRadius;
        Vector3 pos = transform.position;

        foreach (var enemy in FindObjectsByType<Enemy>(FindObjectsSortMode.None))
        {
            float d = (enemy.transform.position - pos).sqrMagnitude;
            if (d < minDistSqr)
            {
                minDistSqr = d;
                nearest = enemy;
            }
        }

        return nearest;
    }

    private void FireAt(Enemy target)
    {
        Vector3 dir = (target.transform.position + Vector3.up * 0.5f) - firePoint.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f)
            dir = transform.forward;
        else
            dir.Normalize();

        Projectile.Create(firePoint.position, dir, attackDamage, projectileSpeed);
    }
}
