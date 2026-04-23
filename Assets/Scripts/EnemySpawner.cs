using UnityEngine;

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

    private float _difficultyMultiplier = 1f;
    private bool  _gameStarted;

    public int CurrentWave => _waveIndex;

    public void SetDifficulty(float diff)
    {
        _difficultyMultiplier = diff;
        _gameStarted = true;
        _timer = delayBeforeFirstWave;
    }

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
        if (!_gameStarted || enemyPrefabs == null || enemyPrefabs.Length == 0)
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
        int quotaRaw = baseEnemiesInWave + (_waveIndex - 1) * extraEnemiesPerWave;
        _quota = Mathf.RoundToInt(quotaRaw * _difficultyMultiplier);
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

        Vector2 r   = Random.insideUnitCircle * spawnRadius;
        Vector3 pos = new Vector3(r.x, 1f, r.y);

        GameObject prefab = SelectPrefab();
        if (prefab == null)
            return false;

        var spawned = Instantiate(prefab, pos, Quaternion.identity);

        if (_waveIndex >= 4 && Random.value < 0.28f)
            spawned.AddComponent<EnemyRanged>();

        return true;
    }

    private GameObject SelectPrefab()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return null;

        int maxTypes;
        if (_waveIndex <= 2)      maxTypes = 1;
        else if (_waveIndex <= 4) maxTypes = 2;
        else                      maxTypes = enemyPrefabs.Length;

        maxTypes = Mathf.Min(maxTypes, enemyPrefabs.Length);

        int index;
        if (maxTypes == 1)
            index = 0;
        else if (maxTypes == 2)
            index = Random.value < 0.65f ? 0 : 1;
        else
        {
            float r = Random.value;
            index = r < 0.50f ? 0 : r < 0.80f ? 1 : 2;
        }

        return enemyPrefabs[index];
    }

    private static int CountEnemies()
    {
        return GameObject.FindGameObjectsWithTag("Enemy").Length;
    }
}
