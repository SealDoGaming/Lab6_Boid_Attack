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

    [Header("Collisions")]
    public LayerMask obstacleMask;
    public float raycastRadius = 0.3f;
    public float obstacleSightRadius = 0.3f;

    [Header("Boid Noise")]
    [Tooltip("Determines intensity of noise")]
    public float noiseMagnitude = 0.1f;


    [Header("Boid Loneliness")]
    [Tooltip("Determines max loneliness value before the boid is completely sad")]
    public int maxLoneliness = 100;
    [Tooltip("Determines how quickly the boid becomes lonely")]
    public int lonelinessGainRate = 1;

    [Tooltip("Determines how quickly the boid becomes not-lonely after joining a flock")]
    public int lonelinessLossRate = 2;



    //[Tooltip("Determines how far the boids consider being at the \"center\" of a flock extends, with smaller flocks having a smaller center area")]
    //public float centerSizeCoefficient = 0.5f;

    [Header("Boid Colors")]
    [Tooltip("Determines the default boid color")]
    public Color defaultColor = Color.cyan;
    [Tooltip("Determines color the boid turns if it is too close to an obstacle")]
    public Color startledColor = Color.yellow;
    //public Color happyColor = Color.green;
    [Tooltip("Determines color the boid turns if it is without a flock for too long")]
    public Color sadColor = Color.blue;



}
