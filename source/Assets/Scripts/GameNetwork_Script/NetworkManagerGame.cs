using kcp2k;
using Mirror;
using Mirror.Discovery;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.UI.GridLayoutGroup;

public class NetworkManagerGame : NetworkManager
{
    public static NetworkManagerGame instance;

    [Header("UI Menu Buttons")]
    [SerializeField] GameObject m_exitButton;

    [Header("In-Game UI References")]
    //[SerializeField] NetworkTimer m_roundTimer = default;
    [SerializeField] KillFeed m_killFeed = default;

    [Header("FX References (for Reset)")]
    [SerializeField] Material m_glitchEffectMat;

    List<PlayerBase> m_playerList = new List<PlayerBase>();

    [HideInInspector]
    public bool m_roundConcluded = false;

    float m_roundStartTime;

    public override void Awake()
    {
        base.Awake();

        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        SceneManager.sceneLoaded += OnAfterSceneLoad;
    }

    static void OnAfterSceneLoad(Scene scene, LoadSceneMode loadSceneMode)
    {
        Debug.Log("OnAfterSceneLoad called");

        instance.m_exitButton = GameObject.FindGameObjectWithTag("ExitButton");
        Debug.Log("Exit Button: " + instance.m_exitButton.name);

        var go = GameObject.FindGameObjectWithTag("KillFeed");
        instance.m_killFeed = go.GetComponent<KillFeed>();
        Debug.Log("Kill Feed: " + instance.m_killFeed.gameObject.name);
    }

    IEnumerator LateGet()
    {
        yield return new WaitForSeconds(.2f);

        //var go = GameObject.FindGameObjectWithTag("RoundTimer");
        //instance.m_roundTimer = go.GetComponent<NetworkTimer>();
        //Debug.Log("Round Timer: " + instance.m_roundTimer.gameObject.name);
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        Transform startPos = GetStartPosition();
        GameObject player = startPos != null
            ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
            : Instantiate(playerPrefab);

        // instantiating a "Player" prefab gives it the name "Player(clone)"
        // => appending the connectionId is WAY more useful for debugging!
        player.name = $"{playerPrefab.name} [connId={conn.connectionId}]";
        NetworkServer.AddPlayerForConnection(conn, player);
    }

    public void OnPlayerSelectedCharacter(PlayerBase playerCharacter)
    {
        m_playerList.Add(playerCharacter);
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();

        instance.m_exitButton?.SetActive(false);

        //instance.m_roundTimer?.OnClientConnect(m_roundStartTime, Time.time);
        instance.m_killFeed?.OnClientConnect();

        GetComponent<NetworkDiscoveryHUD>().enabled = false;
    }

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        instance.m_exitButton?.SetActive(true);

        //instance.m_roundTimer?.OnClientDisconnect();
        instance.m_killFeed?.OnClientDisconnect();

        GetComponent<NetworkDiscoveryHUD>().enabled = true;

        m_glitchEffectMat.SetFloat("_intensity", 0f);
    }

    public override void OnStartServer()
    {
        //m_roundStartTime = Time.time;
        //base.OnStartServer();

        //m_roundTimer = GameObject.FindGameObjectWithTag("RoundTimer").GetComponent<NetworkTimer>();
        //m_roundTimer.OnRoundStart(this);
    }

    public void OnPlayButtonPressed()
    {
        gameObject.SetActive(true);
    }

    [ServerCallback]
    public void ConcludeRound()
    {
        foreach(var player in m_playerList)
        {
            player.RpcOnRoundConcluded();
        }

        StartCoroutine(EndGameCountdown(30));
    }

    [Server]
    public float? GetCurrentTimeLeft()
    {
        //return instance.m_roundTimer?.m_TimeLeft;
        return 30.0f;
    }

    IEnumerator EndGameCountdown(uint secondsBeforeGameEnds)
    {
        yield return new WaitForSeconds(secondsBeforeGameEnds);

        StopClient();
        StopServer();
    }

    public void OnBackToMainMenuButtonPressedClient()
    {
        StopClient();
        //GetComponent<NetworkDiscoveryHUD>().networkDiscovery.StopDiscovery();

        //m_mainMenu?.SetActive(true);
        //m_roundTimer?.gameObject.SetActive(false);
        //gameObject?.SetActive(false);
    }

    public void OnBackToMainMenuButtonPressedServer()
    {
        StopClient();
        StopServer();
        GetComponent<NetworkDiscoveryHUD>().networkDiscovery.StopDiscovery();

        //instance.m_roundTimer?.gameObject.SetActive(false);
        gameObject?.SetActive(false);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
