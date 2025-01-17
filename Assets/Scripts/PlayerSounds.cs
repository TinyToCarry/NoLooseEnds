using System;
using UnityEngine;

public class PlayerSounds : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource walkSource;
    public AudioSource runSource;
    public AudioSource attackSource;
    public AudioSource hitSource;
    public AudioSource parrySource;
    public AudioSource pickupSource;
    public AudioSource dropSource;

    [Header("Audio Clips")]
    public AudioClip walkClip;
    public AudioClip runClip;
    public AudioClip slashClip;
    public AudioClip pistolClip;
    public AudioClip smgClip;
    public AudioClip shotgunClip;
    public AudioClip rifleClip;
    public AudioClip ammoOutClip;
    public AudioClip hitClip;
    public AudioClip parryClip;
    public AudioClip pickupClip;
    public AudioClip dropClip;

    public FirstPersonController playerController;
    public WeaponAnim weaponAnim;
    public HealthBar playerHealth;

    void Start()
    {

        // Assign audio clips
        walkSource.clip = walkClip;
        runSource.clip = runClip;
        hitSource.clip = hitClip;
        parrySource.clip = parryClip;
        pickupSource.clip = pickupClip;
        dropSource.clip = dropClip;
    }

    void Update()
    {
        HandleMovementSounds();
    }

    private void HandleMovementSounds()
    {
        if (playerController.isGrounded)
        {
            if (playerController.isWalking && !walkSource.isPlaying)
            {
                walkSource.loop = true;
                walkSource.Play();
            }
            else if (!playerController.isWalking)
            {
                walkSource.Stop();
            }

            if (playerController.isSprinting && !runSource.isPlaying)
            {
                runSource.loop = true;
                runSource.Play();
            }
            else if (!playerController.isSprinting)
            {
                runSource.Stop();
            }
        }
    }

    public void PlayAttackSound(WeaponAnim.GunType gunType)
    {
        attackSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);

        switch (gunType)
        {
            case WeaponAnim.GunType.Pistol:
                attackSource.PlayOneShot(pistolClip);
                break;
            case WeaponAnim.GunType.SMG:
                attackSource.PlayOneShot(smgClip);
                break;
            case WeaponAnim.GunType.Shotgun:
                attackSource.PlayOneShot(shotgunClip);
                break;
            case WeaponAnim.GunType.Rifle:
                attackSource.PlayOneShot(rifleClip);
                break;
            default:
                break;
        }
    }

    public void PlaySlashSound()
    {
        attackSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
        attackSource.PlayOneShot(slashClip); // Assuming `slashClip` is your katana slash sound
    }


    public void PlayAmmoOutSound(WeaponAnim.GunType gunType)
    {
        attackSource.pitch = (gunType == WeaponAnim.GunType.Shotgun) ? 0.9f : 1.1f;
        attackSource.PlayOneShot(ammoOutClip);
    }


    public void PlayHitSound()
    {
        hitSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
        hitSource.PlayOneShot(hitClip);
    }

    public void PlayParrySound()
    {
        parrySource.pitch = UnityEngine.Random.Range(0.8f, 1.0f);
        parrySource.PlayOneShot(parryClip);
    }

    public void PlayPickupSound(WeaponAnim.GunType gunType)
    {
        pickupSource.pitch = (gunType == WeaponAnim.GunType.Rifle) ? 1.1f : 1.0f;
        pickupSource.PlayOneShot(pickupClip);
    }

    public void PlayDropSound()
    {
        dropSource.PlayOneShot(dropClip);
    }
}
