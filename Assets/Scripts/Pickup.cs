using UnityEngine;

public class Pickup : Interactable
{
    
    [SerializeField]
    private WeaponAnim.GunType gunTypeEnum; // Enum for the actual GunType
    private WeaponAnim weaponAnim;

    private void Start()
    {
        // Find the WeaponAnim script on the player
        weaponAnim = FindObjectOfType<WeaponAnim>();

        if (weaponAnim == null)
        {
            Debug.LogError("WeaponAnim script not found on player.");
        }
    }

    protected override void Interact()
    {
        if (weaponAnim != null)
        {
            // Equip the selected gun type based on the enum value
            weaponAnim.SwitchToGun(gunTypeEnum);

            // Replenish ammo for the picked-up gun
            weaponAnim.ReplenishAmmo(gunTypeEnum);

            // Destroy the pickup object after interaction
            Destroy(gameObject);
        }
    }
}
