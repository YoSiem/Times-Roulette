using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;
using Mirror;
using Mono.CSharp;
using System.Linq;
using TMPro;
using UnityEngine.UI;

public class PlayerBase : NetworkBehaviour
{
    [Header("Movement Variables")]
    [SerializeField] float m_baseSpeed;
    [SerializeField] float m_jumpHeight;
    [SerializeField] float m_acceleration;
    [SerializeField] float m_movementDampGround;
    [SerializeField] float m_movementDampAir;
    [SerializeField] float m_playerGravity;

    [Header("Sprint Speed Modifier")]
    [SerializeField] Modifier m_sprintModifier;

    [Header("Player Camera FOV")]
    [SerializeField] float m_defaultCameraFOV;
    [SerializeField] float m_sprintCameraFOV;
    [SerializeField] float m_dashCameraFOV;

    [Header("References")]
    [SerializeField] Transform m_orientation;
    [SerializeField] Camera m_playerCamera;
    [SerializeField] GameObject m_spinePivot;
    [SerializeField] GameObject m_playerController;
    [SerializeField] GameObject m_weaponHolder;
    [SerializeField] Animator m_thirdPersonAnimator;
    [SerializeField] Animator m_firstPersonAnimatorTop;
    [SerializeField] Animator m_firstPersonAnimatorBottom;
    [SerializeField] GameObject m_scoreboard;
    [SerializeField] List<TrailRenderer> m_playerTrails;
    [SerializeField] TextMeshProUGUI m_playerDisplayName;

    [Header("HUD & Post-Processing")]
    [SerializeField] GameObject m_playerHUD;
    [SerializeField] TextMeshProUGUI m_healthDisplay;
    [SerializeField] TMP_Text m_KD_Display;
    [SerializeField] HealtBar m_healthSlider;
    [SerializeField] Image m_hitMarker;
    [SerializeField] Image m_damageVignette;
    [SerializeField] Material m_glitchEffectMat;
    [SerializeField] GameObject m_damageSpotParent;
    [SerializeField] List<Sprite> m_damageOilSpots;
    [SerializeField] GameObject m_volumeObject;

    [Header("VFX")]
    [SerializeField] GameObject m_deathEffect;

    [Header("Player Models")]
    [SerializeField] GameObject m_firstPersonModel;
    [SerializeField] GameObject m_thirdPersonModel;

    [Header("Player Statistics")]
    [SerializeField] int m_maxHealth = 100;
    [SyncVar] public string m_name = "YoSiem";
    [SyncVar] public int m_killCount;
    [SyncVar] public int m_deathCount;
    [SyncVar] public int m_assistCount;
    [SyncVar] public int m_currentHealth;

    [Header("Damage Attribution")]
    private Dictionary<PlayerBase, float> attackers = new Dictionary<PlayerBase, float>();
    private PlayerBase lastAttacker;
    private float assistTimeWindow = 10f; // Time window for an attack to count as an assist

    [Header("Spawn Points")]
    private Transform[] spawnPoints;

    public GameObject m_WeaponHolder
    {
        get
        {
            return m_weaponHolder;
        }
    }


    [SyncVar] int m_chosenArmorIndex;

    #region Internal Variables
    CharacterController m_controller;
    CapsuleCollider m_collider;

    List<Modifier> m_modifiers;

    Vector3 movementInput;
    
    private Vector3 velocity;
    public Vector3 Velocity
    {
        get
        {
            return velocity;
        }
    }

    float targetCameraFOV;
    float yVelocity = 0;
    float m_appliedSpeed;

    bool isDead;
    public bool IsDead
    {
        get
        {
            return isDead;
        }
    }

    bool canDoubleJump = true;
    bool hasControl = true;

    PlayerSound m_playerSound;

    #endregion

    #region Start-Methods

    void Start()
    {
        SetupPlayer();
        InitialSpawn();
    }

    #endregion

    #region Update-Methods

    [ClientCallback]
    void Update()
    {
        ApplyGravity();

        if (!canDoubleJump)
        {
            TryResetDoubleJump();
        }

        if(m_playerCamera.fieldOfView != targetCameraFOV)
        {
            UpdateCameraFOV();
        }



        if (UnityEngine.Input.GetKeyDown(KeyCode.Tab))
        {
            m_scoreboard.gameObject.SetActive(true);
        }
        if (UnityEngine.Input.GetKeyUp(KeyCode.Tab))
        {
            m_scoreboard.gameObject.SetActive(false);
        }
    }

