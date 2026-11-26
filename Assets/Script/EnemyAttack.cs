
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public Health health;

    public int damage;
    public int enemyDamage;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();

        TrafficVehicle vehicle =
                       GetComponent<TrafficVehicle>();

        if (vehicle != null && vehicle.isGolden)
        {
            health.TakeDamage(enemyDamage);
            // Golden vehicle: reward coins, no damage.
            return;
        }

        // Normal vehicle: deal damage.
        playerHealth.TakeDamage(damage);

        if (health != null)
        {
            health.TakeDamage(enemyDamage);
        }
    }
}

