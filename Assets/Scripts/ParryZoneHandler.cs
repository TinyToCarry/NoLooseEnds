using UnityEngine;
using System.Collections;

public class ParryZoneHandler : MonoBehaviour
{
    public Camera playerCamera;  // Reference to the player's camera
    public GameObject bulletPrefab;  // Prefab for the redirected bullet
    public float bulletSpeed = 40f;
    public float bulletLifetime = 10f;
    public float bulletSpread = 0.1f;
    public float hitstopDuration = 0.05f;  // Duration of the hitstop in seconds

    public WeaponAnim weaponAnim;  // Reference to the WeaponAnim script
    private LayerMask raycastLayerMask;

    public GameObject muzzleFlashPrefab;
    public PlayerSounds ps;

    void Start()
    {
        // Find the WeaponAnim component on the player, if not already assigned
        if (weaponAnim == null)
        {
            weaponAnim = GetComponent<WeaponAnim>();
        }

        // Create a LayerMask that excludes the "IgnoreRaycast" layer
        raycastLayerMask = ~LayerMask.GetMask("IgnoreRaycast"); // Inverse the mask to ignore this layer
    }

    void OnCollisionEnter(Collision other)
    {
        // Check if the collider is tagged as "EnemyBullet"
        if (other.gameObject.CompareTag("EnemyBullet"))
        {
            Vector3 collisionPoint = other.transform.position;
            if (muzzleFlashPrefab != null)
            {
                GameObject muzzleFlash = Instantiate(muzzleFlashPrefab, collisionPoint, Quaternion.identity);
            }

            ps.PlayParrySound();
            GameObject newBullet = Instantiate(bulletPrefab, collisionPoint, Quaternion.identity);
            Vector3 targetPoint = GetTargetPoint();

            Bullet bulletScript = newBullet.GetComponent<Bullet>();
            bulletScript.speed = bulletSpeed;
            bulletScript.lifetime = bulletLifetime;
            bulletScript.Initialize(targetPoint, false, bulletSpread);

            WeaponAnim weaponAnim = GetComponentInParent<WeaponAnim>();
            if (weaponAnim != null && weaponAnim.isParrying)
            {
                weaponAnim.parryEndTime = Time.time + weaponAnim.parryDuration;
            }

            weaponAnim.ResetParryCooldown();

            // Trigger hitstop
            StartCoroutine(ApplyHitstop());
        }
    }

    private Vector3 GetTargetPoint()
    {
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        RaycastHit hit;

        // Use the LayerMask to ignore the parry collider layer
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, raycastLayerMask))
        {
            return hit.point;
        }
        else
        {
            return ray.GetPoint(1000f);  // Arbitrary far point if nothing is hit
        }
    }

    private IEnumerator ApplyHitstop()
    {
        Time.timeScale = 0f;  // Pause the game
        yield return new WaitForSecondsRealtime(hitstopDuration);  // Wait for hitstop duration in real-time
        Time.timeScale = 1f;  // Resume the game
    }
}
