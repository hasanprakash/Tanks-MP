using UnityEngine;
using Unity.Netcode;

public class ProjectileLauncher : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private GameObject clientProjectilePrefab;
    [SerializeField] private GameObject serverProjectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;

    [Header("Settings")]
    [SerializeField] private float projectileSpeed = 10f;

    private bool shouldFire = false;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            inputReader.OnAttackEvent += HandleFire;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            inputReader.OnAttackEvent -= HandleFire;
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (!shouldFire) return;

        // Fire the dummy projectile (doesn't deal the damage) at the owner end, to reduce the latency
        SpawnDummyProjectile(projectileSpawnPoint.position, projectileSpawnPoint.up);

        // Fire the server projectile (deals the damage) at the server end
        FireProjectileServerRpc(projectileSpawnPoint.position, projectileSpawnPoint.up);        
    }

    private void HandleFire(bool shouldFire)
    {
        this.shouldFire = shouldFire;
    }

    [ServerRpc]
    private void FireProjectileServerRpc(Vector2 spawnPos, Vector2 spawnDir)
    {
        GameObject projectileInstance = Instantiate(serverProjectilePrefab, spawnPos, Quaternion.identity);
        projectileInstance.transform.up = spawnDir;

        // Other players should see the projectile, so spawing it on all clients (calling from the server)
        SpawnDummyProjectileClientRpc(spawnPos, spawnDir);
    }

    [ClientRpc]
    private void SpawnDummyProjectileClientRpc(Vector2 spawnPos, Vector2 spawnDir)
    {
        if (IsOwner) return;

        GameObject projectileInstance = Instantiate(clientProjectilePrefab, spawnPos, Quaternion.identity);
        projectileInstance.transform.up = spawnDir;
    }

    private void SpawnDummyProjectile(Vector2 spawnPos, Vector2 spawnDir)
    {
        GameObject projectileInstance = Instantiate(clientProjectilePrefab, spawnPos, Quaternion.identity);
        projectileInstance.transform.up = spawnDir;
    }
}

