using System.Threading.Tasks;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientGameManager
{
    public const string MAIN_MENU_SCENE = "Menu"; 

    // LOGIC TO INTERACT WITH UNITY RELAY
    public async Task<bool> InitAsync()
    {
        await UnityServices.InitializeAsync();

        AuthState isAuthenticated = await AuthenticationWrapper.DoAuth(5); // 5 is the number of tries to authenticate the player.
        if (isAuthenticated == AuthState.Authenticated)
        {
            Debug.Log("Player authenticated successfully.");
            // Proceed with the game logic, such as connecting to a server or starting the game.
            return true;
        }
        else
        {
            Debug.LogError("Player authentication failed.");
            // Handle authentication failure, such as showing an error message or retrying.
            return false;
        }
    }
    /*
    Making a server call to authenticate the player.
    This is done using the Unity Relay service, which allows us to connect players to a game server.
    We are using async call because, server call takes some time to complete, we will get response as callback after some time.
    We don't want to block the main thread while waiting for the response.
    */

    public void GoToMainMenu()
    {
        SceneManager.LoadScene(MAIN_MENU_SCENE);
    }
}
