using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleBehaviour : MonoBehaviour
{
    [SerializeField] GameObject m_weaponToReload;

    public void OnPlayerEntered(Collider other)
    {
        PlayerBase player = other.gameObject.GetComponent<PlayerBase>();
        Transform weapons = player.m_WeaponHolder.transform;



        if(m_weaponToReload == null)
        {
            AddAmmoToAllWeapons(weapons);
            return;
        }

        GunSystemNetwork weapon = GetPickedUpWeapon(weapons);
        if(weapon == null)
        {
            return;
        }

        AddAmmoToWeapon(weapon);
    }

    void AddAmmoToAllWeapons(Transform weapons)
    {
        foreach (Transform weapon in weapons)
        {
            bool wasEnabled = false;

            GunSystemNetwork gunScript = weapon.GetComponent<GunSystemNetwork>();

            if (!gunScript.enabled)
            { 
                gunScript.enabled = true;
                wasEnabled = true;
            }

            if(gunScript.HasBeenPickedUp)
                gunScript.AddAmmo(gunScript.MagazineSize / 2);

            if(wasEnabled)
            {
                gunScript.enabled = false;
            }
        }
    }

    GunSystemNetwork GetPickedUpWeapon(Transform weapons)
    {
        foreach(Transform weapon in weapons)
        {
            if(weapon.gameObject.name.Equals(m_weaponToReload.name))
            {
                return weapon.GetComponent<GunSystemNetwork>();
            }
        }

        return null;
    }

    void AddAmmoToWeapon(GunSystemNetwork weapon)
    {
        bool wasEnabled = false;

        if (!weapon.enabled)
        {
            weapon.enabled = true;
            wasEnabled = true;
        }

        weapon.WasPickedUp();
        weapon.AddAmmo(weapon.MagazineSize);

        if (wasEnabled)
        {
            weapon.enabled = false;
        }
    }
}
