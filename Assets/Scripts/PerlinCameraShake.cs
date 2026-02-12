using UnityEngine;

public class PerlinCameraShake : MonoBehaviour
{
    public float frequency = 25f;

    Vector3 initialLocalPos;
    float seed;
    float timeAlive;

    float timeLeft;
    float startDuration;
    float amplitude;

    void Awake()
    {
        initialLocalPos = transform.localPosition;
        seed = Random.value * 1000f;
    }

    public void Shake(float duration, float strength)
    {
        timeLeft = duration;
        startDuration = duration;
        amplitude = strength;

        timeAlive = 0f;
        seed = Random.value * 1000f;
    }

    void LateUpdate()
    {
        if (timeLeft > 0f)
        {
            timeAlive += Time.deltaTime;

            float t = timeAlive * frequency;

            float nx = Mathf.PerlinNoise(seed, t) * 2f - 1f;
            float ny = Mathf.PerlinNoise(seed + 10f, t) * 2f - 1f;

            // Smooth fade-out
            float fade = (startDuration > 0f) ? (timeLeft / startDuration) : 0f;

            Vector3 offset = new Vector3(nx, ny, 0f) * (amplitude * fade);
            transform.localPosition = initialLocalPos + offset;

            timeLeft -= Time.deltaTime;
        }
        else
        {
            transform.localPosition = initialLocalPos;
        }
    }
}
