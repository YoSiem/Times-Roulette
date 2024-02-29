using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Scripting.APIUpdating;

public class TogglePromptHUD : MonoBehaviour
{
    [SerializeField] RectTransform m_togglePrompt;
    [SerializeField] RectTransform m_controlsList;

    [SerializeField] float m_slideSpeed;

    bool m_isActive = false;

    PlayableDirector m_animator;

    private void Start()
    {
        m_animator = GetComponent<PlayableDirector>();
        m_animator.Play();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.P))
        {
            ToggleActiveState();
        }

        TryMoveHUD();
    }

    private void ToggleActiveState()
    {
        m_isActive = !m_isActive;
    }

    void TryMoveHUD()
    {
        if(m_isActive)
        {
            if(m_animator.time <= 1)
            {
                m_animator.time += m_slideSpeed * Time.deltaTime;
            }
        }
        else
        {
            if(m_animator.time >= 0)
            {
                m_animator.time -= m_slideSpeed * Time.deltaTime;
            }
        }

        m_animator.Evaluate();
    }
}
