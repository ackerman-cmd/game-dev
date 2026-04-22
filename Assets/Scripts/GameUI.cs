using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// HUD through IMGUI: start menu with lore intro, wave-announcement banners,
/// in-game status panel with score, and a game-over screen.
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
    private bool _playerSubscribed;

    // Wave announcement banner
    private int _lastAnnouncedWave = 0;
    private float _bannerEndTime   = -1f;
    private string _bannerTitle    = "";
    private string _bannerLore     = "";
    private const float BannerDuration = 4f;

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

    private void Awake()
    {
        // Freeze immediately — before any other Start() can run the game
        Time.timeScale = 0f;
        _state = GameState.StartMenu;
    }

    private void Start()
    {
        _player   = Object.FindFirstObjectByType<PlayerHealth>();
        _spawner  = Object.FindFirstObjectByType<EnemySpawner>();
        _meteor   = Object.FindFirstObjectByType<MeteorStrike>();
        _dash     = Object.FindFirstObjectByType<PlayerDash>();
        _shield   = Object.FindFirstObjectByType<PlayerShield>();
        _speedUp  = Object.FindFirstObjectByType<PlayerSpeedUp>();
        _slowZone = Object.FindFirstObjectByType<PlayerSlowZone>();

        TrySubscribePlayer();
    }

    // Auto-creates GameUI if no instance exists in the scene.
    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCreate()
    {
        if (Object.FindFirstObjectByType<GameUI>() == null)
            new GameObject("[GameUI]").AddComponent<GameUI>();
    }

    private void OnDestroy()
    {
        if (_player != null)
            _player.OnDeath -= HandlePlayerDeath;
    }

    private void TrySubscribePlayer()
    {
        if (_playerSubscribed) return;
        if (_player == null) _player = Object.FindFirstObjectByType<PlayerHealth>();
        if (_player == null) return;
        _player.OnDeath += HandlePlayerDeath;
        _playerSubscribed = true;
    }

    private void Update()
    {
        if (_state == GameState.Playing)
        {
            // Retry missing references (handles hot-reload and init-order edge cases)
            TrySubscribePlayer();
            if (_spawner  == null) _spawner  = Object.FindFirstObjectByType<EnemySpawner>();
            if (_meteor   == null) _meteor   = Object.FindFirstObjectByType<MeteorStrike>();
            if (_dash     == null) _dash     = Object.FindFirstObjectByType<PlayerDash>();
            if (_shield   == null) _shield   = Object.FindFirstObjectByType<PlayerShield>();
            if (_speedUp  == null) _speedUp  = Object.FindFirstObjectByType<PlayerSpeedUp>();
            if (_slowZone == null) _slowZone = Object.FindFirstObjectByType<PlayerSlowZone>();

            // Fallback death detection in case the OnDeath event was missed
            if (_player != null && !_player.IsAlive)
                HandlePlayerDeath();

            // Wave announcement banner
            if (_spawner != null && _spawner.CurrentWave > _lastAnnouncedWave)
            {
                _lastAnnouncedWave = _spawner.CurrentWave;
                _bannerTitle       = $"WAVE  {_lastAnnouncedWave}";
                _bannerLore        = NarrativeSystem.Instance.GetWaveLore(_lastAnnouncedWave);
                _bannerEndTime     = Time.time + BannerDuration;
            }
        }
    }

    private void HandlePlayerDeath()
    {
        if (_state == GameState.GameOver) return;
        _state = GameState.GameOver;
        Time.timeScale = 0.2f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }

    private static float UiScale => Mathf.Clamp(Screen.height / 1080f, 0.85f, 1.35f);

    private void OnGUI()
    {
        float s   = UiScale;
        float pad = 18f * s;

        var textMuted  = new Color(0.88f, 0.90f, 0.93f, 0.95f);
        var textAccent = new Color(0.65f, 0.82f, 0.88f, 1f);

        var labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = Mathf.RoundToInt(16f * s),
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.UpperLeft
        };
        labelStyle.normal.textColor = textMuted;

        // ── START MENU ──────────────────────────────────────────────────────────
        if (_state == GameState.StartMenu)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;

            DrawRect(new Rect(0f, 0f, Screen.width, Screen.height), new Color(0.04f, 0.05f, 0.08f, 0.94f));

            float w  = 460f * s;
            float h  = 420f * s;
            float bx = (Screen.width  - w) * 0.5f;
            float by = (Screen.height - h) * 0.5f;
            DrawRect(new Rect(bx, by, w, h), new Color(0.07f, 0.08f, 0.12f, 0.98f));
            DrawRect(new Rect(bx, by, w, 3f), new Color(0.3f, 0.6f, 0.85f, 0.9f));
            DrawRect(new Rect(bx, by + h - 3f, w, 3f), new Color(0.3f, 0.6f, 0.85f, 0.35f));

            // Title
            var titleSt = new GUIStyle(GUI.skin.label)
            {
                fontSize  = Mathf.RoundToInt(34f * s),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            titleSt.normal.textColor = new Color(0.85f, 0.93f, 1f, 1f);
            GUI.Label(new Rect(bx, by + 18f * s, w, 52f * s), "METEOR ARENA", titleSt);

            // Lore intro
            var loreSt = new GUIStyle(GUI.skin.label)
            {
                fontSize  = Mathf.RoundToInt(13f * s),
                alignment = TextAnchor.MiddleCenter,
                wordWrap  = true
            };
            loreSt.normal.textColor = new Color(0.72f, 0.78f, 0.85f, 0.9f);
            GUI.Label(new Rect(bx + 20f * s, by + 76f * s, w - 40f * s, 60f * s),
                NarrativeSystem.IntroLore, loreSt);

            // Difficulty label
            var subSt = new GUIStyle(loreSt) { fontSize = Mathf.RoundToInt(14f * s) };
            subSt.normal.textColor = textMuted;
            GUI.Label(new Rect(bx, by + 148f * s, w, 26f * s), "Choose difficulty", subSt);

            float btnW   = w - 80f * s;
            float btnX   = bx + 40f * s;
            float btnH   = 46f * s;
            float btnY   = by + 180f * s;
            float btnGap = 12f * s;

            var btnSt = new GUIStyle(GUI.skin.button) { fontSize = Mathf.RoundToInt(19f * s) };

            if (GUI.Button(new Rect(btnX, btnY, btnW, btnH), "EASY"))   StartGame(0.7f);
            if (GUI.Button(new Rect(btnX, btnY + (btnH + btnGap), btnW, btnH), "NORMAL")) StartGame(1.0f);
            if (GUI.Button(new Rect(btnX, btnY + (btnH + btnGap) * 2f, btnW, btnH), "HARD"))  StartGame(1.5f);

            return;
        }

        // ── GAME OVER ────────────────────────────────────────────────────────────
        if (_state == GameState.GameOver)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;

            DrawRect(new Rect(0f, 0f, Screen.width, Screen.height), new Color(0.1f, 0f, 0f, 0.58f));

            float w  = 400f * s;
            float h  = 270f * s;
            float bx = (Screen.width  - w) * 0.5f;
            float by = (Screen.height - h) * 0.5f;
            DrawRect(new Rect(bx, by, w, h), new Color(0.08f, 0.09f, 0.12f, 0.97f));
            DrawRect(new Rect(bx, by, w, 3f), new Color(0.85f, 0.2f, 0.2f, 0.85f));

            var goSt = new GUIStyle(GUI.skin.label)
            {
                fontSize  = Mathf.RoundToInt(30f * s),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            goSt.normal.textColor = new Color(0.95f, 0.3f, 0.3f, 1f);
            GUI.Label(new Rect(bx, by + 18f * s, w, 44f * s), "GAME OVER", goSt);

            var subSt = new GUIStyle(GUI.skin.label)
            {
                fontSize  = Mathf.RoundToInt(15f * s),
                alignment = TextAnchor.MiddleCenter,
                wordWrap  = true
            };
            subSt.normal.textColor = new Color(0.70f, 0.72f, 0.76f, 0.85f);
            GUI.Label(new Rect(bx + 16f * s, by + 64f * s, w - 32f * s, 40f * s),
                NarrativeSystem.GameOverLore, subSt);

            int waveReached = _spawner != null ? _spawner.CurrentWave : 0;
            int kills       = ScoreTracker.Instance.TotalKills;
            int score       = ScoreTracker.Instance.Score;

            var statSt = new GUIStyle(subSt);
            statSt.normal.textColor = textAccent;
            GUI.Label(new Rect(bx, by + 112f * s, w, 26f * s),
                $"Waves: {waveReached}   Kills: {kills}   Score: {score}", statSt);

            if (GUI.Button(new Rect(bx + 40f * s, by + 200f * s, w - 80f * s, 48f * s), "RESTART"))
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }

            return;
        }

        // ── HUD (Playing) ────────────────────────────────────────────────────────
        float panelW = 300f * s;
        float lineH  = 22f * s;
        float barH   = 7f  * s;
        float y      = pad;

        var titleStyle = new GUIStyle(labelStyle)
        {
            fontSize  = Mathf.RoundToInt(13f * s),
            fontStyle = FontStyle.Bold
        };
        titleStyle.normal.textColor = new Color(0.55f, 0.62f, 0.70f, 1f);

        var panelRect = new Rect(pad, pad, panelW, 230f * s);
        DrawRect(panelRect, new Color(0.06f, 0.07f, 0.10f, 0.78f));
        DrawRect(new Rect(panelRect.x, panelRect.y, panelRect.width, 1f),
            new Color(0.25f, 0.55f, 0.62f, 0.55f));

        GUI.Label(new Rect(pad + 10f * s, y, panelW - 20f * s, lineH), "STATUS", titleStyle);
        y += lineH * 0.85f;

        // HP bar
        if (_player != null)
        {
            float max = Mathf.Max(1f, _player.MaxHealth);
            float pct = Mathf.Clamp01(_player.CurrentHealth / max);

            GUI.Label(new Rect(pad + 10f * s, y, 200f * s, lineH), "Hull integrity", labelStyle);
            y += lineH * 0.9f;

            float barX = pad + 10f * s;
            float barW = panelW - 20f * s;
            DrawRect(new Rect(barX, y, barW, barH), new Color(0.12f, 0.13f, 0.16f, 0.95f));
            if (pct > 0.001f)
            {
                var hpColor = Color.Lerp(new Color(0.55f, 0.22f, 0.20f), new Color(0.28f, 0.62f, 0.48f), pct);
                DrawRect(new Rect(barX, y, barW * pct, barH), hpColor);
            }
            y += barH + 6f * s;

            var valSt = new GUIStyle(labelStyle);
            valSt.normal.textColor = textAccent;
            GUI.Label(new Rect(pad + 10f * s, y, panelW - 20f * s, lineH),
                $"{Mathf.CeilToInt(_player.CurrentHealth)} / {Mathf.CeilToInt(_player.MaxHealth)}", valSt);
            y += lineH * 1.05f;
        }

        // Wave + level
        if (_spawner != null)
        {
            int level  = Mathf.Max(1, (_spawner.CurrentWave - 1) / 3 + 1);
            var lvlSt  = new GUIStyle(labelStyle);
            lvlSt.normal.textColor = new Color(1f, 0.80f, 0.40f, 1f);
            GUI.Label(new Rect(pad + 10f * s, y, panelW, lineH),
                $"Level {level}   (Wave {_spawner.CurrentWave})", lvlSt);
            y += lineH * 1.05f;
        }

        // Score
        var scoreSt = new GUIStyle(labelStyle);
        scoreSt.normal.textColor = new Color(0.72f, 0.90f, 0.60f, 1f);
        GUI.Label(new Rect(pad + 10f * s, y, panelW, lineH),
            $"Score {ScoreTracker.Instance.Score}   Kills {ScoreTracker.Instance.TotalKills}", scoreSt);
        y += lineH * 1.1f;

        // Ability cooldowns
        if (_meteor != null)
        {
            float cd  = _meteor.CooldownRemaining;
            var mSt   = new GUIStyle(labelStyle);
            mSt.normal.textColor = cd > 0.05f ? textMuted : textAccent;
            GUI.Label(new Rect(pad + 10f * s, y, panelW, lineH),
                cd > 0.01f ? $"Orbital strike  {cd:0.0}s" : "Orbital strike  READY", mSt);
            y += lineH * 1.05f;
        }

        if (_dash != null)
        {
            float d = _dash.CooldownRemaining;
            GUI.Label(new Rect(pad + 10f * s, y, panelW, lineH),
                d > 0.05f ? $"Dash  {d:0.0}s" : "Dash  READY", labelStyle);
            y += lineH;
        }

        if (_shield != null)
        {
            float d  = _shield.CooldownRemaining;
            string sh = _shield.IsShieldActive ? "Shield  ACTIVE"
                      : d > 0.05f ? $"Shield  {d:0.0}s" : "Shield  READY";
            GUI.Label(new Rect(pad + 10f * s, y, panelW, lineH), sh, labelStyle);
            y += lineH;
        }

        if (_speedUp != null)
        {
            float d   = _speedUp.CooldownRemaining;
            string sp = _speedUp.IsActive ? "Speed Up  ACTIVE"
                      : d > 0.05f ? $"Speed Up  {d:0.0}s" : "Speed Up  READY";
            GUI.Label(new Rect(pad + 10f * s, y, panelW, lineH), sp, labelStyle);
            y += lineH;
        }

        if (_slowZone != null)
        {
            float d = _slowZone.CooldownRemaining;
            GUI.Label(new Rect(pad + 10f * s, y, panelW, lineH),
                d > 0.05f ? $"Slow Zone  {d:0.0}s" : "Slow Zone  READY", labelStyle);
        }

        // ── Wave announcement banner ─────────────────────────────────────────────
        if (Time.time < _bannerEndTime)
        {
            float remaining = _bannerEndTime - Time.time;
            float elapsed   = BannerDuration - remaining;
            float alpha     = elapsed < 0.35f ? elapsed / 0.35f
                            : remaining < 0.8f ? remaining / 0.8f : 1f;

            DrawWaveBanner(alpha, s);
        }

        // ── Bottom hint bar ──────────────────────────────────────────────────────
        float hintH   = 28f * s;
        var hintRect  = new Rect(pad, Screen.height - hintH - pad, 560f * s, hintH);
        DrawRect(hintRect, new Color(0.05f, 0.06f, 0.08f, 0.55f));
        var hintSt    = new GUIStyle(labelStyle)
        {
            fontSize  = Mathf.RoundToInt(12f * s),
            alignment = TextAnchor.MiddleLeft
        };
        hintSt.normal.textColor = new Color(0.65f, 0.68f, 0.72f, 0.9f);
        GUI.Label(new Rect(hintRect.x + 8f * s, hintRect.y, hintRect.width - 16f * s, hintRect.height),
            "WASD move  ·  Space — jump  ·  LMB — fire  ·  RMB — meteor  ·  V — view  ·  Q shield  ·  E speed  ·  F slow  ·  Shift — dash",
            hintSt);
    }

    private void DrawWaveBanner(float alpha, float s)
    {
        float bw = 580f * s;
        float bh = 110f * s;
        float bx = (Screen.width  - bw) * 0.5f;
        float by = Screen.height  * 0.28f;

        DrawRect(new Rect(bx, by, bw, bh), new Color(0.05f, 0.06f, 0.10f, 0.88f * alpha));
        DrawRect(new Rect(bx, by, bw, 2f), new Color(0.9f, 0.6f, 0.1f, 0.9f * alpha));
        DrawRect(new Rect(bx, by + bh - 2f, bw, 2f), new Color(0.9f, 0.6f, 0.1f, 0.3f * alpha));

        var waveSt = new GUIStyle(GUI.skin.label)
        {
            fontSize  = Mathf.RoundToInt(30f * s),
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        waveSt.normal.textColor = new Color(1f, 0.82f, 0.25f, alpha);
        GUI.Label(new Rect(bx, by + 8f * s, bw, 44f * s), _bannerTitle, waveSt);

        var loreSt = new GUIStyle(GUI.skin.label)
        {
            fontSize  = Mathf.RoundToInt(14f * s),
            alignment = TextAnchor.MiddleCenter,
            wordWrap  = true
        };
        loreSt.normal.textColor = new Color(0.80f, 0.84f, 0.90f, alpha * 0.9f);
        GUI.Label(new Rect(bx + 20f * s, by + 56f * s, bw - 40f * s, 46f * s), _bannerLore, loreSt);
    }

    private void StartGame(float diffMulti)
    {
        _difficultyMultiplier = diffMulti;
        _state = GameState.Playing;
        Time.timeScale = 1f;

        if (_spawner != null)
            _spawner.SetDifficulty(_difficultyMultiplier);
    }

    private static void DrawRect(Rect r, Color c)
    {
        var prev  = GUI.color;
        GUI.color = c;
        GUI.DrawTexture(r, WhiteTex);
        GUI.color = prev;
    }
}
