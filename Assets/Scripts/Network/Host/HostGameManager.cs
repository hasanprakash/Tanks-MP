using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HostGameManager
{
    private Allocation _allocation;
    private string _joinCode;

    private const int MaxConnections = 20;
    private const string GameSceneName = "Game";
    public async Task StartHostAsync()
    {
        try
        {
            _allocation = await RelayService.Instance.CreateAllocationAsync(MaxConnections);
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Failed to create host: {e.Message}");
            return;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"An error occurred while starting the host: {e.Message}");
            return;
        }

        try
        {
            _joinCode = await RelayService.Instance.GetJoinCodeAsync(_allocation.AllocationId);
            Debug.Log($"Host started successfully. Join code: {_joinCode}");
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Failed to get join code: {e.Message}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"An error occurred while getting the join code: {e.Message}");
        }

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        // Get Relay Server Data
        RelayServerData relayServerData = AllocationUtils.ToRelayServerData(_allocation, "dtls");
        transport.SetRelayServerData(relayServerData);

        NetworkManager.Singleton.StartHost();

        NetworkManager.Singleton.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
    }
}
