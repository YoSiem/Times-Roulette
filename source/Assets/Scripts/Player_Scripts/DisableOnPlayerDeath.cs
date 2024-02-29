using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableOnPlayerDeath : MonoBehaviour
{
    [SerializeField] List<GameObject> objectsToDisable;

    public void OnPlayerDeath()
    {
        foreach(GameObject obj in objectsToDisable)
        {
            obj.SetActive(false);
        }
    }

    public void OnPlayerSpawn()
    {
        foreach(GameObject obj in objectsToDisable)
        {
            obj.SetActive(true);
        }
    }
}
