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

    private async void FindMatch()
    {
        if (isCancelling) return;

        if (isMatchmaking)
        {
            queueStatusText.text = "Cancelling...";
            isCancelling = true;
            // cancel queue
            isCancelling = false;
            isMatchmaking = false;
            findMatchButtonText.text = "Find Match";
            queueTimerText.text = string.Empty;
            return;
        }

        // start queue
        findMatchButtonText.text = "Cancel";
        queueStatusText.text = "Searching...";
        isMatchmaking = true;
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
