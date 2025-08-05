using Unity.Netcode;
using Unity.Cinemachine;
using UnityEngine;
using Unity.Collections;

public class TankPlayer : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineVirtualCameraBase _virtualCamera;

    [Header("Settings")]
    [SerializeField] private int cameraPriority = 11; // should be greater than default value of 10

    public NetworkVariable<FixedString32Bytes> PlayerName = new(new FixedString32Bytes("Player"));

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            UserData userData = HostSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientId(OwnerClientId); // getting the user data from the server
            // Set the player name on the server
            PlayerName.Value = userData.userName;
        }

        if (IsOwner)
        {
            _virtualCamera.Priority = cameraPriority;

            // _playerNameText.text = PlayerPrefs.GetString(NameSelector.PlayerName, string.Empty);  // not a great way to get the player name
        }
    }
}
