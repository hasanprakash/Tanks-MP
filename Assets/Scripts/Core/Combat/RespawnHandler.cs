using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class RespawnHandler : NetworkBehaviour
{
    [SerializeField] private TankPlayer playerPrefab;
    [SerializeField] private int keptCoinPercentage; // percentage of coins to keep on respawn, e.g., 20 means 20% of coins are kept

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        TankPlayer[] tankPlayers = FindObjectsByType<TankPlayer>(FindObjectsSortMode.None);
        foreach (var player in tankPlayers)
        {
            HandlePlayerSpawned(player);
        }

        TankPlayer.OnPlayerSpawned += HandlePlayerSpawned;
        TankPlayer.OnPlayerDespawned += HandlePlayerDespawned;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;

        TankPlayer.OnPlayerSpawned -= HandlePlayerSpawned;
        TankPlayer.OnPlayerDespawned -= HandlePlayerDespawned;
    }

    private void HandlePlayerSpawned(TankPlayer player)
    {
        player.Health.OnDie += (health) => HandlePlayerDied(player);
    }

    private void HandlePlayerDespawned(TankPlayer player)
    {
        player.Health.OnDie -= (health) => HandlePlayerDied(player);
    }

    private void HandlePlayerDied(TankPlayer player)
    {
        int coinsToKeep = (int) (player.Wallet.TotalCoins.Value * (keptCoinPercentage / 100f));

        Destroy(player.gameObject);

        // Respawn the player after a frame delay to ensure the player object is fully destroyed
        StartCoroutine(RespawnPlayer(player.OwnerClientId, coinsToKeep));
    }

    private IEnumerator RespawnPlayer(ulong ownerClientId, int coinsToKeep)
    {
        // yield return new WaitForEndOfFrame(); // runs at the end of the current frame after rendering is completed
        yield return null; // runs at the beginning of the next frame

        TankPlayer playerInstance = Instantiate(playerPrefab, SpawnPoint.GetRandomSpawnPosition(), Quaternion.identity);
        playerInstance.NetworkObject.SpawnAsPlayerObject(ownerClientId);

        playerInstance.Wallet.TotalCoins.Value = coinsToKeep;
    }
}
