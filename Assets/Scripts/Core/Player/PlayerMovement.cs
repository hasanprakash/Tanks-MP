using UnityEngine;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Rigidbody2D playerRigidbody;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float turningRate = 30f;

    private Vector2 previousMovementInput;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        inputReader.OnMoveEvent += HandleMove;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;

        inputReader.OnMoveEvent -= HandleMove;
    }

    private void Update()
    {
        if (!IsOwner) return;

        Move();
        Rotate();
    }

    private void Move()
    {
        /* ONE WAY TO DO IT */
        /*Vector2 moveDirection = playerTransform.up * previousMovementInput.y;
        Vector2 newPosition = playerTransform.position + (Vector3)moveDirection * (moveSpeed * Time.deltaTime);
        playerRigidbody.MovePosition(newPosition);*/

        /* ANOTHER WAY TO DO IT */
        playerRigidbody.linearVelocity = moveSpeed * previousMovementInput.y * (Vector2) playerTransform.up;
    }

    private void Rotate()
    {
        float zRotation = previousMovementInput.x * -turningRate * Time.deltaTime;
        playerTransform.Rotate(0, 0, zRotation);
    }

    private void HandleMove(Vector2 moveInput)
    {
        previousMovementInput = moveInput;
    }
}
