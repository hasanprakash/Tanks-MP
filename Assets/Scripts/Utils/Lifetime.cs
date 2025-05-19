using UnityEngine;

public class Lifetime : MonoBehaviour
{
    [SerializeField] private float lifetime = 1f;
    void Start()
    {
        // Destroy the game object after 5 seconds
        Destroy(gameObject, lifetime);
    }
}
