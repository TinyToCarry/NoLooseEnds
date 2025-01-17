using UnityEngine;

public class AttackZone : MonoBehaviour
{
    public float damage = 50f;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object entering the trigger is an enemy
        EnemyAI enemy = other.GetComponent<EnemyAI>();
        if (enemy != null && enemy.currentState != EnemyAI.EnemyState.Dead)
        {
            enemy.TakeDamage(damage);
        }
    }
}
