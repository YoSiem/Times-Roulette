using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KillListing : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI m_killerDisplay;
    [SerializeField] TextMeshProUGUI m_killedDisplay;
    
    [SerializeField] Image m_howImageDisplay;

    private void Start()
    {
        Destroy(gameObject, 10f);
    }

    public void SetNamesAndHowImage(string killerName, string killedName, Sprite howImage)
    {
        m_killerDisplay.text = killerName;
        m_killedDisplay.text = killedName;
        m_howImageDisplay.sprite = howImage;
    }
}
