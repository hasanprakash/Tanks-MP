using TMPro;
using Unity.Netcode;
using UnityEngine;

public class LeaderboardEntityDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text displayText;
    [SerializeField] private Color myColor;

    private string playerName;
    public ulong ClientId { get; private set; }
    public int Coins { get; private set; }

    public void Initialize(ulong clientId, string playerName, int coins)
    {
        ClientId = clientId;
        this.playerName = playerName;

        if (ClientId == NetworkManager.Singleton.LocalClientId)
        {
            displayText.color = myColor;
        }

        UpdateCoins(coins);
    }

    public void UpdateCoins(int newCoins)
    {
        Coins = newCoins;
        UpdateText();
    }

    public void UpdateText()
    {
        displayText.text = $"{transform.GetSiblingIndex() + 1}. {playerName} ({Coins})";
    }
}
