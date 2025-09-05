using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Leaderboard : NetworkBehaviour
{
    [SerializeField] private Transform leaderboardEntityHolder;
    [SerializeField] private LeaderboardEntityDisplay leaderboardEntityPrefab;

    private int maxEntitiesToDisplay = 8;
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
            UpdateExistingEntitiesOnUI();
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

    private void UpdateExistingEntitiesOnUI()
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

        // Sort the entities by coins in descending order
        entityDisplays.Sort((a, b) => b.Coins.CompareTo(a.Coins));
        // Update the UI positions based on the sorted order
        for (int i = 0; i < entityDisplays.Count; i++)
        {
            entityDisplays[i].transform.SetSiblingIndex(i);
            entityDisplays[i].UpdateText();
            entityDisplays[i].gameObject.SetActive(i < maxEntitiesToDisplay); // Show only top N entities
        }

        LeaderboardEntityDisplay myDisplay =
            entityDisplays.Find(e => e.ClientId == NetworkManager.Singleton.LocalClientId);

        if (myDisplay != null && myDisplay.transform.GetSiblingIndex() >= maxEntitiesToDisplay)
        {
            leaderboardEntityHolder.GetChild(maxEntitiesToDisplay - 1).gameObject.SetActive(false);
            myDisplay.gameObject.SetActive(true);
        }
    }

    private void HandlePlayerSpawned(TankPlayer player)
    {
        leaderboardEntities.Add(new LeaderboardEntityState
        {
            ClientId = player.OwnerClientId,
            PlayerName = player.PlayerName.Value,
            Coins = player.Wallet.TotalCoins.Value
        });

        player.Wallet.TotalCoins.OnValueChanged += (oldCoinValue, newCoinValue) =>
            HandleCoinsChanged(player, newCoinValue);
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

        player.Wallet.TotalCoins.OnValueChanged -= (oldCoinValue, newCoinValue) =>
            HandleCoinsChanged(player, newCoinValue);
    }
    
    private void HandleCoinsChanged(TankPlayer player, int newCoinValue)
    {
        for (int i = 0; i < leaderboardEntities.Count; i++)
        {
            if (leaderboardEntities[i].ClientId == player.OwnerClientId)
            {
                LeaderboardEntityState updatedState = leaderboardEntities[i]; // Get a copy
                updatedState.Coins = newCoinValue; // Modify the copy
                leaderboardEntities[i] = updatedState; // updating the network list will trigger the OnListChanged event
                break;
            }
        }
    }
}
