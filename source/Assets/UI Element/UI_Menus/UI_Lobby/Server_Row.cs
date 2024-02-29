using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Server_Row : MonoBehaviour
{
    public TMP_Text Text_ServerName;
    public TMP_Text Text_PlayersCount;
    public TMP_Text Text_MapName;
    public Button ConnectButton;

    private NetworkClientStartup nClient;
    [SerializeField] private NetworkClientStartup.GameServer _serverInfo;


    private void Awake()
    {
        nClient = FindObjectOfType<NetworkClientStartup>();

        if (nClient == null)
        {
            Debug.LogError("NetworkClientStartup not found!");
        }
    }

    public void Initialize(NetworkClientStartup.GameServer serverInfo)
    {
        _serverInfo = serverInfo;

        Text_ServerName.text = serverInfo.Name;
        Text_PlayersCount.text = $"{serverInfo.PlayerCount.ToString()}/16";
        Text_MapName.text = serverInfo.MapName;


        ConnectButton.onClick.AddListener(OnConnectButtonClicked);
    }

    public void UpdateInfo(int playerCount)
    {
        // Im not sure if we need update Name / Map since we have no option to change them after create.
        Text_PlayersCount.text = $"{playerCount.ToString()}/16";
        _serverInfo.PlayerCount = playerCount;

    }

    private void OnConnectButtonClicked()
    {
        SceneManager.LoadScene(Text_MapName.text);
        nClient.ConnectToGameServer(_serverInfo.IP, (ushort)_serverInfo.Port, _serverInfo.MapName);
    }

}
