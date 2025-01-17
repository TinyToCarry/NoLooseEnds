using UnityEngine;

public class SMGEnemy : EnemyAI
{
    protected override void Start()
    {
        base.Start();
        health = 20f;
        detectionRange = 30f;
        firingRange = 18f;
        fireRate = 0.6f;
        bulletSpeed = 35f;
        bulletsPerShot = 1;
        spread = 4f;
        damage = 8f;
    }
}

