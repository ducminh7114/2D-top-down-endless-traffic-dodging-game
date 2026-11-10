using UnityEngine;
using System.Collections;
using System;

public class Health : MonoBehaviour
{
    public GameObject explosionPrefab;
    public int defaultHealthPoint;

    public Action onDead;
    public Action onHealthChanged;

    [HideInInspector] public int healthPoint;

    protected bool isDead = false;

    private void Start()
    {
        healthPoint = defaultHealthPoint;
        onHealthChanged?.Invoke();
    }

    public void TakeDamage(int value)
    {
        if (isDead || healthPoint <= 0) {
            return;
        }
        healthPoint -= value;
        onHealthChanged?.Invoke();
    
        if (healthPoint <= 0 )
        {
            Die();
        }
    }


    public void Heal(int value) { 
        if(healthPoint >= defaultHealthPoint)
        {
            Debug.LogError("Player already at max health");
        }

        healthPoint = Mathf.Min(healthPoint + value, defaultHealthPoint);
        onHealthChanged.Invoke();
    }

    protected virtual void Die()
    {
        if(isDead)
        {
            return;
        }
        isDead = true;

        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, transform.rotation);

            Destroy(explosion, 1f);
        } 
        onDead?.Invoke();
        Destroy(gameObject);
    }

    private void Update()
    {
        
    }
}
