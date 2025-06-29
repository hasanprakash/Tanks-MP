using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientGameManager
{
    private JoinAllocation _allocation;
    private NetworkClient _networkClient;
    public const string MAIN_MENU_SCENE = "Menu"; 

    // LOGIC TO INTERACT WITH UNITY RELAY
    public async Task<bool> InitAsync()
    {
        await UnityServices.InitializeAsync();

        _networkClient = new NetworkClient(NetworkManager.Singleton);

        AuthState isAuthenticated = await AuthenticationWrapper.DoAuth(5); // 5 is the number of tries to authenticate the player.
        if (isAuthenticated == AuthState.Authenticated)
        {
            Debug.Log("Player authenticated successfully.");
            // Proceed with the game logic, such as connecting to a server or starting the game.
            return true;
        }
        else
        {
            Debug.LogError("Player authentication failed.");
            // Handle authentication failure, such as showing an error message or retrying.
            return false;
        }
    }
    /*
    Making a server call to authenticate the player.
    This is done using the Unity Relay service, which allows us to connect players to a game server.
    We are using async call because, server call takes some time to complete, we will get response as callback after some time.
    We don't want to block the main thread while waiting for the response.
    */

    public void GoToMainMenu()
    {
        SceneManager.LoadScene(MAIN_MENU_SCENE);
    }

    public async Task StartClientAsync(string joinCode)
    {
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        try
        {
            _allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Failed to create host: {e.Message}");
            return;
        }
        catch (Exception e)
        {
            Debug.LogError($"An error occurred while starting the host: {e.Message}");
            return;
        }

        RelayServerData relayServerData = AllocationUtils.ToRelayServerData(_allocation, "dtls");
        transport.SetRelayServerData(relayServerData); // setting this, so that network manager uses the relay server ip and port to connect to the server.

        UserData userData = new UserData
        {
            userName = PlayerPrefs.GetString(NameSelector.PlayerName, "UknownPlayer"),
            userAuthId = AuthenticationService.Instance.PlayerId
        };
        string payload = JsonUtility.ToJson(userData);
        byte[] payloadBytes = System.Text.Encoding.UTF8.GetBytes(payload);
        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

        NetworkManager.Singleton.StartClient();
        Debug.Log($"Client started with join code: {joinCode}");
    }
}
