using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    Canvas canvas;
    private TextMeshProUGUI textUI;
    int deadBoidCount = 0;
    int initialBoidCount = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        boids = ((Boid[])FindObjectsByType(typeof(Boid),FindObjectsSortMode.None)).ToList<Boid>();
        foreach(Boid boido in boids)
        {
            boido.Initialize(settings,player);
        }
        canvas = GetComponentInChildren<Canvas>();
        textUI = canvas.GetComponentInChildren<TextMeshProUGUI>();
        initialBoidCount = boids.Count;
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
