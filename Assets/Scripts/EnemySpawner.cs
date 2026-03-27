using UnityEngine;

/// <summary>
/// Spawns enemies in discrete waves with a pause between waves. Difficulty scales with wave index.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private float spawnRadius = 24f;
    [SerializeField] private int maxEnemies = 28;

    [Header("Waves")]
    [SerializeField] private float delayBeforeFirstWave = 2f;
    [SerializeField] private float timeBetweenWaves = 8f;
    [SerializeField] private float spawnGapInWave = 0.65f;
    [SerializeField] private int baseEnemiesInWave = 4;
    [SerializeField] private int extraEnemiesPerWave = 2;

    private enum Phase
    {
        InitialWait,
        SpawningWave,
        BetweenWaves
    }

    private Phase _phase = Phase.InitialWait;
    private float _timer;
    private int _waveIndex;
    private int _quota;
    private int _spawnedInWave;

    /// <summary>Current wave index (0 = before wave 1, then 1, 2, …).</summary>
    public int CurrentWave => _waveIndex;

    private void Awake()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            enemyPrefabs = new[]
            {
                Resources.Load<GameObject>("Enemy"),
                Resources.Load<GameObject>("Enemy_Brute"),
                Resources.Load<GameObject>("Enemy_Swarm")
            };
        }
    }

    private void Start()
    {
        _timer = delayBeforeFirstWave;
    }

    private void Update()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
            return;

        switch (_phase)
        {
            case Phase.InitialWait:
            case Phase.BetweenWaves:
                _timer -= Time.deltaTime;
                if (_timer <= 0f)
                    BeginSpawningWave();
                break;

            case Phase.SpawningWave:
                _timer -= Time.deltaTime;
                if (_timer > 0f)
                    return;
                if (_spawnedInWave >= _quota)
                {
                    EnterBetweenWaves();
                    return;
                }
                if (TrySpawnOne())
                    _spawnedInWave++;
                _timer = spawnGapInWave;
                break;
        }
    }

    private void BeginSpawningWave()
    {
        _waveIndex++;
        _quota = baseEnemiesInWave + (_waveIndex - 1) * extraEnemiesPerWave;
        _spawnedInWave = 0;
        _phase = Phase.SpawningWave;
        _timer = 0f;
    }

    private void EnterBetweenWaves()
    {
        _phase = Phase.BetweenWaves;
        _timer = timeBetweenWaves;
    }

    private bool TrySpawnOne()
    {
        if (CountEnemies() >= maxEnemies)
            return false;

        Vector2 r = Random.insideUnitCircle * spawnRadius;
        Vector3 pos = new Vector3(r.x, 1f, r.y);
        GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        if (prefab == null)
            return false;
        Instantiate(prefab, pos, Quaternion.identity);
        return true;
    }

    private static int CountEnemies()
    {
        return GameObject.FindGameObjectsWithTag("Enemy").Length;
    }
}
