using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDBehaviour : MonoBehaviour
{
    [SerializeField] List<GameObject> m_showOnDeathList;

    private void Start()
    {
        OnPlayerSpawn();
    }

    public void OnPlayerDeath()
    {
        foreach(Transform child in transform)
        {
            if(m_showOnDeathList.Contains(child.gameObject) )
            {
                child.gameObject.SetActive(true);
            }
            else
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    public void OnPlayerSpawn()
    {
        foreach (Transform child in transform)
        {
            if (m_showOnDeathList.Contains(child.gameObject) || child.gameObject.CompareTag("HUDDontShowOnSpawn"))
            {
                child.gameObject.SetActive(false);
            }
            else
            {
                child.gameObject.SetActive(true);
            }
        }
    }

    public void OnRoundConcluded()
    {
        foreach(Transform child in transform)
        {
            if(child.gameObject.GetComponent<Scoreboard>() != null)
            {
                Debug.Log("Showing " + child.gameObject.name);
                child.gameObject.SetActive(true);
                continue;
            }

            Debug.Log("Hiding " + child.gameObject.name);
            child.gameObject.SetActive(false);
        }
    }
}
