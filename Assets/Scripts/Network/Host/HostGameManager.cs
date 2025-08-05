using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using Unity.Services.Authentication;
using System;

public class HostGameManager : IDisposable
{
    private Allocation _allocation;
    private string _joinCode;
    private string _lobbyId;

    public NetworkServer NetworkServer { get; private set; }

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

        try
        {
            string playerName = PlayerPrefs.GetString(NameSelector.PlayerName, "UnknownPlayer");
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync($"{playerName}'s Lobby", MaxConnections, new CreateLobbyOptions
            {
                IsPrivate = false,
                Data = new Dictionary<string, DataObject>
                {
                    { "JoinCode", new DataObject(DataObject.VisibilityOptions.Member, _joinCode) }
                    // DataObject.VisibilityOptions.Member, Private, Public
                    // Member: Visible to all members of the lobby
                    // Private: Visible only to the creator of the lobby
                    // Public: Visible to everyone, including non-members
                }
            });

            _lobbyId = lobby.Id;
            Debug.Log($"Lobby created successfully with ID: {_lobbyId}");

            HostSingleton.Instance.StartCoroutine(HeartBeatLobby(15f));
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to create lobby: {e.Message}");
            return;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"An error occurred while creating the lobby: {e.Message}");
            return;
        }

        UserData userData = new UserData
        {
            userName = PlayerPrefs.GetString(NameSelector.PlayerName, "UknownPlayer"),
            userAuthId = AuthenticationService.Instance.PlayerId
        };
        string payload = JsonUtility.ToJson(userData);
        byte[] payloadBytes = System.Text.Encoding.UTF8.GetBytes(payload);
        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

        NetworkServer = new NetworkServer(NetworkManager.Singleton);

        NetworkManager.Singleton.StartHost();

        NetworkManager.Singleton.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
    }

    private IEnumerator HeartBeatLobby(float waitTimeSeconds) {
        WaitForSeconds delay = new WaitForSeconds(waitTimeSeconds);
        while (true)
        {
            try
            {
                LobbyService.Instance.SendHeartbeatPingAsync(_lobbyId);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"Failed to send lobby heartbeat: {e.Message}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"An error occurred while sending the lobby heartbeat: {e.Message}");
            }
            yield return delay;
        }
    }

    public void Dispose()
    {
        HostSingleton.Instance.StopCoroutine(nameof(HeartBeatLobby));

        if (!string.IsNullOrEmpty(_lobbyId))
        {
            try
            {
                LobbyService.Instance.DeleteLobbyAsync(_lobbyId);
                Debug.Log($"Lobby with ID {_lobbyId} deleted successfully.");
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"Failed to delete lobby: {e.Message}");
            }
            catch (Exception e)
            {
                Debug.LogError($"An error occurred while deleting the lobby: {e.Message}");
            }
            _lobbyId = string.Empty; // Clear the lobby ID after deletion
        }

        NetworkServer?.Dispose();
    }
}
