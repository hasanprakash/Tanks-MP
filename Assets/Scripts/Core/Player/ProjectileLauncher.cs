using UnityEngine;
using Unity.Netcode;

public class ProjectileLauncher : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private GameObject clientProjectilePrefab;
    [SerializeField] private GameObject serverProjectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private GameObject muzzleFlash;
    [SerializeField] private Collider2D playerCollider;

    [Header("Settings")]
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float fireRate = 0.5f; // how many seconds it takes to fire a projectile
    [SerializeField] private float muzzleFlashDuration = 0.1f;

    private bool shouldFire = false;
    private float lastFireTime = 0f;
    private float muzzleFlashTimer = 0f;

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
        if (muzzleFlashTimer > 0)
        {
            muzzleFlashTimer -= Time.deltaTime;
            if (muzzleFlashTimer <= 0)
            {
                muzzleFlash.SetActive(false);
            }
        }

        if (!IsOwner) return;

        if (!shouldFire) return;

        if (Time.time < lastFireTime + fireRate) return;
        lastFireTime = Time.time;

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
        Physics2D.IgnoreCollision(projectileInstance.GetComponent<Collider2D>(), playerCollider);

        muzzleFlash.SetActive(true);
        muzzleFlashTimer = muzzleFlashDuration;

        // Other players should see the projectile, so spawing it on all clients (calling from the server)
        SpawnDummyProjectileClientRpc(spawnPos, spawnDir);
    }

    [ClientRpc]
    private void SpawnDummyProjectileClientRpc(Vector2 spawnPos, Vector2 spawnDir)
    {
        if (IsOwner) return;

        // Spawn the projectile on all clients except the owner
        SpawnDummyProjectile(spawnPos, spawnDir);
    }

    private void SpawnDummyProjectile(Vector2 spawnPos, Vector2 spawnDir)
    {
        muzzleFlash.SetActive(true);
        muzzleFlashTimer = muzzleFlashDuration;

        GameObject projectileInstance = Instantiate(clientProjectilePrefab, spawnPos, Quaternion.identity);
        projectileInstance.transform.up = spawnDir;

        Physics2D.IgnoreCollision(projectileInstance.GetComponent<Collider2D>(), playerCollider);

        if (projectileInstance.TryGetComponent<DealDamageOnContact>(out DealDamageOnContact dealDamage))
        {
            dealDamage.SetOwner(OwnerClientId);
        }

        if (projectileInstance.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
        {
            rb.linearVelocity = spawnDir * projectileSpeed;
            // linear velocity is a Vector2, in the direction where the velocity is applied
            // velocity is applied to the rigidbody, so that multiplying to delta time is not needed
            /* 
            SIDE NOTE:
            speed is a scalar quantity,
            velocity is a vector quantity, which has both magnitude and direction (speed of object and it's direction)
            direction * speed => velocity
            */
            Debug.Log("Velocity: " + rb.linearVelocity);
        }
        else
        {
            Debug.LogError("Projectile does not have a Rigidbody2D component.");
        }
    }
}

