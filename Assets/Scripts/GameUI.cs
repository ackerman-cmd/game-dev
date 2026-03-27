using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Lightweight HUD + game over using IMGUI (no Canvas setup required).
/// </summary>
public class GameUI : MonoBehaviour
{
    private PlayerHealth _player;
    private EnemySpawner _spawner;
    private MeteorStrike _meteor;

    private bool _gameOver;

    private void Start()
    {
        _player = UnityEngine.Object.FindFirstObjectByType<PlayerHealth>();
        _spawner = UnityEngine.Object.FindFirstObjectByType<EnemySpawner>();
        _meteor = UnityEngine.Object.FindFirstObjectByType<MeteorStrike>();

        if (_player != null)
            _player.OnDeath += HandlePlayerDeath;
    }

    private void OnDestroy()
    {
        if (_player != null)
            _player.OnDeath -= HandlePlayerDeath;
    }

    private void HandlePlayerDeath()
    {
        if (_gameOver)
            return;
        _gameOver = true;
        Time.timeScale = 0f;
    }

    private void OnGUI()
    {
        const float pad = 16f;
        var labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 18 };

        if (_player != null)
        {
            GUI.Label(new Rect(pad, pad, 420f, 28f),
                $"HP: {Mathf.CeilToInt(_player.CurrentHealth)} / {Mathf.CeilToInt(_player.MaxHealth)}",
                labelStyle);
        }

        if (_spawner != null)
        {
            var waveLabel = _spawner.CurrentWave <= 0 ? "Wave: …" : $"Wave: {_spawner.CurrentWave}";
            GUI.Label(new Rect(pad, pad + 28f, 420f, 28f), waveLabel, labelStyle);
        }

        if (_meteor != null)
        {
            float cd = _meteor.CooldownRemaining;
            string meteorLine = cd > 0.01f
                ? $"Meteor: {cd:0.0}s"
                : "Meteor: ready";
            GUI.Label(new Rect(pad, pad + 56f, 420f, 28f), meteorLine, labelStyle);
        }

        if (!_gameOver)
            return;

        GUI.Box(new Rect(0f, 0f, Screen.width, Screen.height), GUIContent.none);
        var w = 280f;
        var h = 140f;
        var box = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);
        GUILayout.BeginArea(box);
        GUILayout.Label("Game Over", labelStyle);
        GUILayout.Space(8f);
        if (GUILayout.Button("Restart", GUILayout.Height(36f)))
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        GUILayout.EndArea();
    }
}
