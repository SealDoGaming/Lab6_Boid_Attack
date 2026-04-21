using NUnit.Framework;
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
    private List<List<Boid>> boidMatrix;
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


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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

        GenerateBoidGrid();
        AssignBoidsToGrid(boids);


    }
    private void GenerateBoidGrid()
    {
        float regionSize = 2*settings.perceptionRadius;
        Vector3 size = boidActiveZoneSize;
        float numRegionsX = Mathf.Ceil(size.x/ regionSize)+2;
        float numRegionsY = Mathf.Ceil(size.y/ regionSize)+2;
        float numRegionsZ = Mathf.Ceil(size.z/ regionSize)+2;
        boidMatrix = new List<List<Boid>>();
        for (int i=0;i<numRegionsX*numRegionsX*numRegionsZ;i++)
        {
            boidMatrix.Add(new List<Boid>());
        }
        gridSizes = new Vector3Int((int) numRegionsX, (int) numRegionsY, (int) numRegionsZ);
        //i = y * (Z_MAX * X_MAX) + Z * (X_MAX) + x
        regionOffset = new Vector3(numRegionsX,numRegionsY,numRegionsZ) *-regionSize / 2;

    }
    private void AssignBoidsToGrid(List<Boid> boids)
    {
        for (int i = 0; i < boids.Count; i++)
        {
            Vector3 boidPos = boids[i].transform.position;
            Vector3 regionPos = (boidPos - regionOffset) / (2 * settings.perceptionRadius);

            int value = Mathf.FloorToInt(regionPos.y) * (gridSizes.z * gridSizes.x) + Mathf.FloorToInt(regionPos.z) * (gridSizes.x) + Mathf.FloorToInt(regionPos.x);
            boidMatrix[value].Add(boids[i]);
            boids[i].localGridIndex = value;
        }
    }
    private void UpdateLocalGrid(Boid boid)
    {
        Vector3 boidPos = boid.transform.position;
        Vector3 regionPos = (boidPos - regionOffset) / (2 * settings.perceptionRadius);
        int oldValue = boid.localGridIndex;
        int value = Mathf.FloorToInt(regionPos.y) * (gridSizes.z * gridSizes.x) + Mathf.FloorToInt(regionPos.z) * (gridSizes.x) + Mathf.FloorToInt(regionPos.x);

        boidMatrix[oldValue].Remove(boid);
        boidMatrix[value].Add(boid);
        boid.localGridIndex = value;
    }

    // Update is called once per frame
    void Update()
    {
        //Create boidData array and set each boid position and dir

        int numBoids =boids.Count;
        if (numBoids <= 0)
        {
            return;
        }
        BoidData[] boidData = new BoidData[numBoids];
        for(int i=0;i<boidData.Length; i++)
        {
            boidData[i].position = boids[i].position;
            boidData[i].direction = boids[i].forward;
            boidData[i].fearLevel = boids[i].fearLevel;
            boidData[i].isDead = boids[i].isAlreadyDead?1:0;
        }
        int[] matrixCounts = new int[boidMatrix.Count+1];
        int count = 0;

        for (int i = 0; i < matrixCounts.Length; i++)
        {

            matrixCounts[i] = count;
            if (i < matrixCounts.Length - 1)
            {
                count += boidMatrix[i].Count;

            }
            else
            {
                count = numBoids;
            }
            

        }

        boidMatrix.SelectMany(i => i);
        //Create Compute buffer using data
        ComputeBuffer boidBuffer = new ComputeBuffer(numBoids, BoidData.Size);
        //set computer buffer and other inputs
        boidBuffer.SetData(boidData);
        compute.SetBuffer(0,"boids",boidBuffer);
        compute.SetInt("numBoids", numBoids);
        compute.SetFloat("viewRadius",settings.perceptionRadius);
        compute.SetFloat("avoidRadius", settings.avoidanceRadius);
        compute.SetFloat("maxFear", settings.maxFear);



        int threadGroups = Mathf.CeilToInt(numBoids / (float)threadGroupSize);
        compute.Dispatch(0, threadGroups, 1, 1);

        boidBuffer.GetData(boidData);
        //use boidData to set the various calculated things of each boid and then update that boid
        int length = boidData.Length;
        for(int i = 0; i < length; i++)
        {
            boids[i].sumFlockDir = boidData[i].flockHeading;
            boids[i].sumSeperationDir = boidData[i].avoidanceHeading;
            boids[i].sumFlockCenter = boidData[i].flockCentre;
            boids[i].flockSize = boidData[i].numFlockmates;
            boids[i].fearLevelChange =boidData[i].fearLevelChange;
            boids[i].sumFlockFearLevel = boidData[i].flockFearLevel;
            if (boids[i].isAlreadyDead)
            {
                GameObject corpse = GameObject.Instantiate(corpseObject);
                corpse.transform.position = boids[i].transform.position;
                corpse.transform.rotation = boids[i].transform.rotation;
                Destroy(boids[i].gameObject);
                boids.RemoveAt(i);
                deadBoidCount++;
                i--;
                length--;
            }
            else
            {

                boids[i].UpdateBoid();
                UpdateLocalGrid(boids[i]);

            }
}

        //release the buffer.
        boidBuffer.Release();
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

        public static int Size
        {
            get
            {
                return sizeof(float) * 3 * 5+sizeof(float)*3 + sizeof(int)*2;
            }
        }
    }
}