    private void FixedUpdate()
    {
        if (!isOwned)
        {
            return;
        }

        Move();
    }

    #endregion

    #region Client-Side

    [Client]
    public void SetMoveDirection(Vector3 input)
    {
        movementInput = input.normalized;
    }

    [Client]
    void CalculateVelocity(Vector3 input)
    {
        if (input.magnitude > 0 && hasControl)
        {
            velocity = new Vector3(velocity.x, 0, velocity.z);
            velocity = Vector3.Lerp(velocity, (m_orientation.forward * input.z + m_orientation.right * input.x).normalized * m_appliedSpeed, m_acceleration * Time.deltaTime);
            velocity -= Vector3.down * yVelocity;
        }
        else
        {
            bool isGrounded = CheckIfGrounded();
            if (isGrounded)
                velocity = Vector3.Lerp(velocity, -Vector3.down * yVelocity, m_movementDampGround * Time.deltaTime);
            else
            {
                velocity = new Vector3(velocity.x, 0, velocity.z);
                velocity = Vector3.Lerp(velocity, Vector3.zero, m_movementDampAir * Time.deltaTime);
                velocity -= Vector3.down * yVelocity;
            }
        }
    }

    [Client]
    void Move()
    {
        CalculateVelocity(movementInput);
        m_controller.Move(velocity * Time.fixedDeltaTime);
    }

    [ClientCallback]
    public bool CheckIfGrounded()
    {
        bool retVal = false;

        var yCheckLength = - .1f;

        var frontRight = new Vector3(m_collider.radius, yCheckLength, m_collider.radius);
        var frontLeft = new Vector3(-m_collider.radius, yCheckLength, m_collider.radius);
        var backRight = new Vector3(m_collider.radius, yCheckLength, -m_collider.radius);
        var backLeft = new Vector3(-m_collider.radius, yCheckLength, -m_collider.radius);

        retVal = Physics.Raycast(transform.position, frontRight, m_collider.radius * 1.5f) || Physics.Raycast(transform.position, frontLeft, m_collider.radius * 1.5f) || Physics.Raycast(transform.position, backRight, m_collider.radius * 1.5f) || Physics.Raycast(transform.position, backLeft, m_collider.radius * 1.5f);

        return retVal;
    }

    [Client]
    void ApplyGravity()
    {
        if(isDead)
        {
            return;
        }

        if (!hasControl)
        {
            yVelocity = m_controller.velocity.y;
            return;
        }

        bool isGrounded = CheckIfGrounded();
        if (isGrounded && yVelocity < 0)
        {
            yVelocity = -2f;
        }
        else
        {
            yVelocity += m_playerGravity * Time.deltaTime;
            //yVelocity += Physics.gravity.y * Time.deltaTime;
        }
    }

    [Client]
    public void TryJump()
    {
        if(!hasControl)
        {
            return;
        }

        //if-condition moved into Command-method, since client-side controller didn't set "isGrounded" accordingly
        if (m_controller.isGrounded)
        {
            yVelocity = Mathf.Sqrt(-2.0f * m_playerGravity * m_jumpHeight);
            m_playerSound.PlayerJumped();
        }
        else if (canDoubleJump)
        {
            canDoubleJump = false;
            yVelocity = Mathf.Sqrt(-2.0f * m_playerGravity * m_jumpHeight);
            m_playerSound.PlayerJumped();
        }
    }

    [ClientCallback]
    void TryResetDoubleJump()
    {
        if (CheckIfGrounded())
        {
            canDoubleJump = true;
        }
    }

    [Client]
    public void RotateX(float mouseX)
    {
        transform.Rotate(Vector3.up * mouseX);
    }

    [Client]
    public void RotateY(float mouseY)
    {
        if(isDead)
        {
            return;
        }

        m_playerCamera.GetComponent<PlayerCamera>().RotateY(mouseY);
        CmdRotateY(mouseY);
    }

