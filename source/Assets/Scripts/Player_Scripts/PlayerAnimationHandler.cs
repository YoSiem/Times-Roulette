using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationHandler : NetworkBehaviour
{
    [SerializeField] Animator m_animator;
    [SerializeField] CharacterController m_controller;

    private void Update()
    {
        if(!isOwned)
        {
            return;
        }

        m_animator.SetFloat("velocity", m_controller.velocity.magnitude);
        m_animator.SetBool("isGrounded", m_controller.gameObject.GetComponent<PlayerBase>().CheckIfGrounded());
    }
}
