using UnityEngine;

public class BoidManager : MonoBehaviour
{
    const int threadGroupSize = 1024;

    public BoidSettings settings;
    public ComputeShader compute;
    [SerializeField]
    private Boid[] boids;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach(Boid boido in boids)
        {
            boido.Initialize(settings);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Create boidData array and set each boid position and dir

        //Create Compute buffer using data
        //set computer buffer and other inputs

        int threadGroups = Mathf.CeilToInt(numBoids / (float)threadGroupSize);
        compute.Dispatch(0, threadGroups, 1, 1);

        boidBuffer.GetData(boidData);
        //use boidData to set the various calculated things of each boid and then update that boid
        

        //release the buffer.
    }


    public struct BoidData
    {
        public Vector3 position;
        public Vector3 direction;

        public Vector3 flockHeading;
        public Vector3 flockCentre;
        public Vector3 avoidanceHeading;
        public int numFlockmates;

        public static int Size
        {
            get
            {
                return sizeof(float) * 3 * 5 + sizeof(int);
            }
        }
    }
}
