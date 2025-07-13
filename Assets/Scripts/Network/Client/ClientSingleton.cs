using System.Threading.Tasks;
using UnityEngine;

public class ClientSingleton : MonoBehaviour
{
    private static ClientSingleton _instance;
    public ClientGameManager GameManager { get; private set; }

    public static ClientSingleton Instance
    {
        get
        {
            if (_instance != null)
            {
                return _instance;
            }
            _instance = FindFirstObjectByType<ClientSingleton>(); // FindFirstObjectByType is a Unity method that finds the first object of the specified type in the scene.
            // we can't use "this" here because this is not a static property.

            if (_instance == null)
            {
                // TO MANUALLY CREATE THE SINGLETON GAMEOBJECT
                /*
                GameObject singletonObject = new GameObject("ClientSingleton");
                _instance = singletonObject.AddComponent<ClientSingleton>();
                DontDestroyOnLoad(singletonObject);
                */
                Debug.LogError("ClientSingleton instance not found in the scene. Please ensure it is present.");
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

    public async Task<bool> CreateClient()
    {
        GameManager = new ClientGameManager();
        return await GameManager.InitAsync();
        // After initialization, you can proceed with other client logic, such as connecting to a server or starting the game.
    }

    private void Destroy()
    {
        GameManager?.Dispose();
        _instance = null; // Clear the instance when the singleton is destroyed
        Debug.Log("ClientSingleton destroyed.");
    }
}
