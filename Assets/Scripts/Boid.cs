using Assets.Scripts;
using System;
using UnityEngine;
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
    public int flockSize;
    [HideInInspector]
    public bool isAlreadyDead = false;

    private int loneliness = 0;
    private int spookValue = 0;
    [SerializeField]
    private GameObject corpseObject;
    private bool deadforreal=false;

    Material boidMaterial;

    private void Awake()
    {
        boidMaterial = transform.GetComponentInChildren<MeshRenderer>().material;

    }
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
        if (isAlreadyDead&&!deadforreal)
        {
            GameObject corpse = GameObject.Instantiate(corpseObject);
            corpse.transform.position = transform.position;
            corpse.transform.rotation = transform.rotation;
            position = new Vector3(99999, 9999, 9999);
            transform.position = position;
            deadforreal= true;
        }
        if (deadforreal)
        {
            return;
        }
        Vector3 acceleration = Vector3.zero;

        if (flockSize != 0)
        {
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
    
    private Vector3 ObstacleRays()
    {
        // TODO: Actually get the proper RAYS
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


    private Vector3 CalculateForce (Vector3 forceDir)
    {
        Vector3 force = forceDir.normalized * settings.maxSpeed - velocity;
        return Vector3.ClampMagnitude(force, settings.maxSpeed);

    }
}
