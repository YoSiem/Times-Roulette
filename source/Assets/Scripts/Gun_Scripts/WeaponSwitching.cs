using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class WeaponSwitching : NetworkBehaviour
{
    [SerializeField] GameObject thirdPersonWeaponHolder;
    [SerializeField] TextMeshProUGUI HUDWeaponDisplayName;
    [SerializeField] Animator firstPersonTopAnimator;

    [SyncVar]
    int selectedWeapon = 0;

    [SyncVar]
    int previousSelectedWeapon = 0;

    [SyncVar]
    bool scrolledUp;

    bool isPaused;

    [ClientCallback]
    private void Start()
    {
        if(!transform.root.gameObject.GetComponent<PlayerBase>().isOwned)
        {
            return;
        }

        CmdSelectWeapon();
    }

    private void Update()
    {
        if(!isOwned)
        {
            return;
        }

        if(isPaused)
        {
            return;
        }
         
        MyInput();
    }

    [Client]
    void MyInput()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            CmdChangeWeaponIndex(selectedWeapon + 1, true);
        }

        if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            CmdChangeWeaponIndex(selectedWeapon - 1, false);
        }

        if (previousSelectedWeapon != selectedWeapon)
        {
            CmdSelectWeapon();
        }
    }

    [Command]
    void CmdChangeWeaponIndex(int index, bool scrollUp)
    { 

        foreach(Transform weapon in transform)
        {
            if(weapon.GetComponent<GunSystemNetwork>().IsReloading)
            {
                return;
            }
        }

        scrolledUp = scrollUp;

        selectedWeapon = index;
        KeepSelectedWeaponInRange();
    }

    [Command]
    void CmdSelectWeapon()
    {
        SelectWeapon();

        previousSelectedWeapon = selectedWeapon;

        RpcSelectWeapon();
    }

    [ClientRpc]
    void RpcSelectWeapon()
    {
        SelectWeapon();
        HandleArmAnimation();
    }

    void SelectWeapon()
    {
        Transform[] weapons = new Transform[transform.childCount];
        int j = 0;
        foreach (Transform weapon in transform)
        {
            weapons[j] = weapon;
            j++;
        }

        bool weaponSet = false;

        for(int i = scrolledUp ? 0 : transform.childCount - 1; i != (scrolledUp ? transform.childCount : -1); i += scrolledUp ? 1 : -1)
        { 
            if (i == selectedWeapon)
            {
                weapons[i].gameObject.SetActive(true);
                var checkedWeapon = weapons[i].gameObject.GetComponent<GunSystemNetwork>();
                checkedWeapon.enabled = true;

                weapons[i].GetComponent<GunVisualRecoil>().ResetRot();

                if(!checkedWeapon.HasBeenPickedUp)
                {
                    weapons[i].gameObject.SetActive(false);
                    checkedWeapon.enabled = false;

                    selectedWeapon += scrolledUp ? 1 : -1;


                    KeepSelectedWeaponInRange();

                    continue;
                }

                weaponSet = true;

                if (isClient)
                {
                    HUDWeaponDisplayName.text = weapons[i].gameObject.GetComponent<GunSystemNetwork>().weaponDisplayName;
                }
            }
            else
            {
                weapons[i].gameObject.SetActive(false);
                weapons[i].gameObject.GetComponent<GunSystemNetwork>().enabled = false;
            }
        }

        if(!weaponSet)
        {
            weapons[0].gameObject.SetActive(true);
            weapons[0].gameObject.GetComponent<GunSystemNetwork>().enabled = true;
            selectedWeapon = 0;
        }

        int a = 0;
        foreach (Transform weapon in thirdPersonWeaponHolder.transform)
        {
            if (a == selectedWeapon)
            {
                weapon.gameObject.SetActive(true);
            }
            else
            {
                weapon.gameObject.SetActive(false);
            }

            a++;
        }
    }

    bool KeepSelectedWeaponInRange()
    {
        if (selectedWeapon > transform.childCount - 1)
        {
            selectedWeapon = 0;
            return true;
        }

        if (selectedWeapon < 0)
        {
            selectedWeapon = transform.childCount - 1;
            return true;
        }

        return false;
    }

    void HandleArmAnimation()
    {
        if(!isOwned)
        {
            return;
        }

        foreach(GunSystemNetwork gun in transform.GetComponentsInChildren<GunSystemNetwork>())
        {
            if(!gun.gameObject.activeSelf)
            {
                continue;
            }

            switch(gun.GunType)
            {
                case GunType.PISTOL:
                    firstPersonTopAnimator.SetBool("isPistol", true);
                    firstPersonTopAnimator.SetBool("isRifle", false);
                    firstPersonTopAnimator.SetBool("isAMR", false);
                    break;

                case GunType.RIFLE:
                    firstPersonTopAnimator.SetBool("isPistol", false);
                    firstPersonTopAnimator.SetBool("isRifle", true);
                    firstPersonTopAnimator.SetBool("isAMR", false);
                    break;
                case GunType.AMR:
                    firstPersonTopAnimator.SetBool("isPistol", false);
                    firstPersonTopAnimator.SetBool("isRifle", false);
                    firstPersonTopAnimator.SetBool("isAMR", true);
                    break;
            }
        }
    }

    public int GetCurrentlyEquippedWeaponIndex()
    {
        return selectedWeapon;
    }

    public void OnPlayerSpawn()
    {
        CmdChangeWeaponIndex(0, false);
        CmdSelectWeapon();
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
