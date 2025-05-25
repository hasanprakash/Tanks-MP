using UnityEngine;

public class DestroySelfOnContact : MonoBehaviour
{
    // This script will destroy the game object when it collides with another object
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Destroy this game object
        Debug.Log($"Destroying {gameObject.name} on contact with {other.name}");
        Destroy(gameObject);
    }
}
