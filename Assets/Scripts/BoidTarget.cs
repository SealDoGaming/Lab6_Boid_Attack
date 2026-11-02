using UnityEditor;
using UnityEngine;

[RequireComponent (typeof(SphereCollider))]
public class BoidTarget : MonoBehaviour
{

    [SerializeField]
    private float detectRadius = 5f;

    [SerializeField]
    private float weight = 1f;


    [SerializeField]
    private float consumeRadius = 1f;

    [SerializeField]
    private float health = 10f;

    [SerializeField]
    private float boidConsumptionRate = 0.1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        BoidManager manager = GameObject.FindFirstObjectByType<BoidManager>();
        GetComponent<SphereCollider>().radius = consumeRadius;
        manager.AddTarget(transform, detectRadius, weight);


    }

    // Update is called once per frame
    void Update()
    {
        if (health <= 0)
        {
            BoidManager manager = GameObject.FindFirstObjectByType<BoidManager>();
            manager.RemoveTarget(transform,detectRadius,weight);
            Destroy(gameObject);
        }
        
    }
    private void OnDestroy()
    {
        
    }


    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Boid"))
        {
            health -= boidConsumptionRate*Time.fixedDeltaTime;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere (transform.position, consumeRadius);
    }

}
