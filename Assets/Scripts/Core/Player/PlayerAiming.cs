using UnityEngine;
using Unity.Netcode;
public class PlayerAiming : NetworkBehaviour
{
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Transform turrentTransform;

    // LateUpdate is used to ensure that the turrent is updated after the player has moved (Normal Update funtion also would work, but this is a good practice)
    public void LateUpdate()
    {
        if (!IsOwner) return;

        Vector2 aimScreenPosition = inputReader.AimPosition;
        Vector2 aimWorldPosition = Camera.main.ScreenToWorldPoint(aimScreenPosition);
        turrentTransform.up = aimWorldPosition - (Vector2)turrentTransform.position;
        /*
        rough reason behind this subtraction:
        - turrentTransform.position is the position of the turrent in world space
        - aimWorldPosition is the position of the mouse in world space
        - Meaning, world position of the center of the screen would be (0, 0)
        - Imagine, turrent is at position (-5, 0), mouse is pointing at (1, 6)
        - If you assign the mouse's world position to turrent transform.up, it would be pointing at (-4, 6) - ((1, 6) to it's own position)
        - so we need to subtract the turrent position from the mouse position in order to shift center of the screen to the turrent position
        */
    }
}
