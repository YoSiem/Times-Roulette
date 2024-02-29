using Mono.CSharp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHolderBehaviour : MonoBehaviour
{
    public void ResetWeapons()
    {
        foreach (Transform weapon in transform)
        {
            weapon.GetComponent<GunVisualRecoil>().ResetRot();
            weapon.GetComponent<GunSystemNetwork>().OnPlayerSpawn();
            GetComponent<WeaponSwitching>().OnPlayerSpawn();
        }
    }

    public int GetCurrentlyEquippedWeaponIndex()
    {
        return GetComponent<WeaponSwitching>().GetCurrentlyEquippedWeaponIndex();
    }

    public void OnEscMenuPause()
    {
        GetComponent<WeaponSwitching>().OnEscMenuPause();

        foreach(var weapon in GetComponentsInChildren<GunSystemNetwork>())
        {
            weapon.OnEscMenuPause();
        }
    }

    public void OnEscMenuResume()
    {
        GetComponent<WeaponSwitching>().OnEscMenuResume();

        foreach (var weapon in GetComponentsInChildren<GunSystemNetwork>())
        {
            weapon.OnEscMenuResume();
        }
    }
}
