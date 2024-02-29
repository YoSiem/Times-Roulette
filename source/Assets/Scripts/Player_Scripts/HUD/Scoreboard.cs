using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class Scoreboard : MonoBehaviour
{
    [SerializeField]
    GameObject rowPrefab;
    [SerializeField]
    Transform rowsParent;

    public List<PlayerBase> onlinePlayers = new List<PlayerBase>();

    private void Start()
    {
        //this.gameObject.SetActive(false);
    }

    void Update()
    {
        UpdateScoreBoard();
    }

    void UpdateScoreBoard()
    {
        onlinePlayers = new List<PlayerBase>(FindObjectsOfType<PlayerBase>());

        onlinePlayers.Sort((player1, player2) => player2.m_killCount.CompareTo(player1.m_killCount));

        foreach (Transform child in rowsParent)
        {
            Destroy(child.gameObject);
        }

        int rank = 1;
        foreach (var player in onlinePlayers)
        {
            GameObject newGo = Instantiate(rowPrefab, rowsParent);
            TMP_Text[] texts = newGo.GetComponentsInChildren<TMP_Text>();
            texts[0].text = rank.ToString();
            texts[1].text = player.m_name;
            texts[2].text = $"{player.m_killCount} / {player.m_deathCount}";
            rank++;
        }
    }

    void OnEnable()
    {
        Debug.Log("Scoreboard enabled");
    }

    void OnDisable()
    {
        Debug.Log("Scoreboard disabled");
    }
}
