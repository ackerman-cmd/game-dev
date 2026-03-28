using UnityEngine;

public class ProceduralExplosionAudio : MonoBehaviour
{
    private float _time;
    private float _duration = 1.5f;
    private int _sampleRate;
    private System.Random _rand;

    private void Awake()
    {
        _sampleRate = AudioSettings.outputSampleRate;
        _rand = new System.Random();
    }

    private void Start()
    {
        var audio = gameObject.AddComponent<AudioSource>();
        audio.spatialBlend = 1f;
        audio.maxDistance = 60f;
        audio.Play();
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        if (_sampleRate == 0 || _rand == null) return;
        for (int i = 0; i < data.Length; i += channels)
        {
            _time += 1f / _sampleRate;
            float t = _time / _duration;
            if (t > 1f) t = 1f;

            float envelope = Mathf.Exp(-t * 10f);
            float noise = (float)(_rand.NextDouble() * 2.0 - 1.0);
            float value = noise * envelope * 0.8f;

            for (int c = 0; c < channels; c++)
                data[i + c] = value;
        }
    }
}
