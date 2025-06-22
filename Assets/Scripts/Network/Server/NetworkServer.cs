using System;
using System.Text;
using Unity.Netcode;
using UnityEngine;

public class NetworkServer
{
    private NetworkManager _networkManager;

    public NetworkServer(NetworkManager networkManager)
    {
        _networkManager = networkManager;
        
        _networkManager.ConnectionApprovalCallback += OnConnectionApproval;
    }

    private void OnConnectionApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        string payload = Encoding.UTF8.GetString(request.Payload);
        UserData userData = JsonUtility.FromJson<UserData>(payload);

        Debug.Log($"Connection approval requested for user: {userData.userName}");

        response.Approved = true; // Approve the connection
        response.CreatePlayerObject = true;
        // player object won't spawn like it does normally, we will have to explicitly mention it when we change the network connection approval logic.
    }
}
