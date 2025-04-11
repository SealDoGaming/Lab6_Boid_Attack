using UnityEngine;

[CreateAssetMenu(fileName = "BoidSettings", menuName = "Scriptable Objects/BoidSettings")]
public class BoidSettings : ScriptableObject
{
    public float minSpeed = 2;
    public float maxSpeed = 4;
    public float perceptionRadius = 3;
    public float avoidanceRadius = 1;
    public float maxTurnForce = 3;


    public float alignWeight = 1;
    public float cohesionWeight = 1;
    public float seperateWeight = 1;
    public float collisionWeight = 10;


    public LayerMask obstacleMask;
    public float raycastRadius = 0.3f;
    public float obstacleSightRadius = 0.3f;

}
