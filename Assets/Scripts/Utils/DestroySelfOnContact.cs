using UnityEngine;

public class DestroySelfOnContact : MonoBehaviour
{
    // This script will destroy the game object when it collides with another object
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Destroy this game object
        Destroy(gameObject);
    }
}
