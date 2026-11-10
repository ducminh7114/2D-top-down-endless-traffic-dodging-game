using UnityEngine;
using UnityEditor.PackageManager;

public class EnemyHealth : Health
{

    private void Awake()
    {
        
    }

    protected override void Die()
    {
        base.Die();
    }

    public void killEnemy()
    {
        Die();
    }
}
