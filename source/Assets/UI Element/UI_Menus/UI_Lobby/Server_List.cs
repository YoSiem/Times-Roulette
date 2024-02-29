using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



public class Server_List : MonoBehaviour
{

    [Header("Create Server Part")]
    public TMP_InputField ServerNameInput;
    public TMP_InputField PasswordInput;
    public TMP_Dropdown MapNameDropDown;
    public Button CreateButton;

    [Header("Server List Part")]
    [SerializeField] private GameObject serverRow;
    [SerializeField] private Transform serverRowParent;
    public Button RefreshButton;


    private NetworkClientStartup nClient;

    private void Awake()
    {
        nClient = FindObjectOfType<NetworkClientStartup>();

        if (nClient == null)
        {
            Debug.LogError("NetworkClientStartup not found!");
        }

        CreateButton.onClick.AddListener(OnClickServerCreate);
        RefreshButton.onClick.AddListener(Request_Update_List);

        ServerNameInput.onValueChanged.AddListener(OnInputChanged);
        PasswordInput.onValueChanged.AddListener(OnInputChanged);
        MapNameDropDown.onValueChanged.AddListener(OnDropdownValueChanged);

        nClient.ServerListUpdated.AddListener(Update_List);

        CheckButtonState();
    }

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => { return nClient.isConnected; });
        Request_Update_List();
    }

    private void OnClickServerCreate()
    {
        nClient.CreateNewServer(ServerNameInput.text.ToString(), MapNameDropDown.options[MapNameDropDown.value].text, PasswordInput.text.ToString());
    }

    private void OnInputChanged(string value)
    {
        CheckButtonState();
    }
    private void OnDropdownValueChanged(int index)
    {
        CheckButtonState();
    }


    private void CheckButtonState()
    {
        bool isDropdownValueValid = MapNameDropDown.value != 0; 

        if (string.IsNullOrWhiteSpace(ServerNameInput.text) || string.IsNullOrWhiteSpace(PasswordInput.text) || !isDropdownValueValid)
        {
            CreateButton.interactable = false;
        }
        else
        {
            CreateButton.interactable = true;
        }
    }


    public void Request_Update_List()
    {
        nClient.RequestServerList();
    }

    void Update_List()
    {
        foreach (Transform child in serverRowParent)
        {
            Destroy(child.gameObject);
        }

        foreach (var serverData in nClient.serversByPort.Values)
        {
            GameObject newServerRow = Instantiate(serverRow, serverRowParent);
            newServerRow.GetComponent<Server_Row>().Initialize(serverData);
        }
    }
}
