using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Leaderboard : NetworkBehaviour
{
    [SerializeField] private Transform leaderboardEntityHolder;
    [SerializeField] private LeaderboardEntityDisplay leaderboardEntityPrefab;

    private NetworkList<LeaderboardEntityState> leaderboardEntities;
    private List<LeaderboardEntityDisplay> entityDisplays;

    private void Awake()
    {
        leaderboardEntities = new NetworkList<LeaderboardEntityState>();
        entityDisplays = new List<LeaderboardEntityDisplay>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            UpdateExistingEntities();
            leaderboardEntities.OnListChanged += HandleLeaderboardChanged;
        }

        if (IsServer)
        {
            TankPlayer[] tankPlayers = FindObjectsByType<TankPlayer>(FindObjectsSortMode.None);
            foreach (var player in tankPlayers)
            {
                HandlePlayerSpawned(player);
            }

            TankPlayer.OnPlayerSpawned += HandlePlayerSpawned;
            TankPlayer.OnPlayerDespawned += HandlePlayerDespawned;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            leaderboardEntities.OnListChanged -= HandleLeaderboardChanged;
        }

        if (IsServer)
        {
            TankPlayer.OnPlayerSpawned -= HandlePlayerSpawned;
            TankPlayer.OnPlayerDespawned -= HandlePlayerDespawned;
        }
    }
    
    private void UpdateExistingEntities()
    {
        foreach (LeaderboardEntityState entity in leaderboardEntities)
        {
            LeaderboardEntityDisplay leaderboardEntity = Instantiate(leaderboardEntityPrefab, leaderboardEntityHolder);
            leaderboardEntity.Initialize(entity.ClientId, entity.PlayerName.ToString(), entity.Coins);
            entityDisplays.Add(leaderboardEntity);
        }
    }

    private void HandleLeaderboardChanged(NetworkListEvent<LeaderboardEntityState> changeEvent)
    {
        switch (changeEvent.Type)
        {
            case NetworkListEvent<LeaderboardEntityState>.EventType.Add:
                if (!entityDisplays.Exists(e => e.ClientId == changeEvent.Value.ClientId)) // or you can use LINQ: !entityDisplays.Any(e => e.ClientId == changeEvent.Value.ClientId)
                {
                    LeaderboardEntityDisplay leaderboardEntity = Instantiate(leaderboardEntityPrefab, leaderboardEntityHolder);
                    leaderboardEntity.Initialize(changeEvent.Value.ClientId, changeEvent.Value.PlayerName.ToString(), changeEvent.Value.Coins);
                    entityDisplays.Add(leaderboardEntity);
                }
                break;
            case NetworkListEvent<LeaderboardEntityState>.EventType.Remove:
                LeaderboardEntityDisplay entityToRemove = entityDisplays.Find(e => e.ClientId == changeEvent.Value.ClientId); // or you can use LINQ: entityDisplays.FirstOrDefault(e => e.ClientId == changeEvent.Value.ClientId);
                if (entityToRemove != null)
                {
                    entityToRemove.transform.SetParent(null); // detach from parent to avoid destroying the parent object
                    Destroy(entityToRemove.gameObject);
                    entityDisplays.Remove(entityToRemove);
                }
                break;
            case NetworkListEvent<LeaderboardEntityState>.EventType.Value:
                LeaderboardEntityDisplay entityToUpdate = entityDisplays.Find(e => e.ClientId == changeEvent.Value.ClientId);
                if (entityToUpdate != null)
                {
                    entityToUpdate.UpdateCoins(changeEvent.Value.Coins);
                }
                break;
        }
    }

    private void HandlePlayerSpawned(TankPlayer player)
    {
        leaderboardEntities.Add(new LeaderboardEntityState
        {
            ClientId = player.OwnerClientId,
            PlayerName = player.PlayerName.Value,
            Coins = 0
        });
    }

    private void HandlePlayerDespawned(TankPlayer player)
    {
        if (leaderboardEntities.Count == 0) return;

        for (int i = 0; i < leaderboardEntities.Count; i++)
        {
            if (leaderboardEntities[i].ClientId == player.OwnerClientId)
            {
                leaderboardEntities.RemoveAt(i);
                break;
            }
        }
    }
}
