using UnityEngine;
using UnityEngine.InputSystem;

public abstract class PlayerMovement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is createdprivate PlayerControls inputScheme;
    private PlayerControls inputScheme;
    [SerializeField]
    protected PlayerCam playerCamera;

    protected InputAction moveAction;
    private void Awake()
    {
        inputScheme = new PlayerControls();

        InputAction lookAction = inputScheme.PlayerInput1.Look;
        this.moveAction = inputScheme.PlayerInput1.Move;


        playerCamera.Initialize(lookAction, this.transform);
        moveAction.Enable();

    }
}
