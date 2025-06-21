using System.Collections.Generic;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbiesList : MonoBehaviour
{
    [SerializeField] private LobbyItem lobbyItemPrefab;
    [SerializeField] private Transform lobbyListContainer;
    private bool isJoining = false;
    private bool isRefreshing = false;
    private Lobby joiningLobby;
    private string joinCode;

    public async void JoinLobbyAsync(Lobby lobby)
    {
        if (isJoining) return;

        isJoining = true;

        try
        {
            joiningLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id);
            Debug.Log($"Joined lobby: {lobby.Name}");

            joinCode = joiningLobby.Data["JoinCode"].Value;
            await ClientSingleton.Instance.GameManager.StartClientAsync(joinCode);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to join lobby: {e.Message}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to join lobby: {e.Message}");
        }
        finally
        {
            isJoining = false;
        }
    }

    private void OnEnable()
    {
        // get the list of lobbies and populate the UI
        RefreshLobbyList();
    }

    public async void RefreshLobbyList()
    {
        if (isRefreshing) return;
        isRefreshing = true;

        try
        {
            QueryResponse lobbies = await LobbyService.Instance.QueryLobbiesAsync(new QueryLobbiesOptions
            {
                Count = 20,
                Filters = new List<QueryFilter>()
                {
                    new QueryFilter(
                        field: QueryFilter.FieldOptions.AvailableSlots,
                        op: QueryFilter.OpOptions.GT,
                        value: "0"
                    ),
                    new QueryFilter(
                        field: QueryFilter.FieldOptions.IsLocked,
                        op: QueryFilter.OpOptions.EQ,
                        value: "0"
                    )
                }
            });

            // Clear existing lobby items
            foreach (Transform child in lobbyListContainer)
            {
                Destroy(child.gameObject);
            }

            // Populate the lobby list with new items
            foreach (Lobby lobby in lobbies.Results)
            {
                var lobbyItem = Instantiate(lobbyItemPrefab, lobbyListContainer);
                lobbyItem.Initialize(this, lobby);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to query lobbies: {e.Message}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to query lobbies: {e.Message}");
        }
        finally
        {
            isRefreshing = false;
        }
    }
}
