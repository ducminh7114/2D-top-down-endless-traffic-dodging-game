
using UnityEngine;

public class PlayerHealth : Health
{
    private void OnEnable()
    {
        onDead += HandlePlayerDeath;
    }

    private void OnDisable()
    {
        onDead -= HandlePlayerDeath;
    }

    private void HandlePlayerDeath()
    {
        Debug.Log("Player Died");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }
}
