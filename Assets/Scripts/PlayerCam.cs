using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCam : MonoBehaviour
{

    [SerializeField]
    protected Vector3 cameraOffset = Vector3.zero;
    [SerializeField]
    private float sensX = 1f;
    [SerializeField]
    private float sensY = 1f;

    public Transform Orientation { get; private set; }

    private float xRotation;
    private float yRotation;

    private InputAction rotAction;
    private InputAction attackAction;
    private AxeLogic axe;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        axe = GetComponentInChildren<AxeLogic>();
    }
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        xRotation = transform.rotation.eulerAngles.x;
        yRotation = transform.rotation.eulerAngles.y;
    }
    public void Initialize(InputAction rotationAction, InputAction attackAction, Transform playerBody)
    {
        this.Orientation = playerBody;
        this.rotAction = rotationAction;
        this.attackAction = attackAction;
        rotAction.Enable();
        this.attackAction.performed += OnAttack;
        attackAction.Enable();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = Orientation.position + cameraOffset;
        //float mouseX = Inpu
        Vector2 mouseValues = rotAction.ReadValue<Vector2>();
        yRotation += mouseValues.x * sensY;
        xRotation -= mouseValues.y * sensX;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        Orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    private void OnAttack(InputAction.CallbackContext thingy)
    {
        axe.SwingAxe();
    }
}
