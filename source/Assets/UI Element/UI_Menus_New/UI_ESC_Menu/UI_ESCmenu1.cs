using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_ESCmenu : NetworkBehaviour
{
    [SerializeField] Canvas m_canvas;
    [SerializeField] GameObject m_player;
    bool isPaused = false;


    void Update()
    {
        if(!isOwned)
        {
            return;
        }

        if(Input.GetKeyDown(KeyCode.Escape) && isPaused == false)
        {
            ShowESCMenu();
        }
        else if(Input.GetKeyDown(KeyCode.Escape) && isPaused == true)
        {
            HideESCMenu();
        }
    }

    public void HideESCMenu()
    {
        m_canvas.enabled = false;
        isPaused = false;

        m_player.GetComponentInChildren<PlayerController>().OnEscMenuResume();

        LockMenuButtons();

        LockMouse();
    }

    private void ShowESCMenu()
    {
        m_canvas.enabled = true;
        isPaused = true;


        m_player.GetComponentInChildren<PlayerController>().OnEscMenuPause();

        UnlockMenuButtons();

        UnlockMouse();
    }

    void LockMenuButtons()
    {
        var buttons = GetComponentsInChildren<Button>();

        foreach(var button in buttons)
        {
            button.interactable = false;
        }
    }

    void UnlockMenuButtons()
    {
        var buttons = GetComponentsInChildren<Button>();

        foreach (var button in buttons)
        {
            button.interactable = true;
        }
    }

    public void OnSettingsButtonPressed()
    {
        GameObject.FindGameObjectWithTag("OptionsMenu").GetComponent<SettingsMenu>().ShowMenu();
    }

    public void OnBackToMainMenuButtonPressed()
    {
        var go = FindObjectOfType<NetworkManagerGame>();

        Debug.Log(go.name);

        if (isClientOnly)
        {
            go.OnBackToMainMenuButtonPressedClient();
        }

        if(isServer)
        {
            go.OnBackToMainMenuButtonPressedServer();
        }
    }

    void LockMouse()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void UnlockMouse()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
