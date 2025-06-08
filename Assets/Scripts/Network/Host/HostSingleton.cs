using System.Threading.Tasks;
using UnityEngine;

public class HostSingleton : MonoBehaviour
{
    private static HostSingleton _instance;
    private HostGameManager _gameManager;

    public static HostSingleton Instance
    {
        get
        {
            if (_instance != null)
            {
                return _instance;
            }
            _instance = FindFirstObjectByType<HostSingleton>(); // FindFirstObjectByType is a Unity method that finds the first object of the specified type in the scene.
            // we can't use "this" here because this is not a static property.

            if (_instance == null)
            {
                // TO MANUALLY CREATE THE SINGLETON GAMEOBJECT
                /*
                GameObject singletonObject = new GameObject("HostSingleton");
                _instance = singletonObject.AddComponent<HostSingleton>();
                DontDestroyOnLoad(singletonObject);
                */
                Debug.LogError("HostSingleton instance not found in the scene. Please ensure it is present.");
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

    public void CreateHost()
    {
        _gameManager = new HostGameManager();
    }
}
