using Assets.Scripts;
using System;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using Random = UnityEngine.Random;

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
    public float sumFlockFearLevel;
    [HideInInspector]
    public float fearLevel=0;
    [HideInInspector]
    public float fearLevelChange = 0;
    [HideInInspector]
    public int flockSize;
    [HideInInspector]
    public bool isAlreadyDead = false;

    private int loneliness = 0;
    private int spookValue = 0;

    Material boidMaterial;

    private Transform player;

    private void Awake()
    {
        boidMaterial = transform.GetComponentInChildren<MeshRenderer>().material;

    }
    public void Initialize(BoidSettings settings,Transform player)
    {
        this.settings = settings;
        this.player = player;
        position = transform.position;
        forward = transform.forward;

        velocity = transform.forward * (settings.minSpeed+settings.maxSpeed)/2;
    }
    // Update is called once per frame
    public void UpdateBoid()
    {
        Vector3 acceleration = Vector3.zero;
        fearLevel += fearLevelChange;
        if (fearLevel > settings.maxFear)
        {
            fearLevel = settings.maxFear;
        }
        if (player != null)
        {
            Vector3 offsetToPlayer = (player.position - position);
            float playerSight =Mathf.Lerp(settings.minPlayerSightRadius,settings.maxPlayerSightRadius, ((float)fearLevel) / settings.maxFear);
            if (offsetToPlayer.magnitude < playerSight)
            {
                offsetToPlayer.Normalize();
                float fearWeight = Mathf.Lerp(settings.minFearWeight, settings.maxFearWeight, ((float)fearLevel) / settings.maxFear);
                if (CheckObstacle(-offsetToPlayer)){

                    acceleration = CalculateForce(ObstacleRays(-offsetToPlayer)) * fearWeight;
                }
                else
                {
                    acceleration = CalculateForce(-offsetToPlayer) * fearWeight;
                }
            }
        }
        if (flockSize != 0)
        {
            fearLevel = Mathf.Lerp(fearLevel, (sumFlockFearLevel) / flockSize, settings.fearDiffuseRate);
            if (loneliness > 0)
            {
                loneliness-=settings.lonelinessLossRate;
                if (loneliness < 0)
                {
                    loneliness = 0;
                }
            }
            Vector3 avgFlockDir = sumFlockDir / flockSize;
            Vector3 avgSeperationDir = sumSeperationDir / flockSize;
            Vector3 avgsFlockCenter = sumFlockCenter / flockSize;
            Vector3 flockOffset = avgsFlockCenter - position;

            Vector3 flockDirForce = CalculateForce(avgFlockDir) * settings.alignWeight;
            acceleration += flockDirForce; 
            Vector3 seperationForce = CalculateForce(avgSeperationDir) * settings.seperateWeight;
            acceleration += seperationForce;
            Vector3 cohesionForce = CalculateForce(flockOffset) * settings.cohesionWeight;
            acceleration += cohesionForce;


        }
        else
        {
            if (loneliness < settings.maxLoneliness)
            {

                loneliness+=settings.lonelinessGainRate;
            }


        }

        if (CheckObstacle())
        {
            acceleration += CalculateForce(ObstacleRays()) * settings.collisionWeight;
            if (spookValue < 100)
            {
                spookValue += 5;
            }
        }
        else
        {
            if (spookValue > 0)
            {
                spookValue-=3;
                if (spookValue < 0)
                {
                    spookValue=0;   
                }
            }
        }
        acceleration += Random.insideUnitSphere * settings.noiseMagnitude;

        CalcEmotions();
        velocity += acceleration * Time.deltaTime;
        float speed = velocity.magnitude;
        speed = Mathf.Clamp(speed,settings.minSpeed,settings.maxSpeed);
        velocity.Normalize();
        forward = velocity;
        velocity *= speed;

        position += velocity * Time.deltaTime;

        transform.position = position;
        transform.forward = forward;

    }
    public void CalcEmotions()
    {
        Color currentColor = settings.defaultColor;


        currentColor = Color.Lerp(currentColor, settings.sadColor, (float)loneliness / (float)settings.maxLoneliness);
        //currentColor = Color.Lerp(settings.happyColor,currentColor, centerCloseness);
        currentColor = Color.Lerp(currentColor, settings.startledColor, (float)spookValue / 100f);
        currentColor = Color.Lerp(currentColor, settings.fearColor, (float)fearLevel / settings.maxFear);

        SetColour(currentColor);

    }
    public void SetColour(Color col)
    {
        if (boidMaterial != null)
        {
            boidMaterial.color = col;
        }
    }
    private bool CheckObstacle()
    {
        Ray ray = new Ray(position, forward);
        return Physics.SphereCast(ray, settings.raycastRadius, settings.obstacleSightRadius, settings.obstacleMask);
    }
    private bool CheckObstacle(Vector3 dir)
    {
        Ray ray = new Ray(position, dir);
        return Physics.SphereCast(ray, settings.raycastRadius, settings.obstacleSightRadius, settings.obstacleMask);
    }
    private Vector3 ObstacleRays()
    {
        Vector3[] rayDirections = BoidHelper.directions;
        
        for (int i=0; i < rayDirections.Length; i++)
        {
            Vector3 wackDir = rayDirections[i];
            Vector3 dir = transform.TransformDirection(rayDirections[i]);
            Ray ray = new Ray(position, dir);
            if (!Physics.SphereCast(ray, settings.raycastRadius, settings.obstacleSightRadius, settings.obstacleMask)){
                return dir;
            }
        }
        return forward;

    }
    private Vector3 ObstacleRays(Vector3 desiredDirection)
    {
        Vector3[] rayDirections = BoidHelper.directions;
        Quaternion rotation = Quaternion.FromToRotation(transform.forward, desiredDirection);
        for (int i = 0; i < rayDirections.Length; i++)
        {
            Vector3 dir = rotation * transform.TransformDirection(rayDirections[i]);
            Ray ray = new Ray(position, dir);
            if (!Physics.SphereCast(ray, settings.raycastRadius, settings.obstacleSightRadius, settings.obstacleMask))
            {
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
