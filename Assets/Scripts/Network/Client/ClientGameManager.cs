using System.Threading.Tasks;
using UnityEngine;

public class ClientGameManager
{
    // LOGIC TO INTERACT WITH UNITY RELAY
    public async Task InitAsync()
    {
        // Authenticate the Player
    }
    /*
    We will be making a server call to authenticate the player.
    This will be done using the Unity Relay service, which allows us to connect players to a game server.
    We are using async call because, server call takes some time to complete, we will get response as callback after some time.
    We don't want to block the main thread while waiting for the response.
    */
}
