using UnityEngine;

public class ProceduralShootAudio : MonoBehaviour
{
    private int _sampleRate;
    private float _time;
    private float _carrierPhase;
    private float _modPhase;
    private const float Duration = 0.22f;
    private volatile bool _playing;

    private void Awake()
    {
        _sampleRate = AudioSettings.outputSampleRate;
    }

    private void Start()
    {
        _sampleRate = AudioSettings.outputSampleRate;
        var source = gameObject.AddComponent<AudioSource>();
        source.spatialBlend = 0f;
        source.volume = 0.4f;
        var clip = AudioClip.Create("_shootDriver", _sampleRate, 1, _sampleRate, false);
        source.clip = clip;
        source.loop = true;
        source.Play();
    }

    public void TriggerShot()
    {
        _time = 0f;
        _carrierPhase = 0f;
        _modPhase = 0f;
        _playing = true;
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        if (!_playing) return;

        float twoPi = 2f * Mathf.PI;

        for (int i = 0; i < data.Length; i += channels)
        {
            _time += 1f / _sampleRate;

            if (_time > Duration)
            {
                _playing = false;
                break;
            }

            float t = _time / Duration;

            // Exponential frequency sweep: sharp snap down from 1500 Hz to 220 Hz
            float freq = 1500f * Mathf.Exp(-t * 8f) + 220f;
            float modFreq = freq * 0.51f;

            _carrierPhase += freq * twoPi / _sampleRate;
            if (_carrierPhase > twoPi) _carrierPhase -= twoPi;

            _modPhase += modFreq * twoPi / _sampleRate;
            if (_modPhase > twoPi) _modPhase -= twoPi;

            // FM synthesis — modulator bends the carrier for that metallic blaster timbre
            float fm = Mathf.Sin(_carrierPhase + 2.8f * Mathf.Sin(_modPhase));

            // Soft saturation via tanh approximation to add the blaster "crack"
            float saturated = fm / (1f + Mathf.Abs(fm * 0.6f));

            float envelope = Mathf.Exp(-t * 10f);
            float value = saturated * envelope * 0.28f;

            for (int c = 0; c < channels; c++)
                data[i + c] += value;
        }
    }
}
