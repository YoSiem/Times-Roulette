using kcp2k;
using LiteNetLib;
using LiteNetLib.Utils;
using LoginServerMessage;
using Mirror;
using System;
using System.Collections;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkServerStartup : MonoBehaviour
{
    private const string LOGIN_SERVER_IP = "walf.gay";
    private const ushort LOGIN_SERVER_PORT = 42069;

    public bool USE_HOST = false;

    private NetManager client;
    private NetPeer loginServer;
    private EventBasedNetListener listener;
    private KcpTransport kcp;

    private string serverName;
    private string mapName;
    private int playerCount;
    private float idleTime = 0f;

    private bool shutdownOnIdle = false;

    private string game_server_ip;


    private void Start()
    {
        USE_HOST = false;

        Console.WriteLine("Network Server Startup");
        kcp = NetworkManager.singleton.GetComponent<KcpTransport>();
        ParseCommandLineArgs();

        if(SceneManager.GetSceneByName(mapName) == null)
        {
            Console.WriteLine("Map " + mapName + " not found");
            Application.Quit();
            return;
        }

        Console.WriteLine("Parsed map name as: " + mapName);

        NetworkManagerGame.instance.ServerChangeScene(mapName);

        if (USE_HOST)
        {
            Debug.Log("USE HOST ENABLED");
            InitializeNetworkComponents();
            ConnectToLoginServer();

            if(shutdownOnIdle)
            {
                StartCoroutine(CheckForIdleServer());
            }
            
            StartCoroutine(SendServerUpdateRoutine());
            StartServer();
        }
    }

    private void InitializeNetworkComponents()
    {
        listener = new EventBasedNetListener();
        client = new NetManager(listener);
        client.Start();
        listener.NetworkReceiveEvent += ServerListener_NetworkReceiveEvent;
    }

    private bool ParseCommandLineArgs()
    {
        var args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)  // -1 to avoid IndexOutOfRangeException
        {
            switch (args[i].ToLower())
            {
                case "-server":
                    USE_HOST = true;
                    break;
                case "-name":
                    serverName = args[i + 1];
                    i++;
                    break;
                case "-map":
                    mapName = args[i + 1];
                    i++;
                    break;
                case "-port":
                    kcp.port = ushort.Parse(args[i + 1]);
                    i++;
                    break;
                case "-idleshutdown":
                    shutdownOnIdle = true;
                    break;
                case "-endip":
                    game_server_ip = args[i + 1];
                    i++;
                    break;
            }
        }

        return USE_HOST;
    }

    private void Update()
    {
        if (USE_HOST)
        {
            client?.PollEvents();
        }


    }

    private void ConnectToLoginServer()
    {
        if (client == null)
            InitializeNetworkComponents();

        Console.WriteLine("Trying to connect to Login Server");
        loginServer = client.Connect(LOGIN_SERVER_IP, LOGIN_SERVER_PORT, "");
    }

    private void StartServer()
    {
        NetworkManagerGame.instance.StartServer();
    }

    private void ServerListener_NetworkReceiveEvent(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
    {
        ServerMessage message = (ServerMessage)reader.GetByte();
        switch (message)
        {
            case ServerMessage.SendServerPort:
                ushort port = (ushort)reader.GetInt();
                Debug.Log($"Received startup port {port}");
                kcp.port = port;
                NetworkManager.singleton.StartServer();
                SceneManager.LoadScene(mapName);
                SendServerInfoBackToLoginServer(port);
                break;
            case ServerMessage.RequestServerUpdate:
                SendServerInfoBackToLoginServer(kcp.port);
                break;
        }
    }

    private void SendServerInfoBackToLoginServer(int port)
    {
        playerCount = NetworkManager.singleton.numPlayers;

        var writer = new NetDataWriter();
        writer.Put((byte)ServerMessage.ServerUpdate);
        writer.Put(port);
        writer.Put(serverName);
        writer.Put(mapName);
        writer.Put(playerCount);
        writer.Put(game_server_ip);
        writer.Put(port);

        loginServer.Send(writer, DeliveryMethod.ReliableOrdered);
    }


    private IEnumerator CheckForIdleServer()
    {
        while (true)
        {
            yield return new WaitForSeconds(10);
            if (NetworkManager.singleton.numPlayers == 0)
            {
                idleTime += 10;
                if (idleTime >= 180)
                {
                    ShutdownServer();
                    yield break;  // Break out of the loop
                }
            }
            else
            {
                idleTime = 0;
            }
        }
    }

    private IEnumerator SendServerUpdateRoutine()
    {
        while (true)
        {
            SendServerInfoBackToLoginServer(kcp.port);
            yield return new WaitForSeconds(15);
        }
    }


    private void OnApplicationQuit()
    {
        ShutdownServer();
    }


    private void ShutdownServer()
    {
        var writer = new NetDataWriter();
        writer.Put((byte)ServerMessage.RemoveServerFromList);
        writer.Put(kcp.port);
        loginServer.Send(writer, DeliveryMethod.ReliableOrdered);

        NetworkManager.singleton.StopServer();
        Application.Quit();
    }


}
