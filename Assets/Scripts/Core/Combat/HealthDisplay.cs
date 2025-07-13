using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
public class HealthDisplay : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Health health;
    [SerializeField] private Image healthBarImage;

    public override void OnNetworkSpawn()
    {
        if (!IsClient) return;

        health.CurrentHealth.OnValueChanged += HandleHealthChanged;
        HandleHealthChanged(0, health.CurrentHealth.Value);
        // Initialize the health bar on spawn
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer) return;
        
        health.CurrentHealth.OnValueChanged -= HandleHealthChanged;
    }

    public void HandleHealthChanged(int oldHealth, int newHealth)
    {
        if (healthBarImage == null) return;

        // Calculate the health percentage
        float healthPercentage = (float)newHealth / health.MaxHealth;

        // Update the health bar image fill amount
        healthBarImage.fillAmount = healthPercentage;

        // changing the color based on health percentage
        if (healthPercentage > 0.5f)
        {
            healthBarImage.color = Color.green; // Healthy
        }
        else if (healthPercentage > 0.2f)
        {
            healthBarImage.color = Color.yellow; // Caution
        }
        else
        {
            healthBarImage.color = Color.red; // Critical
        }
    }
}
