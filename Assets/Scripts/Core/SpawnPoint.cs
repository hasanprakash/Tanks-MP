using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public static List<SpawnPoint> SpawnPoints { get; private set; } = new List<SpawnPoint>();

    private void OnEnable()
    {
        SpawnPoints.Add(this);
    }

    private void OnDisable()
    {
        SpawnPoints.Remove(this);
    }

    public static Vector3 GetRandomSpawnPosition()
    {
        if (SpawnPoints.Count == 0)
        {
            Debug.LogWarning("No spawn points available.");
            return Vector3.zero;
        }
        else
        {
            int randomIndex = Random.Range(0, SpawnPoints.Count);
            return SpawnPoints[randomIndex].transform.position;
        }
    }

    // gizmos
    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 1);
    }
}
