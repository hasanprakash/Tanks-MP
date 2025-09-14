using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkClient : IDisposable
{
    private NetworkManager _networkManager;

    private const string MAIN_MENU_SCENE = "Menu"; // The main menu scene name

    public NetworkClient(NetworkManager networkManager)
    {
        _networkManager = networkManager;

        _networkManager.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (clientId != 0 && _networkManager.LocalClientId != clientId) return;

        Disconnect();
    }
    
    public void Disconnect()
    {
        if (SceneManager.GetActiveScene().name != MAIN_MENU_SCENE)
        {
            SceneManager.LoadScene(MAIN_MENU_SCENE);
        }

        if (_networkManager.IsConnectedClient)
        {
            _networkManager.Shutdown();
            Debug.Log("Disconnected from server and returned to main menu.");
        }
    }

    public void Dispose()
    {
        if (_networkManager != null)
        {
            _networkManager.OnClientDisconnectCallback -= OnClientDisconnected;
        }

        Debug.Log("NetworkClient disposed.");
    }
}
