using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CharacterSelect : NetworkBehaviour
{
    [Header("Game Manager")]
    [SerializeField] NetworkManagerGame m_gameManager;

    [Header("References")]
    [SerializeField] private GameObject characterSelectDisplay = default;
    [SerializeField] private Transform characterPreviewParent = default;
    [SerializeField] private TMP_Text characterNameText = default;
    [SerializeField] private float turnSpeed = 90f;
    //[SerializeField] private Character[] characters = default;
    [SerializeField] private GameObject characterPreviewObject = default;
    [SerializeField] private TMP_InputField playerNameInputField = default;
    [SerializeField] private GameObject m_playerPrefab;

    //Automatically enables when hosting or joining a server due to being NetworkBehaviour
    private void OnEnable()
    {
        StartCoroutine(ShowCharacterSelect());
    }

    private void Start()
    {
        m_gameManager = GameObject.FindGameObjectWithTag("NetworkManagerGame").GetComponent<NetworkManagerGame>();
    }

    IEnumerator ShowCharacterSelect()
    {
        yield return new WaitForSeconds(.1f);

        if (gameObject.GetComponent<NetworkIdentity>().isServerOnly)
        {
            characterSelectDisplay.SetActive(false);
        }
        else
        {
            characterSelectDisplay.SetActive(true);
            RefreshArmorNameText();
        }
    }

    private void Update()
    {
        characterPreviewParent.RotateAround(characterPreviewParent.position, characterPreviewParent.up, turnSpeed * Time.deltaTime);
    }

    public void RightButtonPressed()
    {
        characterPreviewObject.GetComponent<CharacterPreviewBehaviour>()?.OnRightButtonClick();
        RefreshArmorNameText();
    }

    public void LeftButtonPressed()
    {
        characterPreviewObject.GetComponent<CharacterPreviewBehaviour>()?.OnLeftButtonClick();
        RefreshArmorNameText();
    }

    public void SelectButtonPressed()
    {
        int selectedCharacterIndex = 0;
        selectedCharacterIndex = characterPreviewObject.GetComponent<CharacterPreviewBehaviour>().OnSelectButtonClick();

        string inputPlayerName = playerNameInputField.text;

        CmdSelect(selectedCharacterIndex, inputPlayerName);

        characterSelectDisplay.SetActive(false);
    }

    void RefreshArmorNameText()
    {
        characterNameText.text = characterPreviewObject.gameObject.GetComponent<CharacterPreviewBehaviour>().GetCurrentArmorName();
    }

    [Command(requiresAuthority = false)]
    void CmdSelect(int armorIndex, string playerName, NetworkConnectionToClient sender = null)
    {
        GameObject characterInstance = Instantiate(m_playerPrefab);

        PlayerBase player = characterInstance.GetComponent<PlayerBase>();
        player.SetupAppearance(armorIndex);
        player.m_name = playerName;
        //characterInstance.GetComponent<PlayerBase>().SetupAppearance(armorIndex);
        m_gameManager.OnPlayerSelectedCharacter(player);

        NetworkServer.Spawn(characterInstance, sender);
        NetworkServer.ReplacePlayerForConnection(sender, characterInstance);
    }
}