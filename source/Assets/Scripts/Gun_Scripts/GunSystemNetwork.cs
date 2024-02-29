using Mirror;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum GunType
{
    PISTOL = 0,
    RIFLE,
    AMR,

    GUNTYPE_AMOUNT
}

public class GunSystemNetwork : NetworkBehaviour
{
    public enum FireMode { Raycast, Projectile }

    [Header("Gun Stats")]
    public string weaponDisplayName;
    public int damage;
    public float timeBetweenShooting, spread, range, reloadTime, timeBetweenShots;
    public int magazineSize, bulletsPerTap, maxAmmo;
    public bool allowButtonHold;
    [SerializeField] bool hasBeenPickedUp = false;
    [SerializeField] GunType gunType = GunType.RIFLE;

    public GunType GunType
    {
        get 
        { 
            return gunType; 
        }
    }

    [Header("Projectile Settings")]
    public GameObject bulletPrefab;
    public float bulletSpeed;

    [Header("References")]
    public Camera fpsCam;
    public Transform attackPoint;
    public RaycastHit rayHit;
    public LayerMask whatIsEnemy;
    [SerializeField] GameObject firstPersonArms;
    [SerializeField] PlayerBase Owner;

    [Header("UI Element")]
    public Slider reloadSlider;
    [SerializeField] Image m_weaponIcon;
    [SerializeField] bool showArms;
    [SyncVar] private float reloadProgress = 0;

    [Header("Audio & SFX")]
    [SerializeField] AudioSource m_audioPlayer;
    [SerializeField] AudioSource m_reloadAudioPlayer;
    [SerializeField] AudioClip m_shotSound;
    [SerializeField] AudioClip m_reloadSound;
    [SerializeField] AudioClip m_hitMarkerSound;
    [SerializeField] AudioClip m_noAmmoClick;

    [Header("Fire Type")]
    public FireMode fireMode;

    [Header("Graphics")]
    public GameObject muzzleFlash;
    [SerializeField] GameObject m_bulletImpactParticle;
    public CameraShake camShake;
    public float camShakeMagnitude, camShakeDuration;
    public TextMeshProUGUI text;
    //[SerializeField] LineRenderer bulletTrail;

    [Header("Animator Ref")]
    [SerializeField] Animator[] firstPersonAnimator;

    private bool shooting, readyToShoot, reloading, aiming;

    private bool isPaused;

    public bool HasBeenPickedUp
    {
        get
        {
            return hasBeenPickedUp;
        }
    }

    public bool IsReloading
    {
        get
        {
            return reloading;
        }
    }

    public int MagazineSize
    {
        get
        {
            return magazineSize;
        }
    }

    [SyncVar(hook = nameof(OnBulletsLeftUpdated))]
    private int bulletsLeft;
    [SyncVar]
    private int bulletsShot;
    [SyncVar(hook = nameof(OnTotalBulletsLeftUpdated))]
    private int totalBulletsLeft;

    private Coroutine cachedCoroutine;

    #region ClientSide

    private void Awake()
    {
        bulletsLeft = magazineSize;
        totalBulletsLeft = magazineSize;
        readyToShoot = true;

#if UNITY_EDITOR
        hasBeenPickedUp = true;
#endif
    }


    private void Update()
    {
        if (!isLocalPlayer) return;

        MyInput();
        text.SetText($"{bulletsLeft} \n {totalBulletsLeft}");

        reloadSlider.value = reloadProgress;
    }

    private void OnEnable()
    {
        if(m_weaponIcon == null)
        {
            return;
        }

        m_weaponIcon.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        if(m_weaponIcon == null)
        {
            return;
        }

        m_weaponIcon.gameObject.SetActive(false);
    }

    private void MyInput()
    {
        if(Owner.IsDead)
        {
            return;
        }

        if(isPaused)
        {
            return;
        }

        shooting = allowButtonHold ? Input.GetKey(KeyCode.Mouse0) : Input.GetKeyDown(KeyCode.Mouse0);
        aiming = Input.GetMouseButton(1);
        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !reloading && totalBulletsLeft > 0)
        {
            CmdReload();
        }

