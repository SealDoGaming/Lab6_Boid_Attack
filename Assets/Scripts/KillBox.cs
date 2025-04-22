using UnityEngine;

public class KillBox : MonoBehaviour
{
    // Start is called 
    private void OnTriggerEnter(Collider other)
    {
        Boid bob;
        if (other.TryGetComponent<Boid>(out bob))
        {
            bob.isAlreadyDead = true;
        }
    }
}