    void SetupPlayer()
    {
        m_controller = GetComponent<CharacterController>();
        m_controller.enabled = true;
        m_collider = GetComponent<CapsuleCollider>();
        m_modifiers = new List<Modifier>();
        m_appliedSpeed = m_baseSpeed;
        targetCameraFOV = m_defaultCameraFOV;
        m_playerSound = GetComponentInChildren<PlayerSound>();
        m_healthSlider.SetMaxHealth(m_maxHealth);
        m_healthSlider.SetHealt(m_maxHealth);
        m_playerDisplayName.text = m_name;
        

        GetSpawnPoints();

        if (!isOwned)
        {
            HideFirstPersonModel();
            LocalSetupPlayerAppearance();
            m_playerHUD.SetActive(false);
        }
        else
        {
            CmdResetHealth();
            HideThirdPersonModel();
            SetupPlayerWeapons();
            SetupPlayerCamera();
            m_playerHUD.SetActive(true);
            gameObject.layer = 8;

            LocalSetupPlayerAppearance();

            SetupPlayerController();
        }

        isDead = false;
    }

    private void GetSpawnPoints()
    {
        spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint").Select(go => go.transform).ToArray();
    }

    [Client]
    private void SetupPlayerController()
    {
        GameObject go = Instantiate(m_playerController, transform);
        go.GetComponent<PlayerController>().GetLocalPlayer();
    }

    [Client]
    private void SetupPlayerWeapons()
    {
        //turn off spatial sounds for player-owned weapons
        foreach (Transform childs in m_firstPersonModel.GetComponentsInChildren<Transform>())
        {
            if (!childs.gameObject.CompareTag("Weapon"))
            {
                continue;
            }

            DisableSpatialAudio(childs);
        }
    }

    [Client]
    private void SetupPlayerCamera()
    {
        m_playerCamera.enabled = true;
        m_playerCamera.gameObject.GetComponent<AudioListener>().enabled = true;
    }

    [Client]
    private static void DisableSpatialAudio(Transform entity)
    {
        entity.gameObject.GetComponent<AudioSource>().spatialBlend = 0;
    }

    [Client]
    private void HideThirdPersonModel()
    {
        //layer 7 = cullForPlayer
        //Cull third person model for local player
        foreach (Transform childs in m_thirdPersonModel.GetComponentsInChildren<Transform>())
        {
            childs.gameObject.layer = 7;
        }

        m_firstPersonModel.SetActive(true);
    }

    [ClientCallback]
    private void HideFirstPersonModel()
    {
        //Cull first person model for other players
        foreach (Transform childs in m_firstPersonModel.GetComponentsInChildren<Transform>())
        {
            childs.gameObject.layer = 7;
        }
        m_thirdPersonModel.SetActive(true);
    }

    [Client]
    public void Sprint()
    {
        if(m_controller.velocity.magnitude < 4f)
        {
            return;
        }

        targetCameraFOV = m_sprintCameraFOV;
        AddModifier(m_sprintModifier, 1000f);
    }

    [Client]
    public void StopSprint()
    {
        targetCameraFOV = m_defaultCameraFOV;
        m_modifiers.Clear();
        ApplyModifiers();
    }

    [Client]
    public void AddModifier(Modifier modifier, float activeTime)
    {
        m_modifiers.Add(modifier);
        StartCoroutine(RemoveModifier(modifier, activeTime));
        ApplyModifiers();
    }

    [Client]
    void ApplyModifiers()
    {
        m_appliedSpeed = m_baseSpeed;

        float additiveSpeedMod = 0;
        float multiplicativeSpeedMod = 1;

        foreach (Modifier mod in m_modifiers)
        {
            if (mod.modifierType == Modifier.ModifierType.SPEED_MODIFIER)
            {
                additiveSpeedMod += mod.modifyAdditive;
                multiplicativeSpeedMod *= mod.modifyMultiplicative;
            }
        }

        m_appliedSpeed = (m_appliedSpeed + additiveSpeedMod) * multiplicativeSpeedMod;
    }

    [Client]
    IEnumerator RemoveModifier(Modifier modifierToRemove, float activeTime)
    {
        if (!m_modifiers.Contains(modifierToRemove))
        {
            yield break;
        }

        yield return new WaitForSeconds(activeTime);

        m_modifiers.Remove(modifierToRemove);
        ApplyModifiers();
    }

    [Client]
    void ClearModifiers()
    {
        m_modifiers.Clear();
        ApplyModifiers();
    }

    [Client]
    void UpdateCameraFOV()
    {
        m_playerCamera.fieldOfView = Mathf.Lerp(m_playerCamera.fieldOfView, targetCameraFOV, 20 * Time.deltaTime);
    }

