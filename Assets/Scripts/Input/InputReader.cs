using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static Controls;

[CreateAssetMenu(fileName = "New Input Reader", menuName = "Input/Input Reader", order = 0)]
public class InputReader : ScriptableObject, IPlayerActions
{
    public event Action<bool> OnAttackEvent;
    public event Action<Vector2> OnMoveEvent;
    // not using events here because, position will be updated more frequenctly, we don't want to spam events
    public Vector2 AimPosition { get; private set; }
    private Controls controls;
    public void OnEnable()
    {
        if (controls == null)
        {
            controls = new Controls();
            controls.Player.SetCallbacks(this);
        }
        controls.Player.Enable();
    }

    public void OnDisable()
    {
        controls.Player.Disable();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        OnMoveEvent?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnAttackEvent?.Invoke(true);
        }
        else if (context.canceled)
        {
            OnAttackEvent?.Invoke(false);
        }
    }

    public void OnAim(InputAction.CallbackContext context)
    {
        AimPosition = context.ReadValue<Vector2>();
    }
}