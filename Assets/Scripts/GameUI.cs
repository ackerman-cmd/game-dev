using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// HUD через IMGUI: тёмная панель, полоска HP, читаемые подписи — без Canvas.
/// </summary>
public class GameUI : MonoBehaviour
{
    private PlayerHealth _player;
    private EnemySpawner _spawner;
    private MeteorStrike _meteor;
    private PlayerDash _dash;
    private PlayerShield _shield;
    private PlayerSpeedUp _speedUp;
    private PlayerSlowZone _slowZone;

    private enum GameState { StartMenu, Playing, GameOver }
    private GameState _state = GameState.StartMenu;
    private float _difficultyMultiplier = 1f;

    private static Texture2D s_white;

    private static Texture2D WhiteTex
    {
        get
        {
            if (s_white == null)
            {
                s_white = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                s_white.hideFlags = HideFlags.HideAndDontSave;
                s_white.SetPixel(0, 0, Color.white);
                s_white.Apply(false, true);
            }

            return s_white;
        }
    }

    private void Start()
    {
        _player = Object.FindFirstObjectByType<PlayerHealth>();
        _spawner = Object.FindFirstObjectByType<EnemySpawner>();
        _meteor = Object.FindFirstObjectByType<MeteorStrike>();
        _dash = Object.FindFirstObjectByType<PlayerDash>();
        _shield = Object.FindFirstObjectByType<PlayerShield>();
        _speedUp = Object.FindFirstObjectByType<PlayerSpeedUp>();
        _slowZone = Object.FindFirstObjectByType<PlayerSlowZone>();

        if (_player != null)
            _player.OnDeath += HandlePlayerDeath;

        // Pause game initially for start menu
        Time.timeScale = 0f;
    }

    private void OnDestroy()
    {
        if (_player != null)
            _player.OnDeath -= HandlePlayerDeath;
    }

    private void HandlePlayerDeath()
    {
        if (_state == GameState.GameOver)
            return;
        _state = GameState.GameOver;
        Time.timeScale = 0.2f; // Slow motion death effect
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private static float UiScale => Mathf.Clamp(Screen.height / 1080f, 0.85f, 1.35f);

    private void OnGUI()
    {
        float s = UiScale;
        float pad = 18f * s;

        var textMuted = new Color(0.88f, 0.9f, 0.93f, 0.95f);
        var textAccent = new Color(0.65f, 0.82f, 0.88f, 1f);

        var labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = Mathf.RoundToInt(16f * s),
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.UpperLeft
        };
        labelStyle.normal.textColor = textMuted;

        // === START MENU ===
        if (_state == GameState.StartMenu)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            DrawTintedRect(new Rect(0f, 0f, Screen.width, Screen.height), new Color(0.04f, 0.05f, 0.08f, 0.92f));

            float w = 420f * s;
            float h = 340f * s;
            float bx = (Screen.width - w) * 0.5f;
            float by = (Screen.height - h) * 0.5f;
            DrawTintedRect(new Rect(bx, by, w, h), new Color(0.08f, 0.09f, 0.13f, 0.98f));
            DrawTintedRect(new Rect(bx, by, w, 3f), new Color(0.3f, 0.6f, 0.85f, 0.9f));
            DrawTintedRect(new Rect(bx, by + h - 3f, w, 3f), new Color(0.3f, 0.6f, 0.85f, 0.4f));

            var titleSt = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(34f * s),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            titleSt.normal.textColor = new Color(0.85f, 0.93f, 1f, 1f);
            GUI.Label(new Rect(bx, by + 22f * s, w, 52f * s), "METEOR ARENA", titleSt);

            var subSt = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(15f * s),
                alignment = TextAnchor.MiddleCenter
            };
            subSt.normal.textColor = textMuted;
            GUI.Label(new Rect(bx, by + 78f * s, w, 28f * s), "Choose difficulty to start", subSt);

            float btnW = w - 80f * s;
            float btnX = bx + 40f * s;
            float btnH = 46f * s;
            float btnY = by + 118f * s;
            float btnGap = 12f * s;

