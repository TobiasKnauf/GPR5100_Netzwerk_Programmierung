using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    private bool isShaking = false;
    private Vector3 startPosition;
    private int shakesRemaining;
    private float shakeSpeed;
    private float shakeIntensity;

    public void StartShake(int _amount, float _intensity, float _speed)
    {
        if (isShaking)
            return;
        isShaking = true;
        startPosition = transform.position;
        shakesRemaining = _amount;
        shakeIntensity = _intensity;
        shakeSpeed = _speed;
        StartCoroutine(Shake());
    }

    private IEnumerator Shake()
    {
        if (shakesRemaining <= 0)
        {
            isShaking = false;
            yield break;
        }

        Vector2 rndCircle = Random.insideUnitCircle.normalized * shakeIntensity;
        transform.position += new Vector3(rndCircle.x, rndCircle.y, 0f);
        shakesRemaining--;
        yield return new WaitForSeconds(1f / shakeSpeed);
        transform.position = startPosition;
        StartCoroutine(Shake());
    }
}
