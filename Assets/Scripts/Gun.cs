using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform bulletSpawnPoint;
    public Camera playerCamera;
    public bool isAiming;

    // Gun-specific properties (set these in the Inspector for each gun)
    public float bulletSpeed = 40f;
    public int bulletsPerShot = 1;
    public float damage = 10f;  // Unified damage property
    public float spread = 0.05f; // Unified spread property

    // Muzzle flash prefab
    public GameObject muzzleFlashPrefab;

    public void Fire(float damage, float spread)
    {
        Vector3 targetPoint = GetTargetPoint();

        // Instantiate muzzle flash if prefab is assigned
        if (muzzleFlashPrefab != null)
        {
            GameObject muzzleFlash = Instantiate(muzzleFlashPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        }

        for (int i = 0; i < bulletsPerShot; i++)
        {
            GameObject bulletObject = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity);
            Bullet bulletScript = bulletObject.GetComponent<Bullet>();
            bulletScript.speed = bulletSpeed;
            bulletScript.damage = damage;
            bulletScript.spread = spread;
            bulletScript.Initialize(targetPoint, isAiming, spread);
        }
    }

    private Vector3 GetTargetPoint()
    {
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            return hit.point;
        }
        else
        {
            return ray.GetPoint(1000f); // Arbitrary far point if nothing is hit
        }
    }
}
