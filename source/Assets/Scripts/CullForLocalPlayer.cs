using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CullForLocalPlayer : NetworkBehaviour
{
    private void OnEnable()
    {
        if (isOwned)
        {
            gameObject.SetActive(false);
        }
    }
}
