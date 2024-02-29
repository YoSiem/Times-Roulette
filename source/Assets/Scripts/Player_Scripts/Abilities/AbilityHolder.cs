using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityHolder : MonoBehaviour
{
    public Ability ability;
    float cooldownTime;
    float activeTime;

    bool isPaused;

    enum AbilityState
    {
        READY,
        ACTIVE,
        COOLDOWN
    }

    AbilityState currentState = AbilityState.READY;

    public KeyCode key;

    // Update is called once per frame
    void Update()
    {
        if(isPaused)
        {
            return;
        }

        switch(currentState)
        {
            case AbilityState.READY:
                if(Input.GetKeyDown(key))
                {
                    if (ability.TryActivate(gameObject))
                    {
                        currentState = AbilityState.ACTIVE;
                        activeTime = ability.activeTime;
                    }
                }
                break;

            case AbilityState.ACTIVE:
                if(activeTime > 0)
                {
                    activeTime -= Time.deltaTime;
                }
                else
                {
                    currentState = AbilityState.COOLDOWN;
                    cooldownTime = ability.cooldownTime;
                }
                break;

            case AbilityState.COOLDOWN:
                if(cooldownTime > 0)
                {
                    cooldownTime -= Time.deltaTime;
                }
                else
                {
                    currentState = AbilityState.READY;
                }
                break;
        }
    }

    public void OnEscMenuPause()
    {
        isPaused = true;
    }

    public void OnEscMenuResume()
    {
        isPaused = false;
    }
}
