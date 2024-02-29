using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SpawnerPlateBehaviour : MonoBehaviour
{
    [Header("Spawn List")]
    [SerializeField] List<GameObject> m_spawnableItems;

    [Header("Spawn Settings")]
    [SerializeField] float m_spawnCooldown;

    [Header("References")]
    [SerializeField] GameObject pickupParent;
    [SerializeField] AudioSource m_audioPlayer;

    GameObject m_currentlySpawnedItem;

    private void Start()
    {
        DisableAllItems();
        TrySpawnItem();
    }

    void DisableAllItems()
    {
        foreach(GameObject item in m_spawnableItems)
        {
            item.SetActive(false);
        }
    }

    void DisableCurrentlySpawnedItem()
    {
        m_currentlySpawnedItem.SetActive(false);
    }

    bool TrySpawnItem()
    {
        GameObject itemToSpawn = SelectItemFromList();
        if(itemToSpawn == null)
        {
            StartCoroutine(SpawnCooldown());
            return false;
        }

        SpawnItem(itemToSpawn);
        return true;
    }

    GameObject SelectItemFromList()
    {
        GameObject retVal;

        float pickValue = UnityEngine.Random.Range(0f, 1f);
        float totalItemProbability = 0;
        float probabilityIncrement = 1f / m_spawnableItems.Count;

        foreach(GameObject spawnable in m_spawnableItems)
        {
            totalItemProbability += probabilityIncrement;

            if (pickValue <= totalItemProbability)
            {
                retVal = spawnable;
                return retVal;
            }
        }

        return null;
    }

    void SpawnItem(GameObject itemToSpawn)
    {
        foreach(Transform child in pickupParent.transform)
        {
            if (child.Equals(itemToSpawn.transform))
            {
                child.gameObject.SetActive(true);
                m_currentlySpawnedItem = child.gameObject;
                continue;
            }

            child.gameObject.SetActive(false);
        }
    }

    IEnumerator SpawnCooldown()
    {
        yield return new WaitForSeconds(m_spawnCooldown);
        TrySpawnItem();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Player"))
        {
            return;
        }

        m_currentlySpawnedItem.GetComponent<CollectibleBehaviour>().OnPlayerEntered(other);
        DisableCurrentlySpawnedItem();
        StartCoroutine(SpawnCooldown());
    }
}
