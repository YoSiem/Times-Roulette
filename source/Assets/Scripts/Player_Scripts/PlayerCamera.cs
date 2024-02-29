using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    float xAxisRotation = 0;

    Vector3 originalPos;

    private void Start()
    {
        originalPos = transform.localPosition;
    }

    public void RotateY(float mouseY)
    {
        xAxisRotation -= mouseY;
        xAxisRotation = Mathf.Clamp(xAxisRotation, -85, 85);

        transform.localRotation = Quaternion.Euler(xAxisRotation, 0, 0);
    }

    public void OnPlayerDeath()
    {
        StartCoroutine(LerpRotateCamToDefault());
        StartCoroutine(LerpMoveCamBack());
    }

    public void OnPlayerSpawn()
    {
        transform.localPosition = originalPos;
    }

    IEnumerator LerpRotateCamToDefault()
    {
        float interpol = 0f;

        while(transform.localRotation != Quaternion.Euler(0, 0, 0))
        {
            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(0, 0, 0), interpol);
            interpol += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator LerpMoveCamBack()
    {
        float interpol = 0f;

        var targetOffset = new Vector3(0, 0, -2);

        while (transform.localPosition != new Vector3(0, 0, -2))
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetOffset, interpol);
            interpol += Time.deltaTime;
            yield return null;
        }
    }

    public void OnRoundConcluded()
    {
        GetComponent<Camera>().enabled = false;
    }
}
