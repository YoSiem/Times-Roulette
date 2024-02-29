using Mirror;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] float mouseSensitivity;

    PlayerBase m_player;

    bool m_isPaused = false;

    [Client]
    // Update is called once per frame
    void Update()
    {
        HandleSprint();

        if(m_player == null || m_isPaused)
        {
            return;
        }

        var m_input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        Vector2 m_mouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        m_player.SetMoveDirection(m_input);

        m_player.RotateX(m_mouseInput.x * mouseSensitivity);

        m_player.RotateY(m_mouseInput.y * mouseSensitivity);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            m_player.TryJump();
        }
    }

    [Client]
    void HandleSprint()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            m_player.Sprint();
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            m_player.StopSprint();
        }
    }

    [Client]
    public void GetLocalPlayer()
    {
        m_player = NetworkClient.localPlayer.gameObject.GetComponent<PlayerBase>();
        LockMouse();
    }

    [Client]
    void LockMouse()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    [Client]
    public void OnEscMenuPause()
    {
        m_isPaused = true;

        m_player.SetMoveDirection(Vector3.zero);
        m_player.OnEscMenuPause();

    }

    [Client]
    public void OnEscMenuResume()
    {
        m_isPaused = false;
        m_player.OnEscMenuResume();
    }

    [Client]
    public void OnRoundConcluded()
    {
        enabled = false;
    }
}
