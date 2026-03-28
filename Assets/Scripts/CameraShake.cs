using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private static CameraShake instance;
    private float shakeDuration = 0f;
    private float shakeMagnitude = 0.7f;
    private float dampingSpeed = 1.0f;
    Vector3 initialPosition;

    void Awake()
    {
        if (instance == null)
            instance = this;
    }

    void OnEnable()
    {
        initialPosition = transform.localPosition;
    }

    void Update()
    {
        if (shakeDuration > 0)
        {
            transform.localPosition = initialPosition + Random.insideUnitSphere * shakeMagnitude;
            shakeDuration -= Time.deltaTime * dampingSpeed;
        }
        else
        {
            shakeDuration = 0f;
            transform.localPosition = initialPosition;
        }
    }

    public static void TriggerShake(float duration = 0.5f, float magnitude = 0.3f)
    {
        if (instance != null)
        {
            instance.shakeDuration = duration;
            instance.shakeMagnitude = magnitude;
        }
    }
}
