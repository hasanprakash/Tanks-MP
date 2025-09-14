using Unity.Netcode;
using Unity.Cinemachine;
using UnityEngine;
using Unity.Collections;
using System;

public class TankPlayer : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineVirtualCameraBase _virtualCamera;
    [SerializeField] private Texture2D crossHair;
    [field: SerializeField] public Health Health { get; private set; }
    [field: SerializeField] public CoinWallet Wallet { get; private set; }

    [Header("Settings")]
    [SerializeField] private int cameraPriority = 11; // should be greater than default value of 10

    public NetworkVariable<FixedString32Bytes> PlayerName = new(new FixedString32Bytes("Player"));

    public static Action<TankPlayer> OnPlayerSpawned;
    public static Action<TankPlayer> OnPlayerDespawned;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            UserData userData = HostSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientId(OwnerClientId); // getting the user data from the server
            // Set the player name on the server
            PlayerName.Value = userData.userName;

            OnPlayerSpawned?.Invoke(this);
            Debug.Log("Player spawned: " + PlayerName.Value.ToString());
        }

        if (IsOwner)
        {
            _virtualCamera.Priority = cameraPriority;

            Cursor.SetCursor(crossHair, new Vector2(crossHair.width / 2, crossHair.height / 2), CursorMode.Auto);

            // _playerNameText.text = PlayerPrefs.GetString(NameSelector.PlayerName, string.Empty);  // not a great way to get the player name
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            OnPlayerDespawned?.Invoke(this);
        }
    }
}
