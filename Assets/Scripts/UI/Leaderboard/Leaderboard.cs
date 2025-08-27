using System;
using Unity.Netcode;
using UnityEngine;

public class Leaderboard : NetworkBehaviour
{
    [SerializeField] private Transform leaderboardEntityHolder;
    [SerializeField] private LeaderboardEntityDisplay leaderboardEntityPrefab;

    private NetworkList<LeaderboardEntityState> leaderboardEntities;

    private void Awake()
    {
        leaderboardEntities = new NetworkList<LeaderboardEntityState>();
    }

    // public override void OnNetworkSpawn()
    // {
    //     if (IsServer)
    //     {
    //         leaderboardEntities.OnListChanged += OnLeaderboardEntitiesChanged;
    //     }
    // }
}
