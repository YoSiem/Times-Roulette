using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSound : MonoBehaviour
{
    [SerializeField] CharacterController characterController;

    [Header("Audio Clips")]
    [SerializeField] AudioClip moveSound;
    [SerializeField] AudioClip jumpSound;
    [SerializeField] AudioClip dashSound;
    [SerializeField] AudioClip deathSound;
    [SerializeField] AudioClip deathExplosionSound;

    [Header("Audio Sources")]
    [SerializeField] AudioSource movementSource;
    [SerializeField] AudioSource jumpSource;
    [SerializeField] AudioSource damageTakenSource;
    [SerializeField] AudioSource deathSource;

    // Indicates whether the player jumped in the last update cycle
    private bool playerJumpedLastFrame = false;

    // Indicates whether the player dashed in the last update cycle
    private bool playerDashedLastFrame = false;

    // Maximum speed at which the dash sound will be played
    private float dashVelocityThreshold = 30.0f;

    // Update is called once per frame
    void Update()
    {
        Vector3 controllerHorizontalVelocity = new Vector3(characterController.velocity.x, 0, characterController.velocity.z);

        // Depending on the speed of the character, play different sounds
        if (characterController.isGrounded && !movementSource.isPlaying)
        {
            if (controllerHorizontalVelocity.magnitude > 2.1f)
            {
                movementSource.time = 0f;
                movementSource.clip = moveSound;
                movementSource.Play();
            }
        }

        if(controllerHorizontalVelocity.magnitude < 2.1f || !characterController.isGrounded)
        {
            movementSource.Stop();
        }

        // If the player has jumped, play the jump sound
        if (playerJumpedLastFrame)
        {
            jumpSource.time = 0f;
            jumpSource.clip = jumpSound;
            jumpSource.pitch = Random.Range(.9f, 1.1f);
            jumpSource.Play();
            playerJumpedLastFrame = false;
        }

        // If the player has dashed and his speed exceeds the set threshold, play the dash sound
        if (characterController.velocity.magnitude > dashVelocityThreshold && controllerHorizontalVelocity.magnitude > characterController.velocity.y && playerDashedLastFrame)
        {
            deathSource.clip = dashSound;
            deathSource.time = .4f;
            deathSource.Play();
            playerDashedLastFrame = false;
        }
    }

    // Use this function to invoke a jump (needs to be called from another script)
    public void PlayerJumped()
    {
        playerJumpedLastFrame = true;
    }

    public void PlayerDashed()
    {
        playerDashedLastFrame = true;
    }

    public void PlayerTookDamage()
    {
        damageTakenSource.pitch = Random.Range(.8f, 1.2f);
        damageTakenSource.PlayOneShot(damageTakenSource.clip);
    }

    public void PlayerDies()
    {
        deathSource.pitch = 1f;
        deathSource.time = 0f;
        deathSource.PlayOneShot(deathExplosionSound);
        deathSource.PlayOneShot(deathSound);
    }
}