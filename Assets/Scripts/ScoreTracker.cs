using UnityEngine;

public class ScoreTracker : MonoBehaviour
{
    private static ScoreTracker s_instance;

    public static ScoreTracker Instance
    {
        get
        {
            if (s_instance == null)
            {
                s_instance = Object.FindFirstObjectByType<ScoreTracker>();
                if (s_instance == null)
                    s_instance = new GameObject("[ScoreTracker]").AddComponent<ScoreTracker>();
            }
            return s_instance;
        }
    }

    public int TotalKills { get; private set; }
    public int Score      { get; private set; }

    private void Awake()
    {
        if (s_instance != null && s_instance != this) { Destroy(gameObject); return; }
        s_instance = this;
    }

    private void OnDestroy()
    {
        if (s_instance == this) s_instance = null;
    }

    public void RegisterKill(int points)
    {
        TotalKills++;
        Score += points;
    }
}
