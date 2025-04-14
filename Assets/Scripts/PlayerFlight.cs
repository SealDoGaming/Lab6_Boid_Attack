using UnityEngine;

public class PlayerFlight : PlayerMovement
{
    // Update is called once per frame
    [SerializeField]
    private float speed = 10f;
    void FixedUpdate()
    {
        this.transform.rotation = playerCamera.transform.rotation;
        Vector2 movementDir = moveAction.ReadValue<Vector2>();

        float forwardMovement = movementDir.y* speed;
        float sideMovement = movementDir.x* speed;
        Vector3 offset = new Vector3(sideMovement, 0, forwardMovement);
        transform.position += this.transform.rotation*offset * Time.fixedDeltaTime;
    }
}
