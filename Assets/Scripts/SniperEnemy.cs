using UnityEngine;

public class SniperEnemy : EnemyAI
{
    protected override void Start()
    {
        base.Start();
        health = 12f;
        detectionRange = 40f;
        firingRange = 40f;
        fireRate = 3f;
        bulletSpeed = 40f;
        bulletsPerShot = 1;
        spread = 0.5f;
        damage = 25f;
    }

    protected override void FollowPlayer()
    {
        // Sniper doesn't move, so no following logic is needed.
        Idle();
    }
}