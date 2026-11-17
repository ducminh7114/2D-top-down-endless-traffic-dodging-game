
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public Health health;

    public int damage;
    public int enemyDamage;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);

            if (health != null)
            {
                health.TakeDamage(enemyDamage);
            }
        }
    }
}