    [Client]
    public IEnumerator SetDashFOV(float activeTime)
    {
        float additionalFOVtime = .1f;

        targetCameraFOV = m_dashCameraFOV;

        yield return new WaitForSeconds(activeTime + additionalFOVtime);

        targetCameraFOV = m_defaultCameraFOV;
    }

    [Client]
    public void TakeAwayControl()
    {
        hasControl = false;
    }

    [Client]
    public void ReturnControl()
    {
        velocity = m_controller.velocity;
        hasControl = true;
    }

    [ClientCallback]
    public void ResetDoubleJump()
    {
        canDoubleJump = true;
    }

    [Client]
    public void GetStats()
    {
        // Get player statistics
        Debug.Log($"Kills: {m_killCount} Deaths: {m_deathCount} Assists: {m_assistCount}");
    }

    [Client]
    public IEnumerator FlashHitMarker()
    {
        m_hitMarker.color = new Color(1, 1, 1, 1);

        float fadeSpeed = 5f;

        while(m_hitMarker.color.a > 0)
        {
            m_hitMarker.color = Color.Lerp(m_hitMarker.color, new Color(1, 1, 1, 0), fadeSpeed * Time.deltaTime);
            yield return null;
        }
    }

    [Client]
    IEnumerator FlashDamageVignette(float currentHealth)
    {
        m_damageVignette.color = new Color(1, 1, 1, 1);
        m_glitchEffectMat.SetFloat("_intensity", 1f);

        float fadeSpeed = 5f;
        while (m_damageVignette.color.a > (m_maxHealth - currentHealth) / m_maxHealth && m_damageVignette.color.a != 0)
        {
            m_damageVignette.color = Color.Lerp(m_damageVignette.color, new Color(1, 1, 1, 0), fadeSpeed * Time.deltaTime);
            m_glitchEffectMat.SetFloat("_intensity", Mathf.Lerp(m_glitchEffectMat.GetFloat("_intensity"), 0f, fadeSpeed * Time.deltaTime));
            yield return null;
        }
        StopAllCoroutines();
    }

    [ClientCallback]
    void LocalSetupPlayerAppearance()
    {
        m_thirdPersonModel.GetComponent<ArmorChooser>()?.ChangeArmorTo(m_chosenArmorIndex);
        m_firstPersonModel.GetComponent<ArmorChooserFirstPerson>()?.ChangeArmorTo(m_chosenArmorIndex);
    }

    [ClientCallback]
    public void OnEscMenuPause()
    {
        m_playerHUD.GetComponent<Canvas>().enabled = false;
        m_weaponHolder.GetComponent<WeaponHolderBehaviour>().OnEscMenuPause();
        GetComponent<AbilityHolder>().OnEscMenuPause();
    }

    [ClientCallback]
    public void OnEscMenuResume()
    {
        m_playerHUD.GetComponent<Canvas>().enabled = true;
        m_weaponHolder.GetComponent<WeaponHolderBehaviour>().OnEscMenuResume();
        GetComponent<AbilityHolder>().OnEscMenuResume();
    }

    #endregion

    #region Command (Client -> Server)

    [Command]
    void CmdRotateY(float mouseY)
    {
        if (m_playerCamera != null)
        {
            m_playerCamera.GetComponent<PlayerCamera>().RotateY(mouseY);
        }

        //m_spinePivot.GetComponent<PlayerCamera>().RotateY(mouseY);

        RpcRotateY(mouseY);
    }

    [Command]
    private void CmdStartRespawnCoroutine()
    {
        StartCoroutine(RespawnCoroutine());
    }

    [Command]
    void CmdResetHealth()
    {
        m_currentHealth = m_maxHealth;
    }

    [Command]
    public void CmdTakeDamage(int amount, PlayerBase source)
    {
        TakeDamage(amount, source);
    }

    #endregion

