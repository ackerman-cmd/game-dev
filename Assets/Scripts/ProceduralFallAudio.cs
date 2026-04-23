using UnityEngine;

public class ProceduralFallAudio : MonoBehaviour
{
    private float _time;
    private float _frequency = 800f;
    private float _phase;
    private int _sampleRate;

    private void Awake()
    {
        _sampleRate = AudioSettings.outputSampleRate;
    }

    private void Start()
    {
        var audio = gameObject.AddComponent<AudioSource>();
        audio.spatialBlend = 1f;
        audio.maxDistance = 50f;
        audio.Play();
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        if (_sampleRate == 0) return;
        for (int i = 0; i < data.Length; i += channels)
        {
            _time += 1f / _sampleRate;
            
            _frequency = Mathf.Lerp(800f, 150f, _time * 0.5f);
            float phaseIncrement = _frequency * 2f * Mathf.PI / _sampleRate;
            _phase += phaseIncrement;
            if (_phase > Mathf.PI * 2f) _phase -= Mathf.PI * 2f;

            float value = Mathf.Sin(_phase) * 0.2f;
            
            for (int c = 0; c < channels; c++)
                data[i + c] = value;
        }
    }
}
