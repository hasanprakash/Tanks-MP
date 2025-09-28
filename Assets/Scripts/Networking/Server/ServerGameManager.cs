using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerGameManager : IDisposable
{
    private string _ip;
    private int _port;
    private int _qport;
    private MatchplayBackfiller _backfiller;
    private NetworkServer _networkServer;
    private MultiplayAllocationService _allocationService;

    private const string GameSceneName = "Game"; // Name of the game scene to load

    public ServerGameManager(string ip, int port, int qport, NetworkManager networkManager)
    {
        _ip = ip;
        _port = port;
        _qport = qport;
        _networkServer = new NetworkServer(networkManager);
        _allocationService = new MultiplayAllocationService();
    }
    public async Task StartGameServerAsync()
    {
        await _allocationService.BeginServerCheck();

        try
        {
            MatchmakingResults matchmakingPayload = await GetMatchmakerPayloadAsync();

            if (matchmakingPayload == null)
            {
                await StartBackfill(matchmakingPayload);
                _networkServer.OnUserJoined += UserJoined;
                _networkServer.OnUserLeft += UserLeft;
            }
            else
            {
                Debug.LogWarning("Matchmaker payload timedout.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex);
        }

        if (!_networkServer.OpenConnection(_ip, _port))
        {
            Debug.LogWarning("NetworkServer didn't start as expected.");
            return;
        }

        NetworkManager.Singleton.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
    }

    private async Task<MatchmakingResults> GetMatchmakerPayloadAsync()
    {
        Task<MatchmakingResults> matchmakerPayloadTask = _allocationService.SubscribeAndAwaitMatchmakerAllocation();

        if (await Task.WhenAny(matchmakerPayloadTask, Task.Delay(20000)) == matchmakerPayloadTask)
        {
            return matchmakerPayloadTask.Result;
        }
        return null;
    }

    private async Task StartBackfill(MatchmakingResults payload)
    {
        _backfiller = new MatchplayBackfiller($"{_ip}:{_port}", payload.QueueName, payload.MatchProperties, 4);

        if (_backfiller.NeedsPlayers())
        {
            await _backfiller.BeginBackfilling();
        }
    }

    private void UserJoined(UserData user)
    {
        _backfiller.AddPlayerToMatch(user);
        _allocationService.AddPlayer();
        if (!_backfiller.NeedsPlayers() && _backfiller.IsBackfilling)
        {
            _ = _backfiller.StopBackfill();
        }
    }

    private void UserLeft(UserData user)
    {
        int playerCount = _backfiller.RemovePlayerFromMatch(user.userAuthId);
        _allocationService.RemovePlayer();
        if (playerCount <= 0)
        {
            CloseServer();
            return;
        }
        if (_backfiller.NeedsPlayers() && !_backfiller.IsBackfilling)
        {
            _ = _backfiller.BeginBackfilling();
        }
    }

    private async void CloseServer()
    {
        await _backfiller.StopBackfill();
        Dispose();
        Application.Quit();
    }

    public void Dispose()
    {
        _networkServer.OnUserJoined -= UserJoined;
        _networkServer.OnUserLeft -= UserLeft;
        
        _backfiller?.Dispose();
        _allocationService?.Dispose();
        _networkServer?.Dispose();
    }
}
