using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using TMPro;
using static WeaponAnim;

public class WeaponAnim : MonoBehaviour
{
    public enum KatanaState { Idle, Enter, Walk, Run, Parry, Attack }
    public enum GunType { None, Pistol, SMG, Shotgun, Rifle }

    private KatanaState currentState = KatanaState.Idle;
    public GunType currentGun = GunType.None;
    private GunType lastGun = GunType.None;

    // Animator reference for the gun holder and override controllers for each gun
    public Animator katanaAnimator;
    public Animator gunAnimator;
    public AnimatorOverrideController pistolOverride;
    public AnimatorOverrideController smgOverride;
    public AnimatorOverrideController shotgunOverride;
    public AnimatorOverrideController rifleOverride;

    public PlayerSounds ps;

    // Reference to the Gun Holder GameObject (parent of all gun models)
    public GameObject gunHolder;
    // Weapon model GameObjects (children of the Gun Holder GameObject)
    public GameObject pistolModel;
    public GameObject smgModel;
    public GameObject shotgunModel;
    public GameObject rifleModel;

    // Parry and Attack Colliders
    public BoxCollider parryZone;
    public SphereCollider attackZone;

    // Key bindings
    public KeyCode attackKey = KeyCode.Mouse0; // Shooting
    public KeyCode parryKey = KeyCode.Mouse1;  // Parrying
    public KeyCode discardKey = KeyCode.Q; // Discard weapon

    // Timing variables
    public float attackCooldown = 0.5f;
    public float atkContinue = 1.5f;
    public float parryDuration = 0.3f;
    public float parryCooldown = 1.5f;
    public float parryEndTime;

    // Gun-specific variables
    private float[] fireRates = { 0.4f, 0.15f, 0.8f, 1.2f }; // [Pistol, SMG, Shotgun, Rifle]
    private float lastShootTime;
    private int[] ammoCounts = { 6, 20, 4, 3 }; // Ammo counts for [Pistol, SMG, Shotgun, Rifle]
    public bool isAiming = false;
    public bool isKatanaActive = true; // Tracks if katana mode is active
    private int attackStage; // Track attack stage for katana attacks
    private float lastAttackTime = 0f;
    private float lastClickTime = 0f;
    private float lastParryTime = 0f;
    private Coroutine shootingCoroutine;
    public bool isShooting;

    public TextMeshProUGUI ammoText;

    // States for katana handling
    public bool isAttacking = false;
    public bool isParrying = false;
    private Coroutine attackCooldownCoroutine;
    private Coroutine attackZoneCoroutine;

    public Transform throwPosition; // Reference to where the gun will be thrown from

    // Prefabs for each gun model with ThrowableGun component attached
    public GameObject pistolThrowablePrefab;
    public GameObject smgThrowablePrefab;
    public GameObject shotgunThrowablePrefab;
    public GameObject rifleThrowablePrefab;

    private bool hasPlayedAmmoOutSound = false;

    private FirstPersonController fpc;

    void Start()
    {
        fpc = FindObjectOfType<FirstPersonController>();

        // Ensure both katana and gun models are initially hidden
        katanaAnimator.gameObject.SetActive(false);
        gunHolder.SetActive(false);

        // Set default gun type to Pistol and equip it
        EquipGun(GunType.Pistol);
        isKatanaActive = false;  // Ensure katana is not active at start

        UpdateAmmoDisplay();
    }


    void Update()
    {
        HandleWeaponSwitch();
        HandleGunSwitch();

        if (currentGun != GunType.None)
        {
            HandleGunStates();
            UpdateAmmoDisplay(); // Ensure ammo display is updated
        }
        else if (isKatanaActive)
        {
            HandleKatanaStates();
        }

        parryZone.enabled = isParrying;
        attackZone.enabled = isAttacking;

        // Check if the discard key (Q) is pressed to throw gun
        if (Input.GetKeyDown(discardKey) && !isKatanaActive)
        {
            DiscardCurrentGun();
        }
    }

    void DiscardCurrentGun()
    {

        // Unequip the current gun
        EquipGun(GunType.None);
        Debug.Log("Gun discarded and set to None.");
        ShowKatana();
        isKatanaActive = true;
    }


