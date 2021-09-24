using UnityEngine;

public class PlatformMovement : MonoBehaviour
{
    Vector3 StartPosition;
    float Period;
    float Amplitude;
    float StartOffset;

    void Start()
    {
        Period = 3f;
        Amplitude = 0.1f;
        StartOffset = Random.Range(0, Period);
        StartPosition = transform.position;
    }

    void Update()
    {
        float LapCount = (Time.time + StartOffset) / Period;
        float AngRad = (LapCount) * Mathf.PI * 2;
        Vector3 Movement = Vector3.up * Mathf.Sin(AngRad) * Amplitude;
        transform.position = StartPosition + Movement;
    }
}
