using TMPro;
using UnityEngine;

public class LeaderboardEntityDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text displayText;

    private string playerName;
    public ulong ClientId { get; private set; }
    public int Coins { get; private set; }

    public void Initialize(ulong clientId, string playerName, int coins)
    {
        ClientId = clientId;
        this.playerName = playerName;

        UpdateCoins(coins);
    }

    public void UpdateCoins(int newCoins)
    {
        Coins = newCoins;
        UpdateText();
    }

    private void UpdateText()
    {
        displayText.text = $"1. {playerName} ({Coins})";
    }
}
