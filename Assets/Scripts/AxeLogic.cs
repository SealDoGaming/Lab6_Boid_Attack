using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.IMGUI.Controls.PrimitiveBoundsHandle;
using static UnityEngine.InputSystem.InputAction;

public class AxeLogic : MonoBehaviour
{

    public InputAction axeSwing;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Animation axeAnimation;
    private bool isAttacking=false;
    private void Awake()
    {
        axeAnimation = GetComponentInChildren<Animation>();
        axeSwing.performed += TriggerAxe;
        axeSwing.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        if(isAttacking&&!axeAnimation.isPlaying)
        {
            isAttacking = false;
        }
    }
    private void TriggerAxe(CallbackContext c)
    {
        SwingAxe();
    }
    public void SwingAxe()
    {
        if (!axeAnimation.isPlaying)
        {
            axeAnimation.Play();
            isAttacking = true;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<Boid>(out var boid))
        {
            KillBoid(boid);

        }


    }
    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.TryGetComponent<Boid>(out var boid))
        {
            KillBoid(boid);

        }

        
    }
    private void KillBoid(Boid doomedBoid)
    {
        //Debug.Log("Killed1");
        if (isAttacking&&!doomedBoid.isAlreadyDead)
        {
            //Debug.Log("Killed2");
            doomedBoid.isAlreadyDead = true;
        }
    }
}
