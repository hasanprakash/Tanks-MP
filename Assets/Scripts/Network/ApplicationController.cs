using System.Threading.Tasks;
using UnityEngine;

public class ApplicationController : MonoBehaviour
{
    [SerializeField] private ClientSingleton clientPrefab;
    [SerializeField] private HostSingleton hostPrefab;
    private async void Start()
    {
        DontDestroyOnLoad(gameObject);

        await LaunchInMode(SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null);
        // why can't it be just null?        // Because Unity's GraphicsDeviceType.Null is a specific enum value that indicates no graphics device is available.
        // It is not the same as a null reference in C#. The check ensures that the application is launched only if there is a valid graphics device.   
    }
    
    private async Task LaunchInMode(bool isDedicatedServer)
    {
        if (isDedicatedServer)
        {
            Debug.Log("Launching dedicated server...");
            // Initialize dedicated server logic here
        }
        else
        {
            Debug.Log("Launching client application...");
            // Initialize client application logic here

            // ClientSingleton clientSingleton = Instantiate(ClientSingleton.Instance); // can't we create like this? have to check that out later.
            ClientSingleton clientSingleton = Instantiate(clientPrefab); // what if we don't initialize?
            await clientSingleton.CreateClient();

            HostSingleton hostSingleton = Instantiate(hostPrefab); // what if we don't initialize?
            hostSingleton.CreateHost();

            // Go to main menu
        }
    }
}
