using Unity.Netcode;
using Unity.Cinemachine;
using UnityEngine;

public class TankPlayer : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineVirtualCameraBase _virtualCamera;

    [Header("Settings")]
    [SerializeField] private int cameraPriority = 11; // should be greater than default value of 10
    
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            _virtualCamera.Priority = cameraPriority;
        }
    }
}
