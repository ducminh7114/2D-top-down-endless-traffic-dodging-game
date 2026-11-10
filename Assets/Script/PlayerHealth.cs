using UnityEngine;

public class PlayerHealth : Health
{
    protected override void Die()
    {
        base.Die();
        Debug.Log("Player Died");
    }
}
