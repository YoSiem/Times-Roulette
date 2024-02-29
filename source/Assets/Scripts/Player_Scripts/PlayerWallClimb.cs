using Mirror.Examples.Benchmark;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerWallClimb : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform m_orientation;
    [SerializeField] CharacterController m_characterController;
    [SerializeField] PlayerBase m_playerMovement;

    [SerializeField] LayerMask m_whatIsWall;

    [Header("Climbing Variables")]
    [SerializeField] float m_maxClimbTime;
    [SerializeField] float m_maxClimbSpeed;

    float m_climbSpeed;
    float m_climbTimer;

    [Header("Climb Jump Variables")]
    [SerializeField] float m_climbJumpUpForce;
    [SerializeField] float m_climbJumpBackForce;

    [SerializeField] KeyCode m_jumpKey = KeyCode.Space;
    [SerializeField] int m_maxClimbJumps;

    int m_climbJumpsLeft;

    [Header("Detection Variables")]
    [SerializeField] float m_detectionLength;
    [SerializeField] float m_sphereCastRadius;
    [SerializeField] float m_maxWallLookAngle;

    float m_wallLookAngle;

    RaycastHit m_frontWallHit;
    bool m_wallFront;

    Transform m_lastWall;
    Vector3 m_lastWallNormal;

    [SerializeField] float m_minWallNormalAngleChange;


    [Header("Abilities to Disable during Wallclimb")]
    [SerializeField] AbilityHolder[] abilityHolders;

    #region SyncVars
    Vector3 m_playerHorizontalMovement;
    bool m_climbing = false;
    #endregion

    private void Update()
    {
        CheckIfWallInFront();
        DetermineWallClimbState();

        if (m_climbing)
        {
            ApplyClimbingMovement();
        }
    }

    void CheckIfWallInFront()
    {
        m_wallFront = Physics.SphereCast(transform.position, m_sphereCastRadius, m_orientation.forward, out m_frontWallHit, m_detectionLength, m_whatIsWall);
        m_wallLookAngle = Vector3.Angle(m_orientation.forward, -m_frontWallHit.normal);

        bool newWall = m_frontWallHit.transform != m_lastWall || Mathf.Abs(Vector3.Angle(m_lastWallNormal, m_frontWallHit.normal)) > m_minWallNormalAngleChange;

        if((m_wallFront && newWall) || m_characterController.isGrounded)
        {
            m_climbTimer = m_maxClimbTime;
            m_climbJumpsLeft = m_maxClimbJumps;

            m_playerMovement.ResetDoubleJump();
        }
    }

    void DetermineWallClimbState()
    {
        // State 1 - Climbing
        if(m_wallFront && Input.GetKey(KeyCode.W) && m_wallLookAngle < m_maxWallLookAngle)
        {
            if(!m_climbing && m_climbTimer > 0)
            {
                StartClimbing();
            }

            // Timer
            if(m_climbTimer > 0)
            {
                m_climbTimer -= Time.deltaTime;
            }

            if(m_climbTimer < 0)
            {
                StopClimbing();
            }
        }

        // State 2 - None
        else
        {
            if(m_climbing)
            {
                StopClimbing();
            }
        }

        if(m_wallFront && Input.GetKeyDown(m_jumpKey) && m_climbJumpsLeft > 0)
        {
            m_playerMovement.TryJump();
            
            m_climbJumpsLeft--;
        }
    }

    void StartClimbing()
    {
        m_climbing = true;
        m_playerMovement.TakeAwayControl();

        //Vector3 currentHorizontalMovement = new Vector3(m_characterController.velocity.x, 0f, m_characterController.velocity.z);
        GetPlayerHorizontalVelocity();

        // Climb Speed will be relative to the horizontal speed which the player moves into the wall with
        m_climbSpeed = m_playerHorizontalMovement.magnitude;
        m_climbSpeed = Mathf.Clamp(m_climbSpeed, -m_maxClimbSpeed, m_maxClimbSpeed);

        m_lastWall = m_frontWallHit.transform;
        m_lastWallNormal = m_frontWallHit.normal;

        foreach(AbilityHolder ability in abilityHolders)
        {
            ability.enabled = false;
        }
    }

    void ApplyClimbingMovement()
    {
        m_characterController.Move(new Vector3(0, m_climbSpeed * Time.deltaTime, 0));
    }

    void GetPlayerHorizontalVelocity()
    {
        m_playerHorizontalMovement = new Vector3(m_characterController.velocity.x, 0f, m_characterController.velocity.z);
    }
    void StopClimbing()
    {
        m_climbing = false;
        m_playerMovement.ReturnControl();

        foreach (AbilityHolder ability in abilityHolders)
        {
            ability.enabled = true;
        }
    }

    //[Command]
    //void CmdApplyClimbingMovement()
    //{
    //    m_characterController.Move(new Vector3(0, m_climbSpeed * Time.deltaTime, 0));
    //}

    //[Command]
    //void CmdStartClimbing()
    //{
    //    m_climbing = true;
    //}

    //[Command]
    //void CmdStopClimbing()
    //{
    //    m_climbing = false;
    //}

}
