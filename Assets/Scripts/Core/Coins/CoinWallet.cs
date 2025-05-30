using UnityEngine;
using Unity.Netcode;

public class CoinWallet : NetworkBehaviour
{
    NetworkVariable<int> TotalCoins = new NetworkVariable<int>(0);
    
    private int coinValue;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.TryGetComponent(out Coin coin)) return;
        
        coinValue = coin.Collect();
        if (IsServer)
        {
            TotalCoins.Value += coinValue;
            Debug.Log($"Total Coins: {TotalCoins.Value}");
        }
}
}
