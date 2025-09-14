using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Core;
using UnityEngine;

public class ServerSingleton : MonoBehaviour
{
    private static ServerSingleton _instance;
    public ServerGameManager GameManager { get; private set; }

    public static ServerSingleton Instance
    {
        get
        {
            if (_instance != null)
            {
                return _instance;
            }
            _instance = FindFirstObjectByType<ServerSingleton>(); // FindFirstObjectByType is a Unity method that finds the first object of the specified type in the scene.
            // we can't use "this" here because this is not a static property.

            if (_instance == null)
            {
                // TO MANUALLY CREATE THE SINGLETON GAMEOBJECT
                /*
                GameObject singletonObject = new GameObject("ServerSingleton");
                _instance = singletonObject.AddComponent<ServerSingleton>();
                DontDestroyOnLoad(singletonObject);
                */
                Debug.LogError("ServerSingleton instance not found in the scene. Please ensure it is present.");
                return null;
            }
            return _instance;
        }
    }

    // ANOTTHER WAY TO CREATE A SINGLETON INSTANCE
    /*
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    */
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public async Task CreateServer()
    {
        await UnityServices.InitializeAsync();
        GameManager = new ServerGameManager(
            ApplicationData.IP(),
            ApplicationData.Port(),
            ApplicationData.QPort(),
            NetworkManager.Singleton
        );
    }

    private void OnDestroy()
    {
        GameManager?.Dispose();
        _instance = null; // Clear the instance when the singleton is destroyed
        Debug.Log("ServerSingleton destroyed.");
    }
}
