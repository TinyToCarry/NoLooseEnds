using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public abstract class EnemyAI : MonoBehaviour
{
    public enum EnemyState { Idle, Walking, Shooting, Dead }

    [Header("General Settings")]
    public float health;
    public float detectionRange;
    public float firingRange;
    public float fireRate;
    public float bulletSpeed;
    public int bulletsPerShot;
    public float spread;
    public float damage;

    [Header("References")]
    public GameObject bulletPrefab;
    public Transform gunEndPoint;
    public Animator animator;

    // Weapon GameObject and dropped weapon prefab
    public GameObject weaponObject;  // Reference to the weapon the enemy is holding
    public GameObject weaponDropPrefab;  // Prefab to drop on death
    public GameObject muzzleFlashPrefab;

    [Header("Audio Settings")]
    public AudioClip fireClip;  // Sound when firing
    public AudioClip hitClip;   // Sound when taking damage
    public AudioSource fireAudioSource;
    public AudioSource hitAudioSource;

    public EnemyState currentState = EnemyState.Idle;
    protected Transform player;
    protected NavMeshAgent agent;
    protected float lastFireTime;

    protected virtual void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = firingRange - 1f;  // Stop slightly before firing range

        // Initialize audio sources
        fireAudioSource.clip = fireClip;
        hitAudioSource.clip = hitClip;
    }

    protected virtual void Update()
    {
        if (currentState == EnemyState.Dead) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange && currentState != EnemyState.Dead)
        {
            // Check if the player is within firing range and line of sight is clear
            if (distanceToPlayer <= firingRange && HasClearLineOfSight())
            {
                AttackPlayer();
            }
            else
            {
                // If not in line of sight, follow the player
                FollowPlayer();
            }
        }
        else
        {
            Idle();
        }
    }

    protected void Idle()
    {
        currentState = EnemyState.Idle;
        animator.SetBool("isWalking", false);
        animator.SetBool("isShooting", false);
        agent.isStopped = true;
    }

    protected virtual void FollowPlayer()
    {
        currentState = EnemyState.Walking;
        agent.isStopped = false;
        agent.SetDestination(player.position);
        animator.SetBool("isWalking", true);
        animator.SetBool("isShooting", false);
    }

    protected void AttackPlayer()
    {
        currentState = EnemyState.Shooting;
        agent.isStopped = true;
        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

        animator.SetBool("isWalking", false);
        animator.SetBool("isShooting", true);

        // Check if there is a clear line of sight to the player
        if (Time.time >= lastFireTime + fireRate && HasClearLineOfSight())
        {
            lastFireTime = Time.time;
            StartCoroutine(FireBullets());

            // Play firing sound
            if (fireAudioSource != null && fireClip != null)
            {
                fireAudioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                fireAudioSource.Play();
            }
        }
    }

    // Check if there is a clear line of sight to the player
    protected bool HasClearLineOfSight()
    {
        Vector3 directionToPlayer = player.position - gunEndPoint.position;
        RaycastHit hitInfo;

        // Raycast from gunEndPoint to player
        if (Physics.Raycast(gunEndPoint.position, directionToPlayer, out hitInfo, firingRange))
        {
            // Check if the ray hit the player directly
            if (hitInfo.transform.CompareTag("Player"))
            {
                return true;
            }
        }

        return false;
    }

    protected IEnumerator FireBullets()
    {
        if (muzzleFlashPrefab != null)
        {
            GameObject muzzleFlash = Instantiate(muzzleFlashPrefab, gunEndPoint.position, gunEndPoint.rotation);
        }

        for (int i = 0; i < bulletsPerShot; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab, gunEndPoint.position, Quaternion.identity);
            EnemyBullet bulletScript = bullet.GetComponent<EnemyBullet>();

            Vector3 baseDirection = (player.position - gunEndPoint.position).normalized;

            float horizontalSpread = Random.Range(-spread, spread);
            float verticalSpread = Random.Range(-spread, spread);

            Quaternion spreadRotation = Quaternion.Euler(verticalSpread, horizontalSpread, 0);
            Vector3 spreadDirection = spreadRotation * baseDirection;

            bulletScript.Initialize(spreadDirection * bulletSpeed, damage);

            // Wait for 0.05 seconds before spawning the next bullet
            yield return new WaitForSeconds(0.05f);
        }
    }

    public void TakeDamage(float damageAmount)
    {
        health -= damageAmount;

        // Play hit sound
        if (hitAudioSource != null && hitClip != null)
        {
            hitAudioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
            hitAudioSource.Play();
        }

        if (health <= 0 && currentState != EnemyState.Dead)
        {
            Die();
        }
    }

    protected void Die()
    {
        currentState = EnemyState.Dead;
        animator.SetBool("isWalking", false);
        animator.SetBool("isShooting", false);
        animator.SetBool("isDead", true);

        // Notify the Objective script of the enemy's death
        Objective objective = FindObjectOfType<Objective>();
        if (objective != null)
        {
            objective.EnemyDefeated();
        }

        // Destroy the weapon they are holding and drop weapon prefab
        if (weaponObject != null)
        {
            Destroy(weaponObject);
        }

        if (weaponDropPrefab != null)
        {
            GameObject droppedWeapon = Instantiate(weaponDropPrefab, gunEndPoint.position, gunEndPoint.rotation);
            Rigidbody rb = droppedWeapon.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(gunEndPoint.forward * 10f, ForceMode.Impulse);
            }
        }

        Destroy(gameObject, 5f);  // Destroy after 5 seconds to allow animation to play
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("PlayerBullet") || collision.gameObject.CompareTag("Thrown"))
        {
            Bullet bullet = collision.gameObject.GetComponent<Bullet>();
            if (bullet != null)
            {
                TakeDamage(bullet.damage);
            }

            ThrowableGun thrownGun = collision.gameObject.GetComponent<ThrowableGun>();
            if (thrownGun != null)
            {
                TakeDamage(thrownGun.damage);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, firingRange);
    }
}
