using UnityEngine;
using Unity.Netcode;
using System;

public class Health : NetworkBehaviour
{
    [field: SerializeField] public int MaxHealth { get; private set; } = 100;
    // use field: SerializeField to expose the field in the inspector while keeping the restriction of private set.
    // This allows you to set the MaxHealth in the inspector, but prevents it from being changed at runtime.
    public NetworkVariable<int> CurrentHealth = new NetworkVariable<int>();
    /*
    Network Variable can only be changed on the server. If client tries to change it, nothing will happen.
    If you want to change it on the client, you need to send a request to the server to change it. (through server RPC)
    NetworkVariable is a special type of variable that is synchronized across the network.
    It is used to keep track of the health of the player.
    When the health changes, it will automatically update on all clients.
    */
    private bool isDead = false;
    public Action<Health> OnDie;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        CurrentHealth.Value = MaxHealth;
    }

    public void TakeDamage(int damage)
    {
        ModifyHealth(-damage);
    }

    public void Heal(int healValue)
    {
        ModifyHealth(healValue);
    }

    public void ModifyHealth(int amount)
    {
        if (!IsServer) return; // Only the server can modify health

        if (isDead) return;

        /*
        ONE WAY TO DO IT:
        CurrentHealth.Value += amount;
        if (CurrentHealth.Value <= 0)
        {
            CurrentHealth.Value = 0;
            isDead = true;
            OnDie?.Invoke(this);
        }
        else if (CurrentHealth.Value > MaxHealth)
        {
            CurrentHealth.Value = MaxHealth;
        }
        ANOTHER WAY TO DO IT:
        */
        CurrentHealth.Value = Mathf.Clamp(CurrentHealth.Value + amount, 0, MaxHealth);
        if (CurrentHealth.Value <= 0)
        {
            isDead = true;
            OnDie?.Invoke(this);
        }
    }
}
