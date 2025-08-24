using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class RespawnHandler : NetworkBehaviour
{
    [SerializeField] private NetworkObject playerPrefab;

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
        Debug.Log("Subscribed to handle player spawn");
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;

        TankPlayer.OnPlayerSpawned -= HandlePlayerSpawned;
        TankPlayer.OnPlayerDespawned -= HandlePlayerDespawned;
        Debug.Log("Unsubscribed to handle player despawn");
    }

    private void HandlePlayerSpawned(TankPlayer player)
    {
        player.Health.OnDie += (health) => HandlePlayerDied(player);
        Debug.Log("Subscribed to handle player death for: " + player.PlayerName.Value.ToString());
    }

    private void HandlePlayerDespawned(TankPlayer player)
    {
        player.Health.OnDie -= (health) => HandlePlayerDied(player);
        Debug.Log("Unsubscribed to handle player death for: " + player.PlayerName.Value.ToString());
    }

    private void HandlePlayerDied(TankPlayer player)
    {
        Debug.Log("Handling player death and respawn for: " + player.PlayerName.Value.ToString());
        Destroy(player.gameObject);

        // Respawn the player after a frame delay to ensure the player object is fully destroyed
        StartCoroutine(RespawnPlayer(player.OwnerClientId, 3f));
    }

    private IEnumerator RespawnPlayer(ulong ownerClientId, float v)
    {
        // yield return new WaitForEndOfFrame(); // runs at the end of the current frame after rendering is completed
        yield return null; // runs at the beginning of the next frame

        Instantiate(playerPrefab, SpawnPoint.GetRandomSpawnPosition(), Quaternion.identity).SpawnAsPlayerObject(ownerClientId);
    }
}
