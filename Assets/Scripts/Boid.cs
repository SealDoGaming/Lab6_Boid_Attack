using Assets.Scripts;
using NUnit.Framework;
using System;
using System.Collections.Generic;
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
    public Vector3 sumTargetDir;
    //[HideInInspector]
    //public int targetsCount;



    [HideInInspector]
    public int flockSize;
    [HideInInspector]
    public bool isAlreadyDead = false;
    [HideInInspector]
    public Vector3Int localGridPos;
    [HideInInspector]
    public int localGridIndex;


    private int loneliness = 0;
    private int spookValue = 0;


    private float age = 0;
    private bool adult = false;
    private float pregnancy = 0f;


    Material boidMaterial;

    //private Transform player;
    private health hp;

    private BoidManager manager;


    bool intersecting = false; 
    private void Awake()
    {
        boidMaterial = transform.GetComponentInChildren<MeshRenderer>().material;

    }
    public void Initialize(BoidSettings settings, BoidManager manager)
    {
        Initialize(settings, manager, settings.maxAge);
    }
    public void Initialize(BoidSettings settings,BoidManager manager,float age)
    {
        Initialize(transform.position, transform.forward, settings, manager, age);
    }
    public void Initialize(Vector3 position, Vector3 forward, BoidSettings settings, BoidManager manager, float age)
    {
        this.settings = settings;
        //this.player = player;
        this.manager = manager;
        this.position = position;
        this.forward = forward;
        hp = GetComponent<health>();
        velocity = transform.forward * (settings.minSpeed + settings.maxSpeed) / 2;
        fearLevel = 5;
        if (age >= settings.maxAge)
        {
            adult = true;
        }
    }
    // Update is called once per frame
    public void UpdateBoid()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        Vector3 tempVelocity = rb.linearVelocity;
        if (tempVelocity.magnitude > 0.01f)
        {
            velocity = tempVelocity;
            float speedy = velocity.magnitude;
            speedy = Mathf.Clamp(speedy, settings.minSpeed, settings.maxSpeed);
            velocity.Normalize();
            forward = velocity;
            velocity *= speedy;
        }
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        Transform player = manager.Player;
        Vector3 acceleration = Vector3.zero;
        fearLevel += fearLevelChange;
        if (fearLevel > settings.maxFear)
        {
            fearLevel = settings.maxFear;
        }



        if (player != null)
        {
            Vector3 offsetToPlayer = (player.position - position);
            float playerSight = Mathf.Lerp(settings.minPlayerSightRadius, settings.maxPlayerSightRadius, ((float)fearLevel) / settings.maxFear);
            if (offsetToPlayer.magnitude < playerSight)
            {
                offsetToPlayer.Normalize();
                float fearWeight = Mathf.Lerp(settings.minFearWeight, settings.maxFearWeight, ((float)fearLevel) / settings.maxFear);
                if (CheckObstacle(-offsetToPlayer))
                {

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

            //Vector3 avgTargetDir = sumTargetDir;

            Vector3 targetForce = CalculateForce(sumTargetDir,false);
            acceleration += targetForce;

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
            if (intersecting)
            {
                acceleration += CalculateForce(ObstacleRays(settings.obstacleSightRadius*1.5f)) * settings.collisionWeight*2;
                intersecting = false;
            }
            else
            {
                acceleration += CalculateForce(ObstacleRays()) * settings.collisionWeight;
            }
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


        float maxAge = settings.maxAge;
        if (!adult)
        {
            settings.ageScaling.Evaluate(Mathf.InverseLerp(0, maxAge, age));
            if (age > settings.maxAge)
            {
                adult = true;
                pregnancy = 0f;

            }
        }
        else if (settings.enableBreeding&&flockSize > 0)
        {

            float breedingRate = settings.breedingRate.Evaluate(Mathf.InverseLerp(settings.minBreedingPopulation, settings.maxBreedingPopulation, flockSize));
            float overcrowding = settings.populationCurbing.Evaluate(manager.BoidPopulation);
            pregnancy += breedingRate * overcrowding * Time.deltaTime;
            if (pregnancy > settings.breedingThreshold)
            {
                manager.RespawnBoid(position, -transform.up);
                pregnancy = 0;
            }
        }
        age += Time.deltaTime;



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
        return ObstacleRays(0f);

    }
    private Vector3 ObstacleRays(float offset)
    {
        Vector3[] rayDirections = BoidHelper.directions;
        Vector3 lastDir = forward;
        float lastDist = 0f;
        for (int i = 0; i < rayDirections.Length; i++)
        {
            Vector3 wackDir = rayDirections[i];
            Vector3 dir = transform.TransformDirection(rayDirections[i]);
            Ray ray = new Ray(position+dir*offset, dir);

            if (!Physics.SphereCast(ray, settings.raycastRadius, out var hit, settings.obstacleSightRadius, settings.obstacleMask, QueryTriggerInteraction.Collide))
            {
                return dir;
            }
            else if (hit.distance > lastDist)
            {
                lastDir = dir;
                lastDist = hit.distance;
            }
        }
        return lastDir;

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
    private Vector3 CalculateForce(Vector3 forceDir, bool capForce)
    {
        if (capForce)
        {
            Vector3 force = forceDir.normalized * settings.maxSpeed - velocity;
            return Vector3.ClampMagnitude(force, settings.maxTurnForce);
        }
        else
        {
            Vector3 force = forceDir;
            return force;
        }
    }

    /*
    private void FixedUpdate()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
    }
    private void OnCollisionStay(Collision collision)
    {
        if ((settings.obstacleMask & (1 << collision.gameObject.layer)) == 1 << collision.gameObject.layer)
        {
            intersecting = true;

            Rigidbody rb = GetComponent<Rigidbody>();
            rb.isKinematic = false;

        }
    }*/
}
