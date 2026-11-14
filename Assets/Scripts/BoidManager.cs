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
    //private List<List<Boid>> boidMatrix;
    [SerializeField]
    private GameObject corpseObject;
    [SerializeField]
    private Transform player;

    [SerializeField]
    private Canvas canvas;
    private TextMeshProUGUI textUI;
    int deadBoidCount = 0;
    int initialBoidCount = 0;

    //Vector3 bottomCorner = Vector3.zero;
    //Vector3 topCorner = Vector3.zero;
    [SerializeField]
    private Vector3 boidActiveZoneSize = Vector3.one;

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
            boido.Initialize(settings,player);
        }
        if (!canvas)
        {
            canvas = GetComponentInChildren<Canvas>();
        }
        textUI = canvas.GetComponentInChildren<TextMeshProUGUI>();
        initialBoidCount = boids.Count;

        AssignBoidsToGrid(boids);
        GenerateLocalOffsetArray();

    }
    private void GenerateBoidGrid()
    {
        float regionSize = 2*settings.perceptionRadius;
        Vector3 size = boidActiveZoneSize;
        float numRegionsX = Mathf.Ceil(size.x/ regionSize)+2;
        float numRegionsY = Mathf.Ceil(size.y/ regionSize)+2;
        float numRegionsZ = Mathf.Ceil(size.z/ regionSize)+2;
        gridSizes = new Vector3Int((int) numRegionsX, (int) numRegionsY, (int) numRegionsZ);
        //i = y * (Z_MAX * X_MAX) + Z * (X_MAX) + x
        regionOffset = new Vector3(numRegionsX, numRegionsY, numRegionsZ) * -regionSize / 2 + transform.position ;
        numRegions = gridSizes.x * gridSizes.z * gridSizes.y;

    }
    private void AssignBoidsToGrid(List<Boid> boids)
    {
        for (int i = 0; i < boids.Count; i++)
        {
            Vector3 boidPos = boids[i].transform.position;
            Vector3 regionPos = (boidPos - regionOffset) / (2 * settings.perceptionRadius);

            int value = Mathf.FloorToInt(regionPos.y) * (gridSizes.z * gridSizes.x) + Mathf.FloorToInt(regionPos.z) * (gridSizes.x) + Mathf.FloorToInt(regionPos.x);
            //boidMatrix[value].Add(boids[i]);
            boids[i].localGridIndex = value;
            Vector3 size = new Vector3(gridSizes.x, gridSizes.y, gridSizes.z);
            Debug.Log(boidPos + " vs " + regionOffset+"and"+ size * (2 * settings.perceptionRadius) + " to "+(regionOffset+size * (2 * settings.perceptionRadius)));
            //Debug.Log(gridSizes.x + ", " + gridSizes.y + ", " + gridSizes.z + " vs " + Mathf.FloorToInt(regionPos.x) + ", " + Mathf.FloorToInt(regionPos.y) + ", " + Mathf.FloorToInt(regionPos.z));
        }
    }
    private void UpdateLocalGrid(Boid boid)
    {
        Vector3 boidPos = boid.transform.position;
        Vector3 regionPos = (boidPos - regionOffset) / (2 * settings.perceptionRadius);
        int value = Mathf.FloorToInt(regionPos.y) * (gridSizes.z * gridSizes.x) + Mathf.FloorToInt(regionPos.z) * (gridSizes.x) + Mathf.FloorToInt(regionPos.x);
        
        //boidMatrix[oldValue].Remove(boid);
        //boidMatrix[value].Add(boid);
        boid.localGridIndex = value;
    }

    private void GenerateLocalOffsetArray()
    {
        int Z_MAX = gridSizes.z;
        int X_MAX = gridSizes.x;

        int[] array = new int[27];
        for(int i = 0; i < 27; i++)
        {
            array[i] = 0;
        }
        for(int i = 0; i < 9; i++)
        {
            //i = y * (Z_MAX * X_MAX) + Z * (X_MAX) + x
            array[i] -= (Z_MAX * X_MAX);
        }
        for (int i = 18; i < 27; i++)
        {
            //i = y * (Z_MAX * X_MAX) + Z * (X_MAX) + x
            array[i] += (Z_MAX * X_MAX);
        }
        for(int i = 0; i < 27; i += 9)
        {
            array[i] -= X_MAX;
            array[i + 1] -= X_MAX;
            array[i + 2] -= X_MAX;
        }
        for (int i = 6; i < 27; i += 9)
        {
            array[i] += X_MAX;
            array[i + 1] += X_MAX;
            array[i + 2] += X_MAX;
        }



        for (int i = 0; i < 27; i += 3)
        {
            array[i] -= 1;
        }
        for(int i = 2;i < 27; i += 3)
        {
            array[i] += 1;
        }
        localOffsetArray = array;
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
        BoidData[] boidData = new BoidData[numBoids];

        int[] gridCounts = new int[numRegions];
        for (int i = 0; i < boidData.Length; i++)
        {
            gridCounts[boids[i].localGridIndex] += 1;

        }
        int[] matrixCounts = new int[gridCounts.Length];
        int count = 0;

        for (int i = 0; i < matrixCounts.Length; i++)
        {

            matrixCounts[i] = count;
            if (i < matrixCounts.Length - 1)
            {
                count += gridCounts[i];

            }
            else
            {
                count = numBoids;
            }


        }
        int[] matrixCountsClone = new int[gridCounts.Length];
        matrixCounts.CopyTo(matrixCountsClone,0);
        int[] map = new int[boidData.Length];
        for (int i=0;i<boidData.Length; i++)
        {
            int index = matrixCountsClone[boids[i].localGridIndex];

            boidData[index].position = boids[i].position;
            boidData[index].direction = boids[i].forward;
            boidData[index].fearLevel = boids[i].fearLevel;
            boidData[index].isDead = boids[i].isAlreadyDead?1:0;
            boidData[index].localGridIndex = boids[i].localGridIndex;
            map[index] = i;
            matrixCountsClone[boids[i].localGridIndex]++;
            
        }

        //Create Compute buffer using data
        ComputeBuffer boidBuffer = new ComputeBuffer(numBoids, BoidData.Size);
        ComputeBuffer indexBuffer= new ComputeBuffer(matrixCounts.Length, sizeof(int));
        ComputeBuffer localOffsetBuffer = new ComputeBuffer(localOffsetArray.Length, sizeof(int));
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
        {
            boids[map[i]].sumFlockDir = boidData[i].flockHeading;
            boids[map[i]].sumSeperationDir = boidData[i].avoidanceHeading;
            boids[map[i]].sumFlockCenter = boidData[i].flockCentre;
            boids[map[i]].flockSize = boidData[i].numFlockmates;
            boids[map[i]].fearLevelChange =boidData[i].fearLevelChange;
            boids[map[i]].sumFlockFearLevel = boidData[i].flockFearLevel;
            if (boids[map[i]].isAlreadyDead)
            {
                continue;
                GameObject corpse = GameObject.Instantiate(corpseObject);
                corpse.transform.position = boids[map[i]].transform.position;
                corpse.transform.rotation = boids[map[i]].transform.rotation;
                //RemoveFromLocalGrid(boidArray[i]);
                boids.Remove(boids[map[i]]);
                Destroy(boids[map[i]].gameObject);
                deadBoidCount++;
                i--;
                length--;
            }
            else
            {
                Debug.Log(i + " ->" + map[i]);
                boids[map[i]].UpdateBoid();
                UpdateLocalGrid(boids[map[i]]);

            }
}

        //release the buffer.
        boidBuffer.Release();
        indexBuffer.Release();
        localOffsetBuffer.Release();
        UpdateUI();
    }
    private void UpdateUI()
    {
        if (deadBoidCount >= initialBoidCount)
        {
            textUI.rectTransform.position = canvas.pixelRect.center;
            textUI.color = Color.green;
            textUI.text = "YOU WIN!";

        }
        else
        {

            textUI.text = "Boids Slain: " + deadBoidCount + "/" + initialBoidCount;
        }
    }

    void OnDrawGizmosSelected()
    {
        Vector3 pos = transform.position;
        Vector3 size = boidActiveZoneSize;
        Gizmos.color = new Color(255, 165, 0);
        Gizmos.DrawWireCube(pos, size);


    }
    public struct BoidData
    {
        public Vector3 position;
        public Vector3 direction;

        public Vector3 flockHeading;
        public Vector3 flockCentre;
        public Vector3 avoidanceHeading;

        public float fearLevel;
        public float fearLevelChange;
        public float flockFearLevel;


        public int numFlockmates;

        public int isDead;

        public int localGridIndex;

        public static int Size
        {
            get
            {
                return sizeof(float) * 3 * 5+sizeof(float)*3 + sizeof(int)*3;
            }
        }
    }
}
