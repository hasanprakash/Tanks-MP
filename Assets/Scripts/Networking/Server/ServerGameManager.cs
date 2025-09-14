using System;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerGameManager : IDisposable
{
    private string _ip;
    private int _port;
    private int _qport;
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

        if (!_networkServer.OpenConnection(_ip, _port))
        {
            Debug.LogWarning("NetworkServer didn't start as expected.");
            return;
        }

        NetworkManager.Singleton.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
    }
    
    public void Dispose()
    {
        _networkServer?.Dispose();
    }
}