            var btnSt = new GUIStyle(GUI.skin.button) { fontSize = Mathf.RoundToInt(19f * s) };

            if (GUI.Button(new Rect(btnX, btnY, btnW, btnH), "EASY"))
                StartGame(0.7f);

            if (GUI.Button(new Rect(btnX, btnY + btnH + btnGap, btnW, btnH), "NORMAL"))
                StartGame(1.0f);

            if (GUI.Button(new Rect(btnX, btnY + (btnH + btnGap) * 2f, btnW, btnH), "HARD"))
                StartGame(1.5f);

            return;
        }

        // === GAME OVER ===
        if (_state == GameState.GameOver)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            DrawTintedRect(new Rect(0f, 0f, Screen.width, Screen.height), new Color(0.1f, 0f, 0f, 0.55f));

            float w = 360f * s;
            float h = 210f * s;
            float bx = (Screen.width - w) * 0.5f;
            float by = (Screen.height - h) * 0.5f;
            DrawTintedRect(new Rect(bx, by, w, h), new Color(0.08f, 0.09f, 0.12f, 0.97f));
            DrawTintedRect(new Rect(bx, by, w, 3f), new Color(0.85f, 0.2f, 0.2f, 0.85f));

            var goSt = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(30f * s),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            goSt.normal.textColor = new Color(0.95f, 0.3f, 0.3f, 1f);
            GUI.Label(new Rect(bx, by + 20f * s, w, 44f * s), "GAME OVER", goSt);

            int waveReached = _spawner != null ? _spawner.CurrentWave : 0;
            var subSt = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(16f * s),
                alignment = TextAnchor.MiddleCenter
            };
            subSt.normal.textColor = textMuted;
            GUI.Label(new Rect(bx, by + 68f * s, w, 30f * s), $"Waves Survived: {waveReached}", subSt);

            if (GUI.Button(new Rect(bx + 40f * s, by + 130f * s, w - 80f * s, 48f * s), "RESTART"))
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }

            return;
        }

        // === HUD (Playing) ===
        float panelW = 300f * s;
        float lineH = 22f * s;
        float barH = 7f * s;
        float y = pad;

        var titleStyle = new GUIStyle(labelStyle)
        {
            fontSize = Mathf.RoundToInt(13f * s),
            fontStyle = FontStyle.Bold
        };
        titleStyle.normal.textColor = new Color(0.55f, 0.62f, 0.7f, 1f);

        var panelRect = new Rect(pad, pad, panelW, 200f * s);
        DrawTintedRect(panelRect, new Color(0.06f, 0.07f, 0.1f, 0.78f));
        DrawTintedRect(new Rect(panelRect.x, panelRect.y, panelRect.width, 1f), new Color(0.25f, 0.55f, 0.62f, 0.55f));

        GUI.Label(new Rect(pad + 10f * s, y, panelW - 20f * s, lineH), "STATUS", titleStyle);
        y += lineH * 0.85f;

        if (_player != null)
        {
            float max = Mathf.Max(1f, _player.MaxHealth);
            float pct = Mathf.Clamp01(_player.CurrentHealth / max);

            GUI.Label(new Rect(pad + 10f * s, y, 200f * s, lineH), "Hull integrity", labelStyle);
            y += lineH * 0.9f;

            float barX = pad + 10f * s;
            float barW = panelW - 20f * s;
            DrawTintedRect(new Rect(barX, y, barW, barH), new Color(0.12f, 0.13f, 0.16f, 0.95f));
            if (pct > 0.001f)
            {
                var hpColor = Color.Lerp(new Color(0.55f, 0.22f, 0.2f), new Color(0.28f, 0.62f, 0.48f), pct);
                DrawTintedRect(new Rect(barX, y, barW * pct, barH), hpColor);
            }
            y += barH + 6f * s;

            var valStyle = new GUIStyle(labelStyle);
            valStyle.normal.textColor = textAccent;
            GUI.Label(new Rect(pad + 10f * s, y, panelW - 20f * s, lineH),
                $"{Mathf.CeilToInt(_player.CurrentHealth)} / {Mathf.CeilToInt(_player.MaxHealth)}", valStyle);
            y += lineH * 1.1f;
        }

        if (_spawner != null)
        {
            int level = Mathf.Max(1, (_spawner.CurrentWave - 1) / 3 + 1);
            var lvlStyle = new GUIStyle(labelStyle);
            lvlStyle.normal.textColor = new Color(1f, 0.8f, 0.4f, 1f);
            GUI.Label(new Rect(pad + 10f * s, y, panelW, lineH),
                $"Level {level}  (Wave {_spawner.CurrentWave})", lvlStyle);
            y += lineH * 1.05f;
        }

        if (_meteor != null)
        {
            float cd = _meteor.CooldownRemaining;
            var mStyle = new GUIStyle(labelStyle);
            mStyle.normal.textColor = cd > 0.05f ? textMuted : textAccent;
            GUI.Label(new Rect(pad + 10f * s, y, panelW, lineH),
                cd > 0.01f ? $"Orbital strike  {cd:0.0}s" : "Orbital strike  ready", mStyle);
            y += lineH * 1.05f;
        }

        if (_dash != null)
        {
            float d = _dash.CooldownRemaining;
            GUI.Label(new Rect(pad + 10f * s, y, panelW, lineH),
                d > 0.05f ? $"Dash  {d:0.0}s" : "Dash  ready", labelStyle);
            y += lineH;
        }

        if (_shield != null)
        {
            float d = _shield.CooldownRemaining;
            string sh = _shield.IsShieldActive ? "Shield  active" : (d > 0.05f ? $"Shield  {d:0.0}s" : "Shield  ready");
            GUI.Label(new Rect(pad + 10f * s, y, panelW, lineH), sh, labelStyle);
            y += lineH;
        }

        if (_speedUp != null)
        {
            float d = _speedUp.CooldownRemaining;
            string sp = _speedUp.IsActive ? "Speed Up  active" : (d > 0.05f ? $"Speed Up  {d:0.0}s" : "Speed Up  ready");
            GUI.Label(new Rect(pad + 10f * s, y, panelW, lineH), sp, labelStyle);
            y += lineH;
        }

        if (_slowZone != null)
        {
            float d = _slowZone.CooldownRemaining;
            GUI.Label(new Rect(pad + 10f * s, y, panelW, lineH),
                d > 0.05f ? $"Slow Zone  {d:0.0}s" : "Slow Zone  ready", labelStyle);
        }

        float hintH = 28f * s;
        var hintRect = new Rect(pad, Screen.height - hintH - pad, 540f * s, hintH);
        DrawTintedRect(hintRect, new Color(0.05f, 0.06f, 0.08f, 0.55f));
        var hintStyle = new GUIStyle(labelStyle)
        {
            fontSize = Mathf.RoundToInt(12f * s),
            alignment = TextAnchor.MiddleLeft
        };
        hintStyle.normal.textColor = new Color(0.65f, 0.68f, 0.72f, 0.9f);
        GUI.Label(new Rect(hintRect.x + 8f * s, hintRect.y, hintRect.width - 16f * s, hintRect.height),
            "WASD move  ·  Space — jump  ·  LMB — fire  ·  RMB — meteor  ·  V — view  ·  Q/E/F/Shift — abilities",
            hintStyle);
    }

    private void StartGame(float diffMulti)
    {
        _difficultyMultiplier = diffMulti;
        _state = GameState.Playing;
        Time.timeScale = 1f;
        
        if (_spawner != null)
            _spawner.SetDifficulty(_difficultyMultiplier);
    }

    private static void DrawTintedRect(Rect r, Color c)
    {
        var prev = GUI.color;
        GUI.color = c;
        GUI.DrawTexture(r, WhiteTex);
        GUI.color = prev;
    }
}
