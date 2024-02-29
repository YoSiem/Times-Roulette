using kcp2k;
using LiteNetLib;
using LiteNetLib.Utils;
using LoginServerMessage;
using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class NetworkClientStartup : MonoBehaviour
{
    public bool UseHost = false;

    public struct GameServer
    {
        public string Name;
        public string MapName;
        public int PlayerCount;
        public string IP;
        public int Port;
    }

    private NetManager client;
    private NetPeer loginServer;
    private EventBasedNetListener listener;

    private static string ipToConnectTo = "walf.gay";
    private static ushort portToConnectTo = 42069;

    public bool isConnected
    {
        get
        {
            return loginServer != null && loginServer.ConnectionState == ConnectionState.Connected;
        }
    }

    [SerializeField] public Dictionary<int, GameServer> serversByPort = new Dictionary<int, GameServer>();
    [SerializeField] NetworkManagerGame networkManager;

    public UnityEvent ServerListUpdated;

    private void Start()
    {
        foreach (var arg in Environment.GetCommandLineArgs())
        {
            if (arg == "-server") return;
        }

        listener = new EventBasedNetListener();
        listener.NetworkReceiveEvent += OnNetworkReceiveEvent;


        if (UseHost)
        {
            ConnectToLoginServer(ipToConnectTo, portToConnectTo);
        }
    }

    private void OnDestroy()
    {
        client?.Stop();
    }

    private void Update()
    {
        if (!UseHost) return;
        if (client != null)
        {
            //Debug.Log("pulling");
            client.PollEvents();
        }
    }

    private void OnNetworkReceiveEvent(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
    {
        if (reader == null) return;
        ServerMessage message = (ServerMessage)reader.GetByte();

        Debug.Log(message.ToString());

        switch (message)
        {
            case ServerMessage.SendServerList:
                serversByPort.Clear();

                int count = reader.GetInt();
                for (int i = 0; i < count; i++)
                {
                    var gameserver = new GameServer();
                    int key = reader.GetInt();
                    gameserver.Name = reader.GetString();
                    gameserver.MapName = reader.GetString();
                    gameserver.PlayerCount = reader.GetInt();
                    gameserver.IP = reader.GetString();
                    gameserver.Port = reader.GetInt();

                    if (!serversByPort.ContainsKey(key))
                    {
                        serversByPort.Add(key, gameserver);
                    }
                    else
                    {
                        serversByPort[key] = gameserver;
                    }


                }

                if(ServerListUpdated != null)
                {
                    ServerListUpdated.Invoke();
                }

                break;
            case ServerMessage.InvalidPassword:     //Add here later a windows with this Notice.
                Debug.LogError("Fick Dich du Penner");
                break;
        }
    }

    public void CreateNewServer(string name, string mapName, string password)
    {
        var writer = new NetDataWriter();
        writer.Put((byte)ServerMessage.CreateNewServer);
        writer.Put(name);
        writer.Put(mapName);
        writer.Put(password);
        loginServer.Send(writer, DeliveryMethod.ReliableOrdered);
    }


    private void ConnectToLoginServer(string IP, ushort port)
    {
        client = new NetManager(listener);
        client.Start();
        Debug.Log("Trying to connect to Login Server");
        loginServer = client.Connect(IP, port, string.Empty);
        listener.NetworkReceiveEvent += OnNetworkReceiveEvent;

        if (loginServer == null)
        {
            Debug.LogError("Can't connect to login server");
        }
    }

    public void ConnectToGameServer(string IP, ushort port, string sceneName)
    {
        //NetworkManager.singleton.networkAddress = IP;
        //NetworkManager.singleton.GetComponent<KcpTransport>().Port = port;
        //NetworkManager.singleton.StartClient();

        NetworkManagerGame.instance.networkAddress = IP;
        NetworkManagerGame.instance.GetComponent<KcpTransport>().Port = port;
        NetworkManagerGame.instance.StartClient();
        //networkManager.nextSceneToLoad = sceneName;
    }




    public void RequestServerList()
    {
        var writer = new NetDataWriter();
        writer.Put((byte)ServerMessage.RequestServerList);
        loginServer.Send(writer, DeliveryMethod.ReliableOrdered);
    }


}
