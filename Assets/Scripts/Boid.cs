using System;
using UnityEngine;

public class Boid : MonoBehaviour
{
    BoidSettings settings;

    [HideInInspector]
    public Vector3 position;
    [HideInInspector]
    public Vector3 forward;
    [HideInInspector]
    private Vector3 velocity;


    [HideInInspector]
    public Vector3 sumFlockDir;
    [HideInInspector]
    public Vector3 sumSeperationDir;
    [HideInInspector]
    public Vector3 sumFlockCenter;
    [HideInInspector]
    public int flockSize;


    public void Initialize(BoidSettings settings)
    {
        this.settings = settings;
        position = transform.position;
        forward = transform.forward;

        velocity = transform.forward * (settings.minSpeed+settings.maxSpeed)/2;
    }
    // Update is called once per frame
    public void UpdateBoid()
    {
        Vector3 acceleration = Vector3.zero;

        if (flockSize != 0)
        {
            Vector3 avgFlockDir = sumFlockDir / flockSize;
            Vector3 avgSeperationDir = sumSeperationDir / flockSize;
            Vector3 avgsFlockCenter = sumFlockCenter / flockSize;
            Vector3 flockOffset = avgsFlockCenter - position;

            acceleration += CalculateForce(avgFlockDir) * settings.alignWeight;
            acceleration += CalculateForce(avgSeperationDir) * settings.seperateWeight;
            acceleration += CalculateForce(flockOffset) * settings.cohesionWeight;


        }

        if (CheckObstacle())
        {
            acceleration += CalculateForce(ObstacleRays()) * settings.collisionWeight;
        }

        velocity += acceleration * Time.deltaTime;
        float speed = velocity.magnitude;
        speed = Mathf.Clamp(speed,settings.minSpeed,settings.maxSpeed);
        velocity.Normalize();
        forward = velocity;
        velocity *= speed;

        position = velocity * Time.deltaTime;

        transform.position = position;
        transform.forward = forward;

    }

    private bool CheckObstacle()
    {
        Ray ray = new Ray(position, forward);
        return Physics.SphereCast(ray, settings.raycastRadius, settings.obstacleSightRadius, settings.obstacleMask);
    }
    
    private Vector3 ObstacleRays()
    {
        Vector3[] rayDirections = new Vector3[3];
        
        for (int i=0; i < rayDirections.Length; i++)
        {
            Vector3 dir = transform.TransformDirection(rayDirections[i]);
            Ray ray = new Ray(position, dir);
            if (!Physics.SphereCast(ray, settings.raycastRadius, settings.obstacleSightRadius, settings.obstacleMask){
                return dir;
            }
        }
        return forward;

    }


    private Vector3 CalculateForce (Vector3 forceDir)
    {
        Vector3 force = forceDir.normalized * settings.maxSpeed - velocity;
        return Vector3.ClampMagnitude(force, settings.maxSpeed);

    }
}
