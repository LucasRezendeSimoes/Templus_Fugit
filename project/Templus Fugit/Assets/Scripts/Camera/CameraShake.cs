using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private bool isShaking = false;

    public IEnumerator Shake(float duration, float magnitude)
    {
        Vector3 originalPosition = transform.localPosition;

        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(originalPosition.x + x, originalPosition.y + y, originalPosition.z);

            elapsed += Time.deltaTime;

            yield return null;
        }

        transform.localPosition = originalPosition;
    }

    public IEnumerator ShakeContinuous(float intensity, float duration)
    {
        Vector3 originalPosition = transform.localPosition;

        while (true)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float offsetX = Random.Range(-1f, 1f) * intensity;
                float offsetY = Random.Range(-1f, 1f) * intensity;

                transform.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0);

                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.localPosition = originalPosition;
            yield return null;
        }
    }

    public void StopShake()
    {
        isShaking = false;
    }
}
