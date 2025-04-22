using UnityEngine;

public class PlayerRun : PlayerMovement
{
    // Update is called once per frame
    [Header("Movement")]
    [SerializeField]
    private float speed = 10f;
    [SerializeField]
    private float maxSpeed = 10f;
    [SerializeField]
    private float groundDrag=5f;
    [SerializeField]
    private float skiddingDrag = 5f;
    [SerializeField]
    private float airDrag = 0f;
    [Header("Jump/Jetpack")]
    [SerializeField]
    private float jumpForce=10f;    
    [SerializeField]
    private float jetpackAcceleration = 8f;
    private bool jumping = false;

    [Header("Ground Collision")]

    [SerializeField]
    private float playerHeight;
    [SerializeField]
    private LayerMask groundLayers;

    private bool grounded = false;


    private Rigidbody rb;
    private ParticleSystem jetpackParticles;
    private ParticleSystem.EmissionModule jetpackParticleEmission;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        jetpackParticles= GetComponentInChildren<ParticleSystem>();
        jetpackParticleEmission = jetpackParticles.emission;
        jetpackParticleEmission.enabled = false;
    }
    void FixedUpdate()
    {

        this.transform.rotation = playerCamera.Orientation.rotation;
        Vector2 movementDir = moveAction.ReadValue<Vector2>();


        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, groundLayers);
        if (grounded && !jumping)
        {
            if (movementDir != Vector2.zero)
            {

                rb.linearDamping = groundDrag;
            }
            else
            {
                rb.linearDamping = skiddingDrag;
            }
        }
        else
        {
            rb.linearDamping = 0;

        }
        Jump();
        float forwardMovement = movementDir.y * speed;
        float sideMovement = movementDir.x * speed;
        Vector3 offset = new Vector3(sideMovement, 0, forwardMovement);
        //Vector3 newVelocity = this.transform.rotation * offset;
        //newVelocity.y = rb.linearVelocity.y;
        //transform.position += this.transform.rotation*offset * Time.fixedDeltaTime;
        rb.AddRelativeForce(offset*5f, ForceMode.Force);
        //rb.linearVelocity = newVelocity;
        //rb.MovePosition(transform.position + this.transform.rotation * offset * Time.fixedDeltaTime);
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        if (horizontalVelocity.magnitude>maxSpeed)
        {
            horizontalVelocity = horizontalVelocity.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(horizontalVelocity.x,rb.linearVelocity.y,horizontalVelocity.z);
        }
    }
    private void Jump()
    {
        if (this.jumpAction.ReadValue<float>()>0)
        {
            if (grounded)
            {
                rb.AddForce(jumpForce * Vector3.up, ForceMode.Impulse);
            }
            else
            {
                Debug.Log(Physics.gravity);
                rb.AddForce(jetpackAcceleration * Vector3.up, ForceMode.Acceleration);
                jetpackParticleEmission.enabled = true;

            }
            jumping = true;

        }
        else
        {
            jumping = false;
            jetpackParticleEmission.enabled = false;
        }
    }
}
