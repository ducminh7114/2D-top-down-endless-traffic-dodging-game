using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public EnemyHealth Health;
    public int damage;
    public int enemyDamage;

    private void OnTriggerEnter2D(Collider2D collision) 
    {
        var playerHealth = collision.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            Health.TakeDamage(enemyDamage);
        }
    }
}
