using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class NetworkServer : IDisposable
{
    private NetworkManager _networkManager;

    public Action<string> OnClientLeft;
    public Action<UserData> OnUserJoined;
    public Action<UserData> OnUserLeft;
    private Dictionary<ulong, string> _clientIdToAuth = new Dictionary<ulong, string>();
    private Dictionary<string, UserData> _authIdToUserData = new Dictionary<string, UserData>();

    public NetworkServer(NetworkManager networkManager)
    {
        _networkManager = networkManager;

        _networkManager.ConnectionApprovalCallback += OnConnectionApproval;
        _networkManager.OnServerStarted += OnServerStarted;
        // _networkManager.OnClientDisconnectCallback += OnClientDisconnect;
    }

    public bool OpenConnection(string ip, int port)
    {
        try
        {
            _networkManager.gameObject.GetComponent<UnityTransport>().SetConnectionData(ip, (ushort)port);
            if (_networkManager.StartServer())
            {
                Debug.Log($"Server started on {ip}:{port}");
                return true;
            }
            else
            {
                Debug.LogError("Failed to start server.");
                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Exception while starting server: {ex.Message}");
            return false;
        }
    }

    private void OnConnectionApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        string payload = Encoding.UTF8.GetString(request.Payload);
        UserData userData = JsonUtility.FromJson<UserData>(payload);

        Debug.Log($"Connection approval requested for user: {userData.userName}");
        _clientIdToAuth[request.ClientNetworkId] = userData.userAuthId;
        _authIdToUserData[userData.userAuthId] = userData;

        OnUserJoined?.Invoke(userData);

        response.Approved = true; // Approve the connection
        response.Position = SpawnPoint.GetRandomSpawnPosition();
        response.Rotation = Quaternion.identity;
        response.CreatePlayerObject = true;
        // player object won't spawn like it does normally, we will have to explicitly mention it when we change the network connection approval logic.
    }

    private void OnServerStarted()
    {
        _networkManager.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (_clientIdToAuth.TryGetValue(clientId, out string authId))
        {
            Debug.Log($"Client disconnected: {authId}");
            _clientIdToAuth.Remove(clientId);
            OnUserLeft?.Invoke(_authIdToUserData[authId]);
            _authIdToUserData.Remove(authId);
            OnClientLeft?.Invoke(authId);
        }
        else
        {
            Debug.LogWarning($"Client disconnected but no auth ID found for client ID: {clientId}");
        }
    }

    public void Dispose()
    {
        if (_networkManager != null)
        {
            _networkManager.ConnectionApprovalCallback -= OnConnectionApproval;
            _networkManager.OnServerStarted -= OnServerStarted;
            _networkManager.OnClientDisconnectCallback -= OnClientDisconnected;
        }

        if (_networkManager.IsListening)
        {
            _networkManager.Shutdown();
        }

        Debug.Log("NetworkServer disposed.");
    }
    
    public UserData GetUserDataByClientId(ulong clientId)
    {
        if (_clientIdToAuth.TryGetValue(clientId, out string authId) && _authIdToUserData.TryGetValue(authId, out UserData userData))
        {
            return userData;
        }
        return null;
    }
}