    private GameObject GetThrowablePrefabForCurrentGun()
    {
        // Select the appropriate prefab based on the current gun type
        switch (currentGun)
        {
            case GunType.Pistol:
                return pistolThrowablePrefab;
            case GunType.SMG:
                return smgThrowablePrefab;
            case GunType.Shotgun:
                return shotgunThrowablePrefab;
            case GunType.Rifle:
                return rifleThrowablePrefab;
            default:
                return null; // No prefab for GunType.None or if katana is equipped
        }
    }

    void HandleWeaponSwitch()
    {
        if (Input.mouseScrollDelta.y != 0)
        {
            isKatanaActive = !isKatanaActive;
            if (isKatanaActive)
            {
                ShowKatana();
                StartCoroutine(DelayedHideGun());
            }
            else
            {
                StartCoroutine(DelayedHideKatana());
                EquipGun(lastGun);
            }
        }
    }

    void HandleGunStates()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            isAiming = true;
            gunAnimator.SetBool("isAiming", true);
        }
        if (Input.GetKeyUp(KeyCode.Mouse1))
        {
            isAiming = false;
            gunAnimator.SetBool("isAiming", false);
        }

        if (Input.GetKey(attackKey) && Time.time - lastShootTime > fireRates[(int)currentGun - 1])
        {
            Shoot();
        }

        if (isAiming)
        {
            SetGunAnimatorState("isAiming", true);
            SetGunAnimatorState("isWalking", false);
            SetGunAnimatorState("isRunning", false);
        }
        else
        {
            SetGunAnimatorState("isAiming", false);
            if (fpc.isSprinting)
            {
                SetGunAnimatorState("isRunning", true);
                SetGunAnimatorState("isWalking", false);
            }
            else if (fpc.isWalking)
            {
                SetGunAnimatorState("isWalking", true);
                SetGunAnimatorState("isRunning", false);
            }
            else
            {
                SetGunAnimatorState("isWalking", false);
                SetGunAnimatorState("isRunning", false);
            }
        }
    }

    void SetGunAnimatorState(string param, bool state)
    {
        gunAnimator.SetBool(param, state);
    }

    void Shoot()
    {
        if (ammoCounts[(int)currentGun - 1] > 0)
        {
            // Reset the flag when there is ammo to shoot
            hasPlayedAmmoOutSound = false;

            lastShootTime = Time.time;
            ammoCounts[(int)currentGun - 1]--;
            gunAnimator.SetTrigger("Shoot");
            gunAnimator.SetBool("isShooting", true);
            isShooting = true;

            Debug.Log($"Shooting {currentGun}, Ammo left: {ammoCounts[(int)currentGun - 1]}");

            GameObject activeGunModel = GetActiveGunModel();
            Gun gunScript = activeGunModel.GetComponent<Gun>();
            gunScript.isAiming = isAiming;

            // Fire using the gun's own properties
            gunScript.Fire(gunScript.damage, gunScript.spread);

            ps.PlayAttackSound(currentGun);

            fpc.ApplyRecoil();

            if (shootingCoroutine != null)
            {
                StopCoroutine(shootingCoroutine);
            }
            shootingCoroutine = StartCoroutine(ResetShootingFlag());

            UpdateAmmoDisplay();
        }
        else
        {
            // Play ammo out sound only once until ammo is replenished
            if (!hasPlayedAmmoOutSound)
            {
                ps.PlayAmmoOutSound(currentGun);
                hasPlayedAmmoOutSound = true; // Set the flag to prevent repeated sound
            }
            Debug.Log($"Out of Ammo for {currentGun}!");
        }
    }


    void UpdateAmmoDisplay()
    {
        // Update the text with current ammo count for the equipped gun
        if (ammoText != null && currentGun != GunType.None)
        {
            ammoText.text = $"Ammo: {ammoCounts[(int)currentGun - 1]}";
        }
        else if (ammoText != null)
        {
            ammoText.text = ""; // Clear text if no gun is equipped
        }
    }

    GameObject GetActiveGunModel()
    {
        switch (currentGun)
        {
            case GunType.Pistol: return pistolModel;
            case GunType.SMG: return smgModel;
            case GunType.Shotgun: return shotgunModel;
            case GunType.Rifle: return rifleModel;
            default: return null;
        }
    }

    private IEnumerator ResetShootingFlag()
    {
        yield return new WaitForSeconds(fireRates[(int)currentGun - 1] + 0.5f);
        gunAnimator.SetBool("isShooting", false);
        isShooting = false;
    }

    void ShowKatana()
    {
        katanaAnimator.gameObject.SetActive(true);
        StartCoroutine(EnterState());
    }

    IEnumerator DelayedHideKatana()
    {
        yield return HideAnimator(katanaAnimator);
        currentState = KatanaState.Idle;
        ResetAnimatorParameters();
        katanaAnimator.gameObject.SetActive(false);
    }

    IEnumerator EnterState()
    {
        currentState = KatanaState.Enter;
        katanaAnimator.SetTrigger("Enter");
        yield return new WaitForSeconds(katanaAnimator.GetCurrentAnimatorStateInfo(0).length);
        currentState = KatanaState.Idle;
    }

    void HandleGunSwitch()
    {
        for (int i = 1; i <= 4; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + (i - 1)))
            {
                ReplenishAmmo((GunType)i);
                SwitchToGun((GunType)i);
                return;
            }
        }
    }


    public void ReplenishAmmo(GunType gunType)
    {
        switch (gunType)
        {
            case GunType.Pistol:
                ammoCounts[(int)GunType.Pistol - 1] = 6;
                break;
            case GunType.SMG:
                ammoCounts[(int)GunType.SMG - 1] = 20;
                break;
            case GunType.Shotgun:
                ammoCounts[(int)GunType.Shotgun - 1] = 4;
                break;
            case GunType.Rifle:
                ammoCounts[(int)GunType.Rifle - 1] = 3;
                break;
        }
        Debug.Log($"Replenished {gunType} ammo to full.");
    }

    public void SwitchToGun(GunType gunType)
    {
        if (isKatanaActive)
        {
            isKatanaActive = false;
            StartCoroutine(DelayedHideKatana());
        }
        ps.PlayPickupSound(gunType);
        EquipGun(gunType);
        UpdateAmmoDisplay(); // Update ammo display after switching guns
    }

    public void EquipGun(GunType gunType)
    {
        // Determine the correct prefab to throw based on the current gun type
        GameObject throwableGunPrefab = GetThrowablePrefabForCurrentGun();

        if (throwableGunPrefab != null)
        {
            // Instantiate the throwable gun at the throw position
            GameObject throwableGun = Instantiate(throwableGunPrefab, throwPosition.position, throwPosition.rotation);

            // Initialize throw force using the ThrowableGun script
            ThrowableGun throwableGunScript = throwableGun.GetComponent<ThrowableGun>();
            if (throwableGunScript != null)
            {
                throwableGunScript.InitializeThrow(15f); // Adjust the throw force as needed
            }
            ps.PlayDropSound();
            Debug.Log($"Thrown {currentGun} as a projectile.");
        }

        // Hide all gun models initially
        HideCurrentGun();

        // Set the current gun type
        currentGun = gunType;
        lastGun = gunType;

        // Activate the Gun Holder GameObject if equipping a gun
        gunHolder.SetActive(gunType != GunType.None);
        gunAnimator.gameObject.SetActive(gunType != GunType.None);

        // Set the animator override controller and activate the relevant weapon model
        switch (currentGun)
        {
            case GunType.Pistol:
                gunAnimator.runtimeAnimatorController = pistolOverride;
                pistolModel.SetActive(true);
                break;
            case GunType.SMG:
                gunAnimator.runtimeAnimatorController = smgOverride;
                smgModel.SetActive(true);
                break;
            case GunType.Shotgun:
                gunAnimator.runtimeAnimatorController = shotgunOverride;
                shotgunModel.SetActive(true);
                break;
            case GunType.Rifle:
                gunAnimator.runtimeAnimatorController = rifleOverride;
                rifleModel.SetActive(true);
                break;
            default:
                break;
        }

        UpdateAmmoDisplay();
    }

    IEnumerator DelayedHideGun()
    {
        if (currentGun != GunType.None)
        {
            yield return HideAnimator(gunAnimator);
            currentGun = GunType.None;

            // Deactivate the Gun Holder GameObject
            gunHolder.SetActive(false);
        }
    }

    private IEnumerator HideAnimator(Animator animator)
    {
        animator.SetTrigger("Exit");
        yield return new WaitForSeconds(0.4f);
        animator.gameObject.SetActive(false);
    }

    void HideCurrentGun()
    {
        gunAnimator.gameObject.SetActive(false);
        pistolModel.SetActive(false);
        smgModel.SetActive(false);
        shotgunModel.SetActive(false);
        rifleModel.SetActive(false);

    }

    // Remaining methods unchanged


    void HandleKatanaStates()
    {
        if (currentState == KatanaState.Enter) return;

        if (Input.GetKeyDown(parryKey) && Time.time - lastParryTime > parryCooldown)
        {
            StartCoroutine(ParryState());
        }
        else if (Input.GetKeyDown(attackKey) && Time.time - lastAttackTime > attackCooldown)
        {
            HandleAttackState();
        }
        else if (!isAttacking && !isParrying)
        {
            HandleMovementStates();
        }
    }

    void HandleAttackState()
    {
        float timeSinceLastClick = Time.time - lastClickTime;

        // Determine if we should start a new attack stage or reset to stage 1
        int newAttackStage = timeSinceLastClick > atkContinue ? 1 : (attackStage % 3) + 1;

        // Only trigger attack zone if the attack stage changes
        if (newAttackStage != attackStage)
        {
            attackStage = newAttackStage;
            currentState = KatanaState.Attack;
            isAttacking = true;
            lastAttackTime = Time.time;
            lastClickTime = Time.time;

            katanaAnimator.SetInteger("attackStage", attackStage); // Update the animator with the new stage
            if (newAttackStage != 0)
            {
                ps.PlaySlashSound();
            }

            // Activate the attack zone for a short duration
            if (attackZoneCoroutine != null)
            {
                StopCoroutine(attackZoneCoroutine);
            }
            attackZoneCoroutine = StartCoroutine(EnableAttackZoneTemporarily(0.35f));
        }

        // Start the AttackCooldown coroutine if it's not already running
        if (attackCooldownCoroutine == null)
        {
            attackCooldownCoroutine = StartCoroutine(AttackCooldown());
        }
    }

    private IEnumerator EnableAttackZoneTemporarily(float duration)
    {
        attackZone.enabled = true;
        yield return new WaitForSeconds(duration);
        attackZone.enabled = false;
    }


    public void ResetParryCooldown()
    {
        lastParryTime = Time.time;  // Reset the last parry time, allowing immediate parrying again
        Debug.Log("Parry cooldown reset due to successful deflection.");
    }

    IEnumerator ParryState()
    {
        currentState = KatanaState.Parry;
        isParrying = true;
        katanaAnimator.SetBool("isParrying", true);
        lastParryTime = Time.time;
        parryEndTime = Time.time + parryDuration; // Initial parry end time

        while (Time.time < parryEndTime)
        {
            yield return null; // Continue parrying as long as within parry duration
        }

        // End parry state
        isParrying = false;
        katanaAnimator.SetBool("isParrying", false);
        currentState = KatanaState.Idle;
    }

    private IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);

        // Wait until atkContinue time has passed since the last attack
        while (Time.time - lastClickTime <= atkContinue)
        {
            yield return null;
        }

        // Reset attack state after cooldown
        isAttacking = false;
        attackStage = 0; // Reset attack stage
        katanaAnimator.SetInteger("attackStage", 0); // Update animator to reset state
        currentState = KatanaState.Idle;

        // Mark the coroutine as complete
        attackCooldownCoroutine = null;
    }

    void HandleMovementStates()
    {
        if (fpc.isSprinting)
        {
            currentState = KatanaState.Run;
            katanaAnimator.SetBool("isRunning", true);
            katanaAnimator.SetBool("isWalking", false);
        }
        else if (fpc.isWalking)
        {
            currentState = KatanaState.Walk;
            katanaAnimator.SetBool("isWalking", true);
            katanaAnimator.SetBool("isRunning", false);
        }
        else
        {
            currentState = KatanaState.Idle;
            katanaAnimator.SetBool("isWalking", false);
            katanaAnimator.SetBool("isRunning", false);
        }
    }

    void ResetAnimatorParameters()
    {
        katanaAnimator.SetBool("isWalking", false);
        katanaAnimator.SetBool("isRunning", false);
        katanaAnimator.SetBool("isParrying", false);
        katanaAnimator.SetInteger("attackStage", 0);
    }
}
