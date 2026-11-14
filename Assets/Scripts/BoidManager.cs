using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class BoidManager : MonoBehaviour
{
    const int threadGroupSize = 1024;

    public BoidSettings settings;
    public ComputeShader compute;
    private List<Boid> boids;
    [SerializeField]
    private GameObject corpseObject;
    [SerializeField]
    private Transform player;

    [SerializeField]
    private ShopMechanic storeManager;

    private Queue<Boid> deadBoids = new Queue<Boid>();


    private List<(Transform target,float radius,float weight)> boidTargets = new List<(Transform,float,float)>();

    public Transform Player
    {
        get
        {
            return player;
        }
    }

    public List<(Transform target, float radius, float weight)> BoidTargets
    {
        get
        {
            return boidTargets;
        }
    }
    public float BoidPopulation
    {
        get
        {
            return (float)(initialBoidCount-deadBoids.Count)/initialBoidCount;
        }
    }


    [SerializeField]
    private TextMeshProUGUI boidCountText;
    [SerializeField]
    private TextMeshProUGUI winText;
    [SerializeField]
    private TextMeshProUGUI marketValueText;
    int deadBoidCount = 0;
    int initialBoidCount = 0;

    //Vector3 bottomCorner = Vector3.zero;
    //Vector3 topCorner = Vector3.zero;

    private Vector3 regionOffset;
    private Vector3Int gridSizes;
    private int[] localOffsetArray;
    private int numRegions;




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateBoidGrid();
        boids = ((Boid[])FindObjectsByType(typeof(Boid),FindObjectsSortMode.None)).ToList<Boid>();
        foreach(Boid boido in boids)
        {
            boido.Initialize(settings,this);
        }
        initialBoidCount = boids.Count;


    }
    // Update is called once per frame
    void Update()
    {
        //Create boidData array and set each boid position and dir
        //Boid[] boidArray = boidMatrix.SelectMany(i => i).ToArray();
        for(int i = 0; i < boids.Count; i++)
        {
            if (boids[i].isAlreadyDead)
            {
                GameObject corpse = GameObject.Instantiate(corpseObject);
                corpse.transform.position = boids[i].transform.position;
                corpse.transform.rotation = boids[i].transform.rotation;
                //RemoveFromLocalGrid(boidArray[i]);
                boids.Remove(boids[i]);
                Destroy(boids[i].gameObject);
                deadBoidCount++;
                i--;
            }
        }
        int numBoids = boids.Count;
        if (numBoids <= 0)
        {
            return;
        }
        int numTargets = boidTargets.Count;

        TargetData[] targetData = new TargetData[numTargets];
        for(int i = 0; i < targetData.Length; i++)
        {
            targetData[i].position = boidTargets[i].target.position;
            targetData[i].radius = boidTargets[i].radius;
            targetData[i].weight = boidTargets[i].weight;
        }

        BoidData[] boidData = new BoidData[numBoids];

        int[] gridCounts = new int[numRegions];
        for (int i = 0; i < boidData.Length; i++)
        {
            gridCounts[boids[i].localGridIndex] += 1;

        }
        ComputeBuffer targetBuffer;
        //Create Compute buffer using data
        if (numTargets > 0)
        {
            targetBuffer = new ComputeBuffer(numTargets, TargetData.Size);
            targetBuffer.SetData(targetData);

        }
        else
        {
            targetBuffer = new ComputeBuffer(1, TargetData.Size);

        }
        //set computer buffer and other inputs
        //Create Compute buffer using data
        compute.SetBuffer(0, "targets", targetBuffer);
        compute.SetInt("numTargets", numTargets);
        //set computer buffer and other inputs
        boidBuffer.SetData(boidData);
        indexBuffer.SetData(matrixCounts);
        localOffsetBuffer.SetData(localOffsetArray);
        compute.SetBuffer(0,"boids",boidBuffer);
        compute.SetBuffer(0, "indexBuffer", indexBuffer);
        compute.SetBuffer(0, "localOffsetBuffer", localOffsetBuffer);
        compute.SetInt("numBoids", numBoids);
        compute.SetInt("gridXMax", gridSizes.x);
        compute.SetInt("gridZMax", gridSizes.z);
        compute.SetInt("gridYMax", gridSizes.y);
        compute.SetFloat("viewRadius",settings.perceptionRadius);
        compute.SetFloat("avoidRadius", settings.avoidanceRadius);
        compute.SetFloat("maxFear", settings.maxFear);

        //Things to Add to ComputeBuffer
        //code that takes the localgrid index and checks each valid localgrid
        //"localOffsetBuffer


        int threadGroups = Mathf.CeilToInt(numBoids / (float)threadGroupSize);
        compute.Dispatch(0, threadGroups, 1, 1);

        boidBuffer.GetData(boidData);
        //use boidData to set the various calculated things of each boid and then update that boid
        int length = boidData.Length;

        for(int i = 0; i < length; i++)
            boids[i].sumFlockDir = boidData[i].flockHeading;
            boids[i].sumSeperationDir = boidData[i].avoidanceHeading;
            boids[i].sumFlockCenter = boidData[i].flockCentre;
            boids[i].flockSize = boidData[i].numFlockmates;
            boids[i].fearLevelChange =boidData[i].fearLevelChange;
            boids[i].sumFlockFearLevel = boidData[i].flockFearLevel;

            boids[i].sumTargetDir = boidData[i].targetDir;
            //boids[i].targetsCount = boidData[i].targetCount;

            if (!boids[i].isAlreadyDead)
            {
                boids[i].UpdateBoid();
            }
            else if(boids[i].isActiveAndEnabled)
            if (boids[i].isAlreadyDead)
            {
                continue;
                corpse.transform.position = boids[i].transform.position;
                corpse.transform.rotation = boids[i].transform.rotation;
                deadBoids.Enqueue(boids[i]);
                boids[i].gameObject.SetActive(false);
               
                storeManager.AddCash(Mathf.FloorToInt(settings.boidValue.Evaluate(Mathf.InverseLerp(0, initialBoidCount, deadBoids.Count))));
                //Destroy(boids[i].gameObject);
                //boids.RemoveAt(i);

                UpdateLocalGrid(boids[i]);

            }
}

        //release the buffer.
        targetBuffer.Release();
        boidBuffer.Release();
        UpdateUI();
    }
    private void UpdateUI()
    {
        if (deadBoidCount >= initialBoidCount)
        {
            winText.gameObject.SetActive(true);

        }
        else
        {
            boidCountText.text = (initialBoidCount - deadBoids.Count).ToString();

        }
        marketValueText.text = "Market Value: $" + Mathf.FloorToInt(settings.boidValue.Evaluate(Mathf.InverseLerp(0, initialBoidCount, deadBoids.Count)));
    }

    public void AddTarget(Transform target, float radius, float weight)
    {
        boidTargets.Add((target, radius, weight));
    }
    public void RemoveTarget(Transform target)
    {
        boidTargets.RemoveAll((hit) => hit.target == target);
    }
    public void RemoveTarget(Transform target,float radius,float weight)
    {
        boidTargets.Remove((target,radius,weight));
    }
    public void RespawnBoid(Vector3 position, Vector3 forward)
    {
        if (deadBoids.Count > 0)
        {
            Boid reincarnatedBoid = deadBoids.Dequeue();

            reincarnatedBoid.Initialize(position, forward, settings, this, 0f);
        }
        Debug.Log("BREED");
    }


    public struct TargetData
    {
        public Vector3 position;
        public float radius;
        public float weight;

        public static int Size
        {
            get
            {
                return sizeof(float) * 3 * 1 + sizeof(float) * 2;
            }
        }


    }
        public struct BoidData
    {
        public Vector3 position;
        public Vector3 direction;

        public Vector3 flockHeading;
        public Vector3 flockCentre;
        public Vector3 avoidanceHeading;

        public Vector3 targetDir;
        //public int targetCount;

        public float fearLevel;
        public float fearLevelChange;
        public float flockFearLevel;


        public int numFlockmates;

        public int isDead;

        public int localGridIndex;

        public static int Size
        {
            get
                return sizeof(float) * 3 * 6 + sizeof(float)*3 + sizeof(int)*2;
                return sizeof(float) * 3 * 5+sizeof(float)*3 + sizeof(int)*2;
            }
        }
    }
}
