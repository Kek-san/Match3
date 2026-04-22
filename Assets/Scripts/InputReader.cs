using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour {
    PlayerInput playerInput;
    InputAction selectAction;
    InputAction fireAction;

    public event Action Fire;

    public Vector2 Selected => selectAction.ReadValue<Vector2>();

    private void Start() {
        playerInput = GetComponent<PlayerInput>();
        selectAction = playerInput.actions["Select"];
        fireAction = playerInput.actions["Fire"];

        fireAction.performed += OnFire;
    }

    private void OnDestroy() {
        fireAction.performed -= OnFire;
    }

    private void OnFire(InputAction.CallbackContext context) => Fire?.Invoke();
}
