using UnityEngine;

public class ShotgunEnemy : EnemyAI
{
    protected override void Start()
    {
        base.Start();
        health = 25f;
        detectionRange = 25f;
        firingRange = 15f;
        fireRate = 2f;
        bulletSpeed = 30f;
        bulletsPerShot = 8;
        spread = 8f;
        damage = 5f;
    }
}

