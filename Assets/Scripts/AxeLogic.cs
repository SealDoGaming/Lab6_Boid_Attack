using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class AxeLogic : MonoBehaviour
{

    public InputAction axeSwing;

    public AudioSource audio;

    [SerializeField]
     WeightedAudio[] playableAudio;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Animation axeAnimation;
    private bool isAttacking=false;

    private bool useAudio = true;
    private bool didKillBoid = false;
    private bool didPlay = false;


    private WeightedAudioSelecter randomAudioList;
    private void Awake()
    {
        axeAnimation = GetComponentInChildren<Animation>();
        axeSwing.performed += TriggerAxe;
        axeSwing.Enable();
        randomAudioList = new WeightedAudioSelecter(playableAudio);

    }

    // Update is called once per frame
    void Update()
    {
        if(isAttacking&&!axeAnimation.isPlaying)
        {
            isAttacking = false;
            didKillBoid = false;
            didPlay = false;
        }
        if(useAudio&&!didPlay&& isAttacking && didKillBoid)
        {
            audio.PlayOneShot(randomAudioList.RandomClip());
            didPlay = true;
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
            didKillBoid = true;

        }
    }
}
