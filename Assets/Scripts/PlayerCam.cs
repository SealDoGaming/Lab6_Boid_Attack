using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCam : MonoBehaviour
{

    [SerializeField]
    private float sensX = 1f;
    [SerializeField]
    private float sensY = 1f;

    private Transform orientation;

    private float xRotation;
    private float yRotation;

    private InputAction rotAction;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        xRotation = transform.rotation.eulerAngles.x;
        yRotation = transform.rotation.eulerAngles.y;
    }
    public void Initialize(InputAction rotationAction, Transform playerBody)
    {
        this.orientation = playerBody;
        this.rotAction = rotationAction;
        rotAction.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = orientation.position;
        //float mouseX = Inpu
        Vector2 mouseValues =rotAction.ReadValue<Vector2>();
        yRotation += mouseValues.x*sensY;
        xRotation -= mouseValues.y*sensX;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0); 
    }
}
