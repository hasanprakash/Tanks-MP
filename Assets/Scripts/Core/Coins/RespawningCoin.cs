using System;
using NUnit.Framework;
using UnityEngine;

public class RespawningCoin : Coin
{
    public event Action<RespawningCoin> OnCollected;
    private Vector3 previousPosition;

    private void Update()
    {
        if (!IsClient) return;

        if (transform.position != previousPosition)
        {
            previousPosition = transform.position;
            Show(true);
        }
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

        OnCollected?.Invoke(this);

        return coinValue;
    }

    public void Reset()
    {
        isCollected = false;
    }

}
