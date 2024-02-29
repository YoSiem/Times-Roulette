using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NetworkTimer : NetworkBehaviour
{
    [SerializeField] float m_roundTime = 300f;

    [SerializeField] TextMeshProUGUI m_timerText;

    NetworkManagerGame m_gameManager;

    [SyncVar]
    float m_timeLeft;

    public float m_TimeLeft
    {
        get => m_timeLeft;
    }

    private void Awake()
    {
        enabled = false;
    }

    private void Update()
    {
        if (isServer)
        {
            m_timeLeft -= Time.deltaTime;
        }

        int minuteNumber = (int)m_timeLeft / 60;
        int secondsNumber = (int)m_timeLeft % 60;

        if (m_timeLeft < 0)
        {
            //gameObject.SetActive(false);
            enabled = false;

            if (isServer)
            {
                NetworkManagerGame.instance.ConcludeRound();
            }
        }

        m_timerText.text = secondsNumber < 10 ? (minuteNumber + ":" + "0" + secondsNumber) : (minuteNumber + ":" + secondsNumber);
    }

    [Server]
    public void OnRoundStart(NetworkManagerGame gameManager)
    {
        gameObject.SetActive(true);

        m_gameManager = gameManager;

        m_timeLeft = m_roundTime;

        Debug.Log("Enabled timer in RoundStart");
        enabled = true;
    }

    [Client]
    public void OnClientConnect(float roundStartTime, float currentServerTime)
    {
        //Debug.Log("Time is reduced by " + (currentServerTime - roundStartTime));
        //m_timeLeft = m_roundTime - (currentServerTime - roundStartTime);
        gameObject.SetActive(true);
        enabled = true;
    }

    [Client]
    public void OnClientDisconnect()
    {
        enabled = false;
        //gameObject.SetActive(false);
    }
}
