using UnityEngine;
using Unity.Netcode;

public class CoinWallet : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Health health;
    [SerializeField] private BountyCoin coinPrefab;

    [Header("Settings")]
    [SerializeField] private float coinSpread = 3f;
    [SerializeField] private float bountyPercentage = 50f; // percentage of total coins to drop as bounty
    [SerializeField] private int bountyCoinCount = 10;
    [SerializeField] private int minBountyCoinValue = 5;
    [SerializeField] private LayerMask layerMask;

    public NetworkVariable<int> TotalCoins = new NetworkVariable<int>(0);

    private int coinValue;
    private float coinRadius;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) { return; }

        coinRadius = coinPrefab.GetComponent<CircleCollider2D>().radius;

        health.OnDie += HandleDie;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) { return; }

        health.OnDie -= HandleDie;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.TryGetComponent(out Coin coin)) return;

        coinValue = coin.Collect();
        if (IsServer)
        {
            TotalCoins.Value += coinValue;
        }
    }
    public void SpendCoins(int amount) // will be called from ServerRpc (as network variable can only be changed on the server)
    {
        TotalCoins.Value -= amount;
    }

    private void HandleDie(Health health)
    {
        int totalBountyValue = (int)(TotalCoins.Value * (bountyPercentage / 100f));
        int bountyPerCoinValue = totalBountyValue / bountyCoinCount;

        if (bountyPerCoinValue < minBountyCoinValue) return; // don't drop any coins if the value per coin is too low

        for (int i = 0; i < bountyCoinCount; i++)
        {
            BountyCoin coinInstance = Instantiate(
                coinPrefab,
                GetSpawnPoint(),
                Quaternion.identity);

            coinInstance.SetValue(bountyPerCoinValue); 
            coinInstance.NetworkObject.Spawn();
        }
    }

    private Vector2 GetSpawnPoint()
    {
        while (true)
        {
            Vector2 spawnPoint = (Vector2)transform.position + Random.insideUnitCircle * coinSpread;

            Collider2D hit = Physics2D.OverlapCircle(spawnPoint, coinRadius, layerMask);
            if (hit == null)
            {
                return spawnPoint;
            }
        }
    }

}
