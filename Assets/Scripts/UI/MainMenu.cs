using System;
using TMPro;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private TMP_Text queueStatusText;
    [SerializeField] private TMP_Text queueTimerText;
    [SerializeField] private TMP_Text findMatchButtonText;
    [SerializeField] private TMP_InputField joinCodeText;

    private bool isMatchmaking = false;
    private bool isCancelling = false;

    private void Start()
    {
        if (ClientSingleton.Instance == null) { return; }

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

        queueStatusText.text = string.Empty;
        queueTimerText.text = string.Empty;
        findMatchButtonText.text = "Find Match";
    }

    public async void FindMatch()
    {
        if (isCancelling) return;

        if (isMatchmaking)
        {
            queueStatusText.text = "Cancelling...";
            isCancelling = true;
        
            await ClientSingleton.Instance.GameManager.CancelMatchmaking();

            isCancelling = false;
            isMatchmaking = false;
            findMatchButtonText.text = "Find Match";
            queueTimerText.text = string.Empty;
            return;
        }

        ClientSingleton.Instance.GameManager.MatchmakeAsync(OnMatchMade);
        findMatchButtonText.text = "Cancel";
        queueStatusText.text = "Searching...";
        isMatchmaking = true;
    }

    private void OnMatchMade(MatchmakerPollingResult result)
    {
        switch (result)
        {
            case MatchmakerPollingResult.Success:
                queueStatusText.text = "Match found! Joining...";
                break;
            case MatchmakerPollingResult.TicketCreationError:
                queueStatusText.text = "Error creating ticket. Please try again.";
                break;
            case MatchmakerPollingResult.TicketCancellationError:
                queueStatusText.text = "Error cancelling ticket. Please try again.";
                break;
            case MatchmakerPollingResult.TicketRetrievalError:
                queueStatusText.text = "Error retrieving ticket. Please try again.";
                break;
            case MatchmakerPollingResult.MatchAssignmentError:
                queueStatusText.text = "Error assigning match. Please try again.";
                break;
            default:
                queueStatusText.text = "An unknown error occurred. Please try again.";
                break;
        }
    }

    public async void StartHost()
    {
        await HostSingleton.Instance.GameManager.StartHostAsync();
    }

    public async void StartClient()
    {
        await ClientSingleton.Instance.GameManager.StartClientAsync(joinCodeText.text);
    }
}
