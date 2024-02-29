using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

public class GunVisualRecoil : MonoBehaviour
{
    Quaternion originalRot;

    float elapsedTime = 0;

    private void Awake()
    {
        originalRot = transform.localRotation;
    }

    public IEnumerator PlayVisualRecoil(float duration, float magnitude)
    {
        float x = 1f * magnitude;

        while(elapsedTime < duration)
        {
            transform.localRotation = new Quaternion(transform.localRotation.x - x, originalRot.y, originalRot.z, originalRot.w);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        elapsedTime = 0f;

        ResetRot();
    }

    public void ResetRot()
    {
        transform.localRotation = originalRot;
    }
}