    #region RPC-Calls
    [ClientRpc]
    void RpcPlayerDeath(string killerName, int usedWeapon)
    {
        hasControl = false;
        isDead = true;

        yVelocity = 0;

        if (isOwned)
        {
            CmdStartRespawnCoroutine();
            m_firstPersonModel.GetComponent<DisableOnPlayerDeath>().OnPlayerDeath();
            m_hitMarker.color = new Color(1, 1, 1, 0);
            ClearModifiers();
        }
        else
        {
            StartCoroutine(TogglePlayerTrails());
            m_thirdPersonModel.GetComponent<DisableOnPlayerDeath>().OnPlayerDeath();
        }

        Instantiate(m_deathEffect, transform.position, transform.rotation);
        m_playerSound.PlayerDies();
        UpdateKillDeathHUD();   //Added KD Update here too to be sure its work xd - Patryk

        KillFeed.instance.AddNewKillListingWithHowImage(killerName, m_name, usedWeapon);

        if(NetworkManagerGame.instance.m_roundConcluded)
        {
            return;
        }

        m_playerHUD.GetComponent<HUDBehaviour>().OnPlayerDeath();
        m_playerCamera.GetComponent<PlayerCamera>().OnPlayerDeath();
    }

    [Client]
    IEnumerator TogglePlayerTrails()
    {
        yield return new WaitForSeconds(.1f);

        foreach(var trail in m_playerTrails)
        {
            trail.Clear();
            trail.enabled = !trail.enabled;
        }
    }

    [ClientRpc]
    void RpcRotateY(float mouseY)
    {
        if(isOwned)
        {
            return;
        }

        //m_spinePivot.GetComponent<PlayerCamera>().RotateY(mouseY);
    }

    [ClientRpc]
    void RpcDamageTaken(float currentHealth, PlayerBase attacker)
    {
        if(!isOwned)
        {
            return;
        }

        UpdateHealthHUD(currentHealth, attacker);
        ClearModifiers();
        m_playerSound.PlayerTookDamage();
    }

    [ClientRpc]
    void RpcRespawn()
    {
        if(NetworkManagerGame.instance.m_roundConcluded)
        {
            UpdateHealthHUD(m_currentHealth, null);
            return;
        }

        int spawnIndex = Random.Range(0, spawnPoints.Length);
        var newPosition = spawnPoints[spawnIndex].position;

        RespawnOnSpawnPoint(newPosition);

        if (isOwned)
        {
            m_firstPersonModel.GetComponent<DisableOnPlayerDeath>().OnPlayerSpawn();
            m_weaponHolder.GetComponent<WeaponHolderBehaviour>().ResetWeapons();
        }
        else
        {
            m_thirdPersonModel.GetComponent<DisableOnPlayerDeath>().OnPlayerSpawn();
            StartCoroutine(TogglePlayerTrails());
        }

        m_playerHUD.GetComponent<HUDBehaviour>().OnPlayerSpawn();
        m_playerCamera.GetComponent<PlayerCamera>().OnPlayerSpawn();
        UpdateHealthHUD(m_currentHealth, null);
        UpdateKillDeathHUD();   //Added KD update - Patryk

        isDead = false;
        hasControl = true;

        CmdResetIsDead();
    }

    [ClientRpc]
    void RpcPlayerKill()
    {
        if(!isOwned)
        {
            return;
        }

        UpdateKillDeathHUD();
    }

    [Client]
    void UpdateHealthHUD(float currentHealth, PlayerBase attacker)
    {
        if(isOwned)
        {
            string currentHealthDisplayText = currentHealth.ToString();
            m_healthDisplay.text = currentHealthDisplayText;

            m_healthSlider.SetHealt((int) currentHealth);
            UpdateHUDColor(currentHealth);
            UpdateVolumeHpFX(currentHealth);

            StartCoroutine(FlashDamageVignette(currentHealth));

            if (attacker != null)
            {
                m_playerCamera.GetComponent<CameraShake>().Shake(.1f, .1f);
                var attackerRelativePos = attacker.transform.position - transform.position;
            }

        }
    }
    [Client]
    void UpdateKillDeathHUD()
    {

        if(isOwned)
        {
            m_KD_Display.text = $"{m_killCount}          {m_deathCount}";
        }
    }

    [Client]
    private void UpdateHUDColor(float currentHealth)
    {
        m_playerHUD.GetComponent<HUDColorChanger>().UpdateHUDColor(currentHealth / m_maxHealth);
    }

    [Client]
    private void UpdateVolumeHpFX(float currentHealth)
    {
        m_volumeObject.GetComponent<UpdatePlayerVolume>().SetFilmgrainIntensity((m_maxHealth - currentHealth) / m_maxHealth);
    }

    #endregion

    #region Server-Side

