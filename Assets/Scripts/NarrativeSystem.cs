using UnityEngine;

/// <summary>
/// Provides wave-contextual lore strings for the meteor-threat storyline.
/// Auto-instantiates on first access; survives within one scene lifecycle.
/// </summary>
public class NarrativeSystem : MonoBehaviour
{
    private static NarrativeSystem s_instance;

    public static NarrativeSystem Instance
    {
        get
        {
            if (s_instance == null)
            {
                s_instance = Object.FindFirstObjectByType<NarrativeSystem>();
                if (s_instance == null)
                    s_instance = new GameObject("[NarrativeSystem]").AddComponent<NarrativeSystem>();
            }
            return s_instance;
        }
    }

    public static readonly string IntroLore =
        "Year 2187 — a rogue meteor cluster has breached all orbital defences.\n" +
        "The arena is humanity's last fortified position. Survive the waves.";

    public static readonly string GameOverLore =
        "The arena has fallen. The meteor storm claims the last defender.";

    private static readonly string[] WaveLore =
    {
        "First hostile wave detected. Alien vanguard is entering the arena.",
        "Impact cluster approaching. More fragments — more hostiles.",
        "Orbital sensors confirm: a Class-4 event is now inevitable.",
        "The storm is feeding them energy. Adapt your tactics.",
        "WARNING — large fragment cluster inbound from the north sector.",
        "They are coordinating attacks from the debris field. Stay mobile.",
        "Outer perimeter shields have failed. You are the last defence.",
        "The sky is burning. Command is not responding. Hold the line.",
        "Mass impact event 40 km out. Arena containment still holding — barely.",
        "CRITICAL: doomsday-class fragment locked on this sector. Survive.",
    };

    private static readonly string[] LoopLore =
    {
        "Each wave brings new fragments and new hostiles. Keep fighting.",
        "The bombardment shows no signs of stopping. Neither will you.",
        "ALERT: meteor density increasing. Eliminate all contacts.",
        "The arena is the last defended position on the continent.",
        "Impact after impact — still standing. Keep going.",
    };

    private void Awake()
    {
        if (s_instance != null && s_instance != this) { Destroy(gameObject); return; }
        s_instance = this;
    }

    private void OnDestroy()
    {
        if (s_instance == this) s_instance = null;
    }

    public string GetWaveLore(int waveIndex)
    {
        if (waveIndex <= 0) return string.Empty;
        int idx = waveIndex - 1;
        if (idx < WaveLore.Length) return WaveLore[idx];
        return LoopLore[(idx - WaveLore.Length) % LoopLore.Length];
    }
}
