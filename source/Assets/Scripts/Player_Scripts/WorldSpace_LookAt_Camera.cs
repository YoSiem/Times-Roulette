using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldSpace_LookAt_Camera : MonoBehaviour
{
    void Update()
    {
        var lookPosition = transform.position + (transform.position - Camera.main.transform.position);
        transform.LookAt(lookPosition);
    }
}
