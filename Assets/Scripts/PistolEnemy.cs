using UnityEngine;

public class PistolEnemy : EnemyAI
{
    protected override void Start()
    {
        base.Start();
        health = 15f;
        detectionRange = 30f;
        firingRange = 20f;
        fireRate = 1.2f;
        bulletSpeed = 35f;
        bulletsPerShot = 1;
        spread = 2f;
        damage = 10f;
    }
}

