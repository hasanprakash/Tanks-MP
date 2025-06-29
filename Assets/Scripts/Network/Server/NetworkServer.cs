using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

public class NetworkServer
{
    private NetworkManager _networkManager;
    
    private Dictionary<ulong, string> _clientIdToAuth = new Dictionary<ulong, string>();
    private Dictionary<string, UserData> _authIdToUserData = new Dictionary<string, UserData>();

    public NetworkServer(NetworkManager networkManager)
    {
        _networkManager = networkManager;

        _networkManager.ConnectionApprovalCallback += OnConnectionApproval;
        _networkManager.OnServerStarted += OnServerStarted;
        // _networkManager.OnClientDisconnectCallback += OnClientDisconnect;
    }

    private void OnConnectionApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        string payload = Encoding.UTF8.GetString(request.Payload);
        UserData userData = JsonUtility.FromJson<UserData>(payload);

        Debug.Log($"Connection approval requested for user: {userData.userName}");
        _clientIdToAuth[request.ClientNetworkId] = userData.userAuthId;
        _authIdToUserData[userData.userAuthId] = userData;

        response.Approved = true; // Approve the connection
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
            _authIdToUserData.Remove(authId);
        }
        else
        {
            Debug.LogWarning($"Client disconnected but no auth ID found for client ID: {clientId}");
        }
    }
}
