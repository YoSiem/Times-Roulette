using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Modifier : ScriptableObject
{
    public enum ModifierType
    {
        SPEED_MODIFIER,

        MODIFIER_AMOUNT
    }

    public ModifierType modifierType;
    public string modifierName;
    public float modifyAdditive;
    public float modifyMultiplicative;
}
