using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillFeed : MonoBehaviour
{
    public static KillFeed instance;
    [SerializeField] GameObject m_killListingWithImagePrefab;
    [SerializeField] Sprite[] m_howImages;

    private void Start()
    {
        instance = this;
    }

    public void AddNewKillListingWithHowImage(string killer, string killed, int howIndex)
    {
        GameObject temp = Instantiate(m_killListingWithImagePrefab, transform);
        temp.transform.SetSiblingIndex(0);
        KillListing tempListing = temp.GetComponent<KillListing>();
        tempListing.SetNamesAndHowImage(killer, killed, m_howImages[howIndex]);
    }

    public void OnClientConnect()
    {
        gameObject.SetActive(true);
    }

    public void OnClientDisconnect()
    {
        gameObject.SetActive(false);
    }
}
