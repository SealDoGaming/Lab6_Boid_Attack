using UnityEngine;
using static UnityEditor.IMGUI.Controls.PrimitiveBoundsHandle;

public class AxeLogic : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Animation axeAnimation;
    private bool isAttacking=false;
    private void Awake()
    {
        axeAnimation = GetComponentInChildren<Animation>();
        
    }

    // Update is called once per frame
    void Update()
    {
        if(isAttacking&&!axeAnimation.isPlaying)
        {
            isAttacking = false;
        }
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
        KillBoid(other.gameObject.GetComponent<Boid>());


    }
    private void OnTriggerStay(Collider other)
    {
        KillBoid(other.gameObject.GetComponent<Boid>());

        
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
