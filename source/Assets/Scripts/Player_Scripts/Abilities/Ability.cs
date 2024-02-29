using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.LookDev;

public class Ability : ScriptableObject
{
    public new string name;
    public float cooldownTime;
    public float activeTime;
    public Modifier appliedModifier;

    public virtual bool TryActivate(GameObject parent)
    {
        return false;
    }
}
