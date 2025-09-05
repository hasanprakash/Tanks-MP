using UnityEngine;

public class BountyCoin : Coin
{
    [SerializeField] private float lifetime = 30f; // lifetime in seconds before the coin is destroyed

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public override int Collect()
    {
        if (!IsServer)
        {
            Show(false);
            return 0;
        }
        if (isCollected) return 0;

        isCollected = true;

        Destroy(gameObject);

        return coinValue;
    }
}
