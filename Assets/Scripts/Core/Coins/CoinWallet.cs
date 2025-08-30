using UnityEngine;
using Unity.Netcode;

public class CoinWallet : NetworkBehaviour
{
    public NetworkVariable<int> TotalCoins = new NetworkVariable<int>(0);

    private int coinValue;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.TryGetComponent(out Coin coin)) return;

        coinValue = coin.Collect();
        if (IsServer)
        {
            TotalCoins.Value += coinValue;
        }
    }
    public void SpendCoins(int amount) // wiil be called from ServerRpc (as network variable can only be changed on the server)
    {
        TotalCoins.Value -= amount;
    }
}