        if (readyToShoot && shooting && !reloading)
        {
            if(bulletsLeft <= 0)
            {
                PlayNoAmmoClickSound();
                return;
            }

            readyToShoot = false;

            if (muzzleFlash != null)
            {

                Instantiate(muzzleFlash, attackPoint.position, attackPoint.rotation);
            }

            if (bulletsPerTap == 1)
            {
                double syncTimeNow = NetworkTime.time;
                Vector3 shotOffset = Owner.Velocity;
                CmdShoot(shotOffset, syncTimeNow, false);
            }
            else
                CmdBurst();
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

    private void PlayNoAmmoClickSound()
    {
        m_audioPlayer.Stop();
        m_audioPlayer.volume = 1f;
        m_audioPlayer.PlayOneShot(m_noAmmoClick);
    }

    private void ResetShot()
    {
        foreach (Animator animator in firstPersonAnimator)
        {
            animator.SetBool("isShooting", false);
        }

        readyToShoot = true;
    }

    public void WasPickedUp()
    {
        hasBeenPickedUp = true;
    }

    [ClientRpc]
    private void RpcShoot(Vector3 trailStart, Vector3 trailEnd, bool shouldDrawTrail)
    {
        GetComponent<GunVisualRecoil>().ResetRot();

        // Only draw trail if shouldDrawTrail is true
        if (shouldDrawTrail)
        {
            // Here we are using object pooling instead of instantiating a new object
            LineRenderer trail = ObjectPooler.Instance.SpawnFromPool("BulletTrail", trailStart, Quaternion.identity).GetComponent<LineRenderer>();
            trail.positionCount = 2;
            trail.SetPosition(0, trailStart);
            trail.SetPosition(1, trailEnd);

            Instantiate(m_bulletImpactParticle, trailEnd, Quaternion.identity);
        }

        StartCoroutine(camShake.Shake(camShakeDuration, camShakeMagnitude));
        if (GetComponent<GunVisualRecoil>() != null)
        {
            StartCoroutine(GetComponent<GunVisualRecoil>().PlayVisualRecoil(camShakeDuration, camShakeMagnitude * 10));
        }

        //ObjectPooler.Instance.SpawnFromPool("MuzzleFlash", attackPoint.position, Quaternion.identity);

        foreach (Animator animator in firstPersonAnimator)
        {
            animator.SetBool("isShooting", true);
        }

        bulletsLeft--;
        bulletsShot--;

        Invoke("ResetShot", timeBetweenShooting);
        //if (bulletsShot > 0 && bulletsLeft > 0) CmdShoot();
    }

    [ClientRpc]
    private void RpcReload()
    {
        reloadSlider.gameObject.SetActive(true);
        foreach (Animator animator in firstPersonAnimator)
        {
            animator.SetBool("isReloading", true);
        }
    }

    [ClientRpc]
    private void RpcReloadFinished()
    {
        int reloadAmount = magazineSize - bulletsLeft;
        if (totalBulletsLeft >= reloadAmount)
        {
            bulletsLeft += reloadAmount;
            totalBulletsLeft -= reloadAmount;
        }

        else
        {
            bulletsLeft += totalBulletsLeft;
            totalBulletsLeft = 0;
        }

        ResetDefaultWeapon();

        reloading = false;
        reloadSlider.gameObject.SetActive(false);
        reloadSlider.value = 0;

        foreach (Animator animator in firstPersonAnimator)
        {
            animator.SetBool("isReloading", false);
        }
    }

    private void ResetDefaultWeapon()
    {
        if(gameObject.name.Equals("p_pistol_01 Variant"))
        {
            totalBulletsLeft = magazineSize;
            hasBeenPickedUp = true;
        }
    }

    [ClientRpc]
    void RpcPlayShotSound()
    {
        if (m_audioPlayer != null && m_shotSound != null)
        {
            //m_audioPlayer.clip = m_shotSound;
            m_audioPlayer.volume = .25f;
            m_audioPlayer.pitch = Random.Range(.9f, 1.1f);
            m_audioPlayer.PlayOneShot(m_shotSound);
        }
    }

    [ClientRpc]
    void RpcPlayReloadSound()
    {
        if (m_reloadSound != null)
        {
            m_reloadAudioPlayer.clip = m_reloadSound;
            m_reloadAudioPlayer.volume = .7f;
            m_reloadAudioPlayer.Play();
        }
    }

    private void OnBulletsLeftUpdated(int oldBulletsLeft, int newBulletsLeft)
    {
        // Validation
        if (newBulletsLeft < 0 || newBulletsLeft > magazineSize)
        {
            Debug.LogError("Invalid bullet count");
            return;
        }

        bulletsLeft = newBulletsLeft;
    }

    private void OnTotalBulletsLeftUpdated(int oldTotalBulletsLeft, int newTotalBulletsLeft)
    {
        if(newTotalBulletsLeft < 0)
        {
            totalBulletsLeft = 0;
        }

        else if(newTotalBulletsLeft > maxAmmo)
        {
            totalBulletsLeft = maxAmmo;
        }

        else
        {
            totalBulletsLeft = newTotalBulletsLeft;
        }
        Debug.Log("Total bullets left: " + totalBulletsLeft);
    }

    [ClientRpc]
    void RpcSoundHitMarker()
    {
        if(!isOwned)
        {
            return;
        }

        m_audioPlayer.PlayOneShot(m_hitMarkerSound);

        if (cachedCoroutine != null)
        {
            StopCoroutine(cachedCoroutine);
        }

        cachedCoroutine = StartCoroutine(Owner.FlashHitMarker());
    }
    #endregion

    #region Commands
    [Command]
    private void CmdShoot(Vector3 shotOffset, double syncTimeStart, bool muteShotSound)
    {
        Shoot(shotOffset, syncTimeStart, muteShotSound);
    }

    [Command]
    private void CmdBurst()
    {
        StartCoroutine(BurstCoroutine(bulletsPerTap));
    }

    IEnumerator BurstCoroutine(int shootCount)
    {
        double syncTimeNow = NetworkTime.time;
        Vector3 shotOffset = Owner.Velocity;
        bool muteShotSound = false;
        for (int i = 0; i < bulletsPerTap; i++)
        {
            Shoot(shotOffset, syncTimeNow, muteShotSound);
            muteShotSound = true;
        }
        yield return null;
    }

    [Server]
    void Shoot(Vector3 shotOffset, double syncTimeStart, bool muteShotSound)
    {
        if (bulletsLeft <= 0)
        {
            Debug.LogError("No bullets left");
            return;
        }

        readyToShoot = false;

        float currentSpread = aiming ? spread / 2 : spread;
        float x = Random.Range(-currentSpread, currentSpread);
        float y = Random.Range(-currentSpread, currentSpread);

        Vector3 direction = fpsCam.transform.forward + new Vector3(x, y, 0);
        Vector3 trailStart = attackPoint.position;
        Vector3 trailEnd = Vector3.zero;
        bool shouldDrawTrail = false;

        Owner.gameObject.layer = 8;
        if (fireMode == FireMode.Raycast)
        {
            if (Physics.Raycast(fpsCam.transform.position, direction, out rayHit, range, whatIsEnemy))
            {
                trailEnd = rayHit.point;

                Owner.gameObject.layer = 0;

                //Is Hit gameObject a Player and if so, check if the player didn't by chance shoot themselves
                if (rayHit.collider.gameObject.CompareTag("Player") && (rayHit.collider.gameObject.transform.root != this.transform.root))
                {
                    rayHit.collider.GetComponent<PlayerBase>().TakeDamage(damage, Owner);
                    RpcSoundHitMarker();
                }
            }
            else
            {
                trailEnd = attackPoint.position + fpsCam.transform.forward * 5000;
            }
            shouldDrawTrail = true; // Only draw trail in Raycast mode
        }
        else if (fireMode == FireMode.Projectile)
        {
            // Here we are using object pooling instead of instantiating a new object
            //GameObject bulletObject = ObjectPooler.Instance.SpawnFromPool("Bullet", attackPoint.position, Quaternion.identity);

            GameObject bulletObject;

            if (Physics.Raycast(fpsCam.transform.position, direction, out rayHit, range, whatIsEnemy))
            {
                bulletObject = Instantiate(bulletPrefab, attackPoint.position, attackPoint.rotation);
                bulletObject.transform.LookAt(rayHit.point);
            }
            else
            {
                bulletObject = Instantiate(bulletPrefab, attackPoint.position, attackPoint.rotation);
                bulletObject.transform.LookAt(fpsCam.transform.position + fpsCam.transform.forward * 5000);
            }

            Spawn(bulletObject, Owner.gameObject);

            Missile bulletObjectMissile = bulletObject.GetComponent<Missile>();
            if (bulletObjectMissile != null)
            {
                bulletObjectMissile.SetupMissile(netIdentity, Owner);
            }
            //bulletObject.GetComponent<Rigidbody>().velocity = direction.normalized * bulletSpeed;
        }

        Owner.gameObject.layer = 0;

        double ping = NetworkTime.time - syncTimeStart;
        shotOffset = shotOffset * (float)ping * 2;
        RpcShoot(trailStart + shotOffset, trailEnd, shouldDrawTrail);

        if (!muteShotSound)
            RpcPlayShotSound();
    }

    [Command]
    private void CmdReload()
    {
        if (!reloading)
        {
            reloading = true;
            StartCoroutine(ReloadCoroutine());
            RpcReload();
            RpcPlayReloadSound();
        }
    }

    private IEnumerator ReloadCoroutine()
    {
        float reloadStartTime = Time.time;

        while (Time.time - reloadStartTime < reloadTime)
        {
            reloadProgress = (Time.time - reloadStartTime) / reloadTime;
            yield return null;
        }

        RpcReloadFinished();
        reloading = false;
        reloadProgress = 0f; // reset progress
    }

    public void OnPlayerSpawn()
    {
        hasBeenPickedUp = false;
        CmdOnPlayerSpawn();
        ResetDefaultWeapon();
    }

    [Command]
    public void CmdOnPlayerSpawn()
    {
        hasBeenPickedUp = false;
        bulletsLeft = magazineSize;
        ResetDefaultWeapon();
    }

    [ClientCallback]
    public void AddAmmo(int amount)
    {
        Debug.Log("Gun: Added ammo to " + gameObject.name);
        if(totalBulletsLeft + amount > maxAmmo)
        {
            totalBulletsLeft = maxAmmo;
            return;
        }

        totalBulletsLeft += amount;
    }

    #endregion

    #region Server-Side

    [Server]
    void Spawn(GameObject bulletObject, GameObject objectOwner)
    {
        NetworkServer.Spawn(bulletObject, objectOwner);
    }

    #endregion

}