    [Server]
    public void TakeDamage(int damage, PlayerBase attacker, bool damageFromWorld = false)
    { 
        if (isDead)
            return;

        if (attacker == null && !damageFromWorld)
        {
            Debug.LogError("Attacker is null in cmd shoot");
            return;
        }
        else
        {
            Debug.Log("Damage: " + damage + "\n HP:" + m_currentHealth);
        }

        m_currentHealth -= damage;
        RpcDamageTaken(m_currentHealth, attacker);

        if (!damageFromWorld)
        {
            lastAttacker = attacker;
            if (attackers.ContainsKey(attacker))
            {
                attackers[attacker] = Time.time;
            }
            else
            {
                attackers.Add(attacker, Time.time);
            }

            // Prune any attackers outside of the assist time window
            List<PlayerBase> toRemove = new List<PlayerBase>();
            foreach (KeyValuePair<PlayerBase, float> kvp in attackers)
            {
                if (Time.time - kvp.Value > assistTimeWindow)
                {
                    toRemove.Add(kvp.Key);
                }
            }
            foreach (PlayerBase player in toRemove)
            {
                attackers.Remove(player);
            }
        }
        else
        {
            if (lastAttacker != null && Time.time - attackers[lastAttacker] > 5f)
            {
                lastAttacker = null;
                attackers.Clear();
            }
        }

        if (m_currentHealth <= 0)
        {
            isDead = true;
            m_deathCount++;

            if (lastAttacker != null)
            {
                lastAttacker.PlayerKill();
            }
            foreach (PlayerBase player in attackers.Keys)
            {
                if (player != lastAttacker)
                {
                    player.PlayerAssist();
                }
            }

            if(lastAttacker != null)
                RpcPlayerDeath(lastAttacker.m_name, lastAttacker.GetCurrentlyEquippedWeaponIndex());
            else
            {
                RpcPlayerDeath("", 0);
            }
        }
    }

    [Server]
    int GetCurrentlyEquippedWeaponIndex()
    {
        return m_weaponHolder.GetComponent<WeaponHolderBehaviour>().GetCurrentlyEquippedWeaponIndex();
    }


    [Server]
    public void PlayerKill()
    {
        m_killCount++;

        if (m_killCount >= 30)
        {
            NetworkManagerGame.instance.ConcludeRound();
        }

        RpcPlayerKill();
    }

    [Server]
    public void PlayerAssist()
    {
        m_assistCount++;
    }

    [Server]
    void Respawn()
    {
        // Respawn logic
        m_currentHealth = m_maxHealth;

        RpcRespawn();
    }

    void InitialSpawn()
    {
        int spawnIndex = Random.Range(0, spawnPoints.Length);
        m_controller.enabled = false;
        transform.position = spawnPoints[spawnIndex].position;
        m_controller.enabled = true;
    }

    [Server]
    public void SetupAppearance(int armorIndex)
    {
        m_chosenArmorIndex = armorIndex;
        m_thirdPersonModel.GetComponent<ArmorChooser>()?.ChangeArmorTo(armorIndex);
        m_firstPersonModel.GetComponent<ArmorChooserFirstPerson>()?.ChangeArmorTo(armorIndex);
    }

    [Client]
    public void RespawnOnSpawnPoint(Vector3 newPosition) 
    {
        if(!isOwned)
        {
            return;
        }

        m_controller.enabled = false;
        transform.position = newPosition;

        m_controller.enabled = true;
    }

    [Command]
    void CmdResetIsDead()
    {
        isDead = false;
    }

    [ClientRpc]
    public void RpcAddVelocity(Vector3 velocityToAdd)
    {
        if (!isOwned) 
        { 
            return; 
        }
        
        velocity += new Vector3(velocityToAdd.x / 2, 0, velocityToAdd.z / 2);
        yVelocity = velocityToAdd.y;
    }



    #endregion



    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(5f);
        Respawn();
    }

    [ClientRpc]
    public void RpcOnRoundConcluded()
    {
        NetworkManagerGame.instance.m_roundConcluded = true;

        Debug.Log("Round concluded");

        if(!isOwned)
        {
            return;
        }

        isDead = true;
        hasControl = false;
        enabled = false;


        m_playerCamera.gameObject.GetComponent<PlayerCamera>().OnRoundConcluded();
        m_playerHUD.GetComponent<HUDBehaviour>().OnRoundConcluded();
        GetComponentInChildren<PlayerController>().OnRoundConcluded();
    }

}
