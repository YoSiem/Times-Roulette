using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class DashAbility : Ability
{


    public override bool TryActivate(GameObject parent)
    {
        PlayerBase movement = parent.GetComponent<PlayerBase>();

        if (new Vector3(movement.Velocity.x, 0, movement.Velocity.z).magnitude > 0)
        {
            movement.AddModifier(appliedModifier, activeTime);
            movement.StartCoroutine(movement.SetDashFOV(activeTime));

            PlayerSound playerSound = movement.gameObject.GetComponentInChildren<PlayerSound>();
            if(playerSound != null)
            {
                playerSound.PlayerDashed();
            }

            return true;
        }

        return false;
    }
}
