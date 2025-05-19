using Unity.Netcode;
using UnityEngine;

public class ConnectionButtons : MonoBehaviour
{
    public void StartHost()
    {
        Debug.Log("Starting Host");
        NetworkManager.Singleton.StartHost();
    }
    
    public void StartClient()
    {
        Debug.Log("Starting Client");
        NetworkManager.Singleton.StartClient();
    }
}
